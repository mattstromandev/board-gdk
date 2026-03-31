using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Board.Core;

using BE.Emulator.Actions;
using BE.Emulator.Data;
using BE.Emulator.Persistence;
using BE.Emulator.Utility;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator.Editor.Settings
{
internal sealed class EmulatorSettingsProvider : SettingsProvider
{
    public const string RootSettingsProviderPath = "Project/Board Enthusiasts/Emulator";

    private const string AlignedFieldUssClassName = "unity-base-field__aligned";
    private const string BaseFieldLabelUssClassName = "unity-base-field__label";
    private const string PropertyFieldLabelUssClassName = "unity-property-field__label";
    private const string ApplicationPropertyPath = "m_data.m_application";
    private const string ProfilesPropertyPath = "m_data.m_profiles";
    private const string SessionPropertyPath = "m_data.m_session";
    private const string SaveDataPropertyPath = "m_data.m_saveGames";
    private const string StoragePropertyPath = "m_data.m_storage";
    private const string LastNonGuestSessionPlayerTooltip = "Session must included at least one non-guest player";

    private static readonly IReadOnlyDictionary<string, string> TooltipByPropertyName = new Dictionary<string, string>
    {
        ["m_applicationName"] = "The application name shown in the Board pause screen.",
        ["m_showSaveOptionUponExit"] = "Whether Board should offer a save prompt when exiting through the pause screen.",
        ["m_customButtons"] = "The custom buttons shown in the Board pause screen.",
        ["m_audioTracks"] = "The audio tracks shown in the Board pause screen.",
        ["id"] = "The unique identifier returned for this Board pause-screen item.",
        ["text"] = "The label shown for this Board pause-screen custom button.",
        ["iconName"] = "The icon shown for this Board pause-screen custom button.",
        ["name"] = "The label shown for this Board audio track.",
        ["value"] = "The Board audio-track value, from 0 to 100.",
        ["m_playerId"] = "The persistent Board player identifier returned for this mock profile or saved player.",
        ["m_displayName"] = "The display name returned for this mock item.",
        ["m_avatarId"] = "The avatar identifier returned for this mock item.",
        ["m_type"] = "The Board player type returned for this mock item.",
        ["m_avatar"] = "The avatar texture returned for this mock item.",
        ["m_avatarBackgroundColor"] = "The internally persisted avatar background color used by the emulator UI for this item.",
        ["m_activeProfileId"] = "The system-wide active profile returned by the Board Session API.",
        ["m_players"] = "The player data returned by this mock Board surface.",
        ["m_sessionId"] = "The internal emulator session-slot identifier for this player mapping.",
        ["m_description"] = "The save description returned for this mock save.",
        ["m_playedTime"] = "The played-time value, in seconds, returned for this mock save.",
        ["m_gameVersion"] = "The game-version string returned for this mock save.",
        ["m_coverImage"] = "The cover image returned for this mock save.",
        ["m_payload"] = "The payload bytes returned when this mock save is loaded.",
        ["m_maxPayloadSize"] = "The maximum payload size, in bytes, returned by the Board save-game API.",
        ["m_maxAppStorage"] = "The total application storage, in bytes, returned by the Board save-game API.",
        ["m_maxSaveDescriptionLength"] = "The maximum save-description length returned by the Board save-game API."
    };

    private readonly EmulatorMockDataAsset _mockDataAsset;
    private readonly string _sectionDescription;
    private readonly string _serializedPropertyPath;

    private SerializedObject _serializedMockDataAsset;
    private bool _rebuildScheduled;
    private bool _isRebuildingSection;
    private int _suppressedAssetChangedEventsRemaining;
    private string _lastSectionStructureSignature = string.Empty;
    private VisualElement _sectionRoot;
    private VisualElement _titleActionHost;
    private Toggle _safeSpaceToggle;

    private EmulatorSettingsProvider(
        string path,
        string label,
        string serializedPropertyPath,
        string sectionDescription,
        EmulatorMockDataAsset mockDataAsset,
        IEnumerable<string> keywords)
        : base(path, SettingsScope.Project, keywords)
    {
        this.label = label;
        _serializedPropertyPath = serializedPropertyPath;
        _sectionDescription = sectionDescription;
        _mockDataAsset = mockDataAsset;
    }

    public static EmulatorSettingsProvider CreateApplication(
        EmulatorMockDataAsset mockDataAsset,
        IEnumerable<string> keywords)
    {
        return new EmulatorSettingsProvider(
            path: RootSettingsProviderPath + "/Application",
            label: "Application",
            serializedPropertyPath: ApplicationPropertyPath,
            sectionDescription: "Configure mock application and pause-menu state returned through the Board application APIs.",
            mockDataAsset: mockDataAsset,
            keywords: keywords);
    }

    public static EmulatorSettingsProvider CreateProfiles(
        EmulatorMockDataAsset mockDataAsset,
        IEnumerable<string> keywords)
    {
        return new EmulatorSettingsProvider(
            path: RootSettingsProviderPath + "/Profiles",
            label: "Profiles",
            serializedPropertyPath: ProfilesPropertyPath,
            sectionDescription: "Manage the persisted mock Board profiles, display names, avatars, and profile metadata.",
            mockDataAsset: mockDataAsset,
            keywords: keywords);
    }

    public static EmulatorSettingsProvider CreateSession(
        EmulatorMockDataAsset mockDataAsset,
        IEnumerable<string> keywords)
    {
        return new EmulatorSettingsProvider(
            path: RootSettingsProviderPath + "/Session",
            label: "Session",
            serializedPropertyPath: SessionPropertyPath,
            sectionDescription: "Configure the active Board session state, including the active profile and the players currently in the session.",
            mockDataAsset: mockDataAsset,
            keywords: keywords);
    }

    public static EmulatorSettingsProvider CreateSaveData(
        EmulatorMockDataAsset mockDataAsset,
        IEnumerable<string> keywords)
    {
        return new EmulatorSettingsProvider(
            path: RootSettingsProviderPath + "/Save Data",
            label: "Save Data",
            serializedPropertyPath: SaveDataPropertyPath,
            sectionDescription: "Inspect and edit the persisted mock save-game payloads and the players associated with each save.",
            mockDataAsset: mockDataAsset,
            keywords: keywords);
    }

    public static EmulatorSettingsProvider CreateStorage(
        EmulatorMockDataAsset mockDataAsset,
        IEnumerable<string> keywords)
    {
        return new EmulatorSettingsProvider(
            path: RootSettingsProviderPath + "/Storage",
            label: "Storage",
            serializedPropertyPath: StoragePropertyPath,
            sectionDescription: "Set the mock Board storage limits used when validating save payloads and descriptions.",
            mockDataAsset: mockDataAsset,
            keywords: keywords);
    }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        EmulatorProjectSettingsBridge.SetActiveMockDataAsset(_mockDataAsset);
        RefreshSerializedMockDataAsset();

        NormalizeAuthoringDataIfNeeded();

        rootElement.Clear();
        rootElement.style.flexGrow = 1f;
        rootElement.AddToClassList(InspectorElement.ussClassName);

        VisualElement content = new();
        content.style.flexGrow = 1f;
        content.style.paddingLeft = 16f;
        content.style.paddingRight = 16f;
        content.style.paddingTop = 12f;
        content.style.paddingBottom = 16f;
        content.AddToClassList(InspectorElement.ussClassName);

        VisualElement titleRow = new();
        titleRow.style.flexDirection = FlexDirection.Row;
        titleRow.style.justifyContent = Justify.SpaceBetween;
        titleRow.style.alignItems = Align.Center;
        titleRow.style.marginBottom = 8f;

        Label title = new(label);
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.fontSize = 15f;
        titleRow.Add(title);

        _titleActionHost = new VisualElement();
        _titleActionHost.style.flexDirection = FlexDirection.Row;
        _titleActionHost.style.alignItems = Align.Center;
        _titleActionHost.style.flexShrink = 0f;
        titleRow.Add(_titleActionHost);
        content.Add(titleRow);

        HelpBox description = new(_sectionDescription, HelpBoxMessageType.None)
        {
            style =
            {
                marginBottom = 10f
            }
        };
        content.Add(description);

        _sectionRoot = new VisualElement
        {
            style =
            {
                flexGrow = 1f
            }
        };
        content.Add(_sectionRoot);
        rootElement.Add(content);

        RebuildSection();
        RefreshTitleActionHost();
        _mockDataAsset.Changed += OnMockDataAssetChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        base.OnActivate(searchContext, rootElement);
    }

    public override void OnDeactivate()
    {
        _mockDataAsset.Changed -= OnMockDataAssetChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.delayCall -= RebuildSectionDelayed;
        PersistMockDataAssetChanges();
        _rebuildScheduled = false;
        _sectionRoot?.Unbind();
        _sectionRoot = null;
        _safeSpaceToggle = null;
        _titleActionHost = null;
        _serializedMockDataAsset = null;
        _lastSectionStructureSignature = string.Empty;
        base.OnDeactivate();
    }

    public override void OnInspectorUpdate()
    {
        _serializedMockDataAsset?.UpdateIfRequiredOrScript();

        if(HasSectionStructureChanged())
        {
            ScheduleRebuildSection();
        }

        if(_safeSpaceToggle != null && _safeSpaceToggle.value != EmulatorExternalViewActionBridge.IsSafeSpaceVisible)
        {
            _safeSpaceToggle.SetValueWithoutNotify(EmulatorExternalViewActionBridge.IsSafeSpaceVisible);
        }

        base.OnInspectorUpdate();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        RefreshTitleActionHost();
    }

    private void RefreshTitleActionHost()
    {
        if(_titleActionHost == null)
        {
            return;
        }

        _safeSpaceToggle = null;
        _titleActionHost.Clear();

        if(_serializedPropertyPath != ApplicationPropertyPath || !Application.isPlaying)
        {
            return;
        }

        const string safeSpaceTooltip = "Show the emulator safe-space guide overlay while the game is running.";

        VisualElement toggleRow = new();
        toggleRow.style.flexDirection = FlexDirection.Row;
        toggleRow.style.alignItems = Align.Center;
        toggleRow.style.flexShrink = 0f;
        toggleRow.tooltip = safeSpaceTooltip;

        Label toggleLabel = new("Show Safe Space");
        toggleLabel.style.marginRight = 6f;
        toggleLabel.tooltip = safeSpaceTooltip;
        toggleRow.Add(toggleLabel);

        Toggle safeSpaceToggle = new();
        safeSpaceToggle.tooltip = safeSpaceTooltip;
        safeSpaceToggle.label = string.Empty;
        safeSpaceToggle.style.flexShrink = 0f;
        safeSpaceToggle.SetValueWithoutNotify(EmulatorExternalViewActionBridge.IsSafeSpaceVisible);
        safeSpaceToggle.RegisterValueChangedCallback(OnSafeSpaceToggleValueChanged);
        _safeSpaceToggle = safeSpaceToggle;
        toggleRow.Add(safeSpaceToggle);
        _titleActionHost.Add(toggleRow);
    }

    private static void OnSafeSpaceToggleValueChanged(ChangeEvent<bool> eventArgs)
    {
        EmulatorExternalViewActionBridge.Request(new SetSafeSpaceVisibilityViewAction
        {
            IsVisible = eventArgs.newValue
        });
    }

    private void RebuildSection()
    {
        if(_sectionRoot == null)
        {
            return;
        }

        _serializedMockDataAsset.UpdateIfRequiredOrScript();
        _isRebuildingSection = true;
        try
        {
            _sectionRoot.Unbind();
            _sectionRoot.Clear();
            _sectionRoot.Add(CreateSectionContent());
            _lastSectionStructureSignature = CaptureSectionStructureSignature();
        }
        finally
        {
            _isRebuildingSection = false;
        }
    }

    private void ScheduleRebuildSection()
    {
        if(_sectionRoot == null || _rebuildScheduled)
        {
            return;
        }

        _rebuildScheduled = true;
        EditorApplication.delayCall += RebuildSectionDelayed;
    }

    private void RebuildSectionDelayed()
    {
        EditorApplication.delayCall -= RebuildSectionDelayed;
        _rebuildScheduled = false;

        if(_sectionRoot == null)
        {
            return;
        }

        RebuildSection();
    }

    private VisualElement CreateSectionContent()
    {
        return _serializedPropertyPath switch
        {
            ProfilesPropertyPath => CreateProfilesSection(),
            SessionPropertyPath => CreateSessionSection(),
            SaveDataPropertyPath => CreateSaveDataSection(),
            _ => CreateGenericPropertySection()
        };
    }

    private VisualElement CreateGenericPropertySection()
    {
        SerializedProperty property = _serializedMockDataAsset.FindProperty(_serializedPropertyPath);
        if(property == null)
        {
            return new HelpBox(
                $"The mock data section '{_serializedPropertyPath}' could not be found on '{nameof(EmulatorMockDataAsset)}'.",
                HelpBoxMessageType.Error);
        }

        ScrollView scrollView = CreateScrollView();
        PropertyField propertyField = new(property);
        propertyField.AddToClassList(PropertyField.inspectorElementUssClassName);
        property.isExpanded = true;
        propertyField.BindProperty(property);
        propertyField.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
        {
            if(_isRebuildingSection)
            {
                return;
            }

            PersistMockDataAssetChanges();
        });
        propertyField.RegisterCallback<GeometryChangedEvent>(_ => ApplyInspectorFieldPresentation(propertyField, flattenTopLevelFoldout: true));
        scrollView.Add(propertyField);
        return scrollView;
    }

    private VisualElement CreateProfilesSection()
    {
        ScrollView scrollView = CreateScrollView();
        SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);

        for(int index = 0; index < profilesProperty.arraySize; index++)
        {
            SerializedProperty profileProperty = profilesProperty.GetArrayElementAtIndex(index);
            scrollView.Add(CreateProfileCard(profilesProperty, profileProperty, index));
        }

        Button addProfileButton = new(AddProfile)
        {
            text = "Add Profile",
            tooltip = "Add a new mock Board profile. The player and avatar identifiers are assigned automatically."
        };
        scrollView.Add(CreateFooterButtonRow(addProfileButton));
        return scrollView;
    }

    private VisualElement CreateSessionSection()
    {
        ScrollView scrollView = CreateScrollView();
        SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
        SerializedProperty sessionProperty = FindRequiredProperty(SessionPropertyPath);
        SerializedProperty activeProfileIdProperty = sessionProperty.FindPropertyRelative("m_activeProfileId");
        SerializedProperty sessionPlayersProperty = sessionProperty.FindPropertyRelative("m_players");

        if(profilesProperty.arraySize == 0)
        {
            scrollView.Add(new HelpBox(
                "At least one profile is required before session players can be configured.",
                HelpBoxMessageType.Info));
            return scrollView;
        }

        scrollView.Add(CreateProfileSelectionField(
            labelText: "Active Profile",
            currentPlayerId: activeProfileIdProperty.stringValue,
            tooltip: "Select the system-wide active profile returned by the Board Session API.",
            getReservedPlayerIds: () => Array.Empty<string>(),
            includeMissingCurrentValue: false,
            onSelectionChanged: SetActiveProfile));

        Label playersTitle = new("Session Players");
        playersTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        playersTitle.style.marginTop = 8f;
        playersTitle.style.marginBottom = 6f;
        scrollView.Add(playersTitle);

        for(int index = 0; index < sessionPlayersProperty.arraySize; index++)
        {
            int localIndex = index;
            SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(index);
            bool isLastNonGuestSessionPlayer = IsLastNonGuestSessionPlayer(sessionPlayersProperty, localIndex);
            Box playerCard = CreateCard();
            playerCard.Add(CreateCardHeader(
                titleText: $"Player{index + 1}",
                removeButtonTooltip: isLastNonGuestSessionPlayer
                    ? LastNonGuestSessionPlayerTooltip
                    : "Remove this player from the current mock session.",
                removeButtonEnabled: isLastNonGuestSessionPlayer == false,
                onRemove: () => RemoveSessionPlayer(localIndex)));
            playerCard.Add(CreateSessionPlayerEditor(sessionPlayersProperty, sessionPlayerProperty, localIndex));
            scrollView.Add(playerCard);
        }

        Button addPlayerButton = new(AddSessionPlayer)
        {
            text = "Add Session Player",
            tooltip = "Add another player to the current mock Board session. This prefers an unused profile, then falls back to a guest slot."
        };
        scrollView.Add(CreateFooterButtonRow(addPlayerButton));
        return scrollView;
    }

    private VisualElement CreateSaveDataSection()
    {
        ScrollView scrollView = CreateScrollView();
        SerializedProperty saveGamesProperty = FindRequiredProperty(SaveDataPropertyPath);

        if(saveGamesProperty.arraySize == 0)
        {
            scrollView.Add(new HelpBox(
                "No mock save data has been configured yet.",
                HelpBoxMessageType.Info));
        }

        for(int index = 0; index < saveGamesProperty.arraySize; index++)
        {
            SerializedProperty saveGameProperty = saveGamesProperty.GetArrayElementAtIndex(index);
            scrollView.Add(CreateSaveGameCard(saveGameProperty, index));
        }

        Button addSaveButton = new(AddSaveGame)
        {
            text = "Add Save",
            tooltip = "Add a new mock save entry to the emulator."
        };
        scrollView.Add(CreateFooterButtonRow(addSaveButton));
        return scrollView;
    }

    private VisualElement CreateProfileCard(SerializedProperty profilesProperty, SerializedProperty profileProperty, int index)
    {
        string displayName = profileProperty.FindPropertyRelative("m_displayName").stringValue;
        string playerId = profileProperty.FindPropertyRelative("m_playerId").stringValue;
        string avatarId = profileProperty.FindPropertyRelative("m_avatarId").stringValue;

        Box card = CreateCard();
        Label titleLabel = new(GetProfileCardTitle(displayName, index));
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

        int localIndex = index;
        card.Add(CreateCardHeader(
            titleLabel,
            "Remove this mock profile.",
            profilesProperty.arraySize > 1,
            () => RemoveProfile(localIndex)));

        card.Add(CreateMetadataSummaryRow(
            ("Player Id", playerId, "The persistent Board player identifier returned for this mock profile. This identifier is assigned automatically."),
            ("Avatar Id", avatarId, "The avatar identifier returned for this mock profile. This identifier is assigned automatically.")));

        TextField displayNameField = new("Display Name")
        {
            isDelayed = true,
            value = displayName
        };
        ConfigureAlignedField(displayNameField, "The display name returned for this mock profile.");
        displayNameField.RegisterValueChangedCallback(evt =>
        {
            titleLabel.text = GetProfileCardTitle(evt.newValue, localIndex);
            SetProfileDisplayName(playerId, evt.newValue);
        });
        card.Add(displayNameField);
        card.Add(CreateBoundPropertyField(
            profileProperty.FindPropertyRelative("m_avatarBackgroundColor"),
            "Avatar Background Color",
            "The internally persisted avatar background color used by the emulator UI for this mock profile."));
        card.Add(CreateBoundPropertyField(
            profileProperty.FindPropertyRelative("m_avatar"),
            "Avatar",
            "The avatar texture returned when the Board SDK requests this mock profile's avatar."));
        return card;
    }

    private VisualElement CreateSaveGameCard(SerializedProperty saveGameProperty, int index)
    {
        string description = saveGameProperty.FindPropertyRelative("m_description").stringValue;
        string title = string.IsNullOrWhiteSpace(description) ? $"Save {index + 1}" : description;

        Box card = CreateCard();
        Label titleLabel = new(title);
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        card.Add(CreateCardHeader(
            titleLabel,
            "Remove this mock save.",
            true,
            () => RemoveSaveGame(index)));

        PropertyField descriptionField = CreateBoundPropertyField(
            saveGameProperty.FindPropertyRelative("m_description"),
            "Description",
            "The save description returned for this mock save.");
        descriptionField.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
        {
            if(_isRebuildingSection)
            {
                return;
            }

            ScheduleRebuildSection();
        });
        card.Add(descriptionField);

        card.Add(CreateBoundPropertyField(
            saveGameProperty.FindPropertyRelative("m_playedTime"),
            "Played Time",
            "The played-time value, in seconds, returned for this mock save."));
        card.Add(CreateBoundPropertyField(
            saveGameProperty.FindPropertyRelative("m_gameVersion"),
            "Game Version",
            "The game-version string returned for this mock save."));
        card.Add(CreateBoundPropertyField(
            saveGameProperty.FindPropertyRelative("m_coverImage"),
            "Cover Image",
            "The cover image returned for this mock save."));
        card.Add(CreateBoundPropertyField(
            saveGameProperty.FindPropertyRelative("m_payload"),
            "Payload",
            "The payload bytes returned when this mock save is loaded."));

        Label playersTitle = new("Players");
        playersTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        playersTitle.style.marginTop = 6f;
        playersTitle.style.marginBottom = 6f;
        playersTitle.tooltip = "Select which profiles are represented in this mock save snapshot.";
        card.Add(playersTitle);

        SerializedProperty playersProperty = saveGameProperty.FindPropertyRelative("m_players");
        for(int indexInSave = 0; indexInSave < playersProperty.arraySize; indexInSave++)
        {
            int localIndex = indexInSave;
            string currentPlayerId = playersProperty.GetArrayElementAtIndex(indexInSave).FindPropertyRelative("m_playerId").stringValue;

            Box playerCard = CreateNestedCard();
            playerCard.Add(CreateCardHeader(
                $"Player{indexInSave + 1}",
                "Remove this saved player reference from the mock save.",
                true,
                () => RemoveSaveGamePlayer(saveGameProperty, localIndex)));
            playerCard.Add(CreateProfileSelectionField(
                labelText: "Profile",
                currentPlayerId: currentPlayerId,
                tooltip: "Select which profile snapshot is stored with this mock save.",
                getReservedPlayerIds: () => GetReservedSavePlayerIds(playersProperty, localIndex),
                includeMissingCurrentValue: true,
                onSelectionChanged: selectedPlayerId => SetSaveGamePlayerProfile(saveGameProperty, localIndex, selectedPlayerId)));
            card.Add(playerCard);
        }

        Button addSavedPlayerButton = new(() => AddSaveGamePlayer(saveGameProperty))
        {
            text = "Add Saved Player",
            tooltip = "Add another profile reference to this mock save."
        };
        addSavedPlayerButton.SetEnabled(HasAvailableUnassignedProfiles(playersProperty));
        card.Add(CreateFooterButtonRow(addSavedPlayerButton));
        return card;
    }

    private void AddProfile()
    {
        UpdateSerializedValue(
            "Add Emulator Profile",
            () =>
            {
                SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
                string newProfileId = AllocateNextProfileId(profilesProperty);
                string newAvatarId = AllocateNextAvatarId(profilesProperty);
                int newIndex = profilesProperty.arraySize;
                profilesProperty.arraySize++;

                SerializedProperty profileProperty = profilesProperty.GetArrayElementAtIndex(newIndex);
                profileProperty.FindPropertyRelative("m_playerId").stringValue = newProfileId;
                profileProperty.FindPropertyRelative("m_displayName").stringValue = $"Player{newIndex + 1}";
                profileProperty.FindPropertyRelative("m_avatarId").stringValue = newAvatarId;
                profileProperty.FindPropertyRelative("m_type").enumValueIndex = 0;
                profileProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue = EmulatorAvatarUtility.GetPaletteColor(newIndex);
                profileProperty.FindPropertyRelative("m_avatar").objectReferenceValue = null;
            },
            rebuildSection: true);
    }

    private void RemoveProfile(int profileIndex)
    {
        UpdateSerializedValue(
            "Remove Emulator Profile",
            () =>
            {
                SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
                profilesProperty.DeleteArrayElementAtIndex(profileIndex);
                RepairSessionAuthoringData();
            },
            rebuildSection: true);
    }

    private void SetActiveProfile(string playerId)
    {
        UpdateSerializedValue(
            "Set Active Emulator Profile",
            () =>
            {
                SerializedProperty sessionProperty = FindRequiredProperty(SessionPropertyPath);
                sessionProperty.FindPropertyRelative("m_activeProfileId").stringValue = playerId;
                RepairSessionAuthoringData();
            });
    }

    private void AddSessionPlayer()
    {
        UpdateSerializedValue(
            "Add Emulator Session Player",
            () =>
            {
                SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
                SerializedProperty sessionPlayersProperty = FindRequiredProperty(SessionPropertyPath).FindPropertyRelative("m_players");
                string nextProfileId = GetFirstAvailableUnassignedProfileId(sessionPlayersProperty);
                int nextIndex = sessionPlayersProperty.arraySize;
                sessionPlayersProperty.arraySize++;

                SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(nextIndex);
                sessionPlayerProperty.FindPropertyRelative("m_sessionId").intValue = AllocateNextSessionId(sessionPlayersProperty, nextIndex);

                if(string.IsNullOrWhiteSpace(nextProfileId) == false)
                {
                    sessionPlayerProperty.FindPropertyRelative("m_type").enumValueIndex = (int)BoardPlayerType.Profile;
                    sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue = nextProfileId;
                    sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue = string.Empty;
                    sessionPlayerProperty.FindPropertyRelative("m_avatarId").stringValue = string.Empty;
                    sessionPlayerProperty.FindPropertyRelative("m_avatar").objectReferenceValue = null;
                    sessionPlayerProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue = default;
                }
                else
                {
                    InitializeGuestSessionPlayer(profilesProperty, sessionPlayersProperty, sessionPlayerProperty, nextIndex);
                }

                RepairSessionAuthoringData();
            },
            rebuildSection: true);
    }

    private void RemoveSessionPlayer(int sessionPlayerIndex)
    {
        UpdateSerializedValue(
            "Remove Emulator Session Player",
            () =>
            {
                SerializedProperty sessionPlayersProperty = FindRequiredProperty(SessionPropertyPath).FindPropertyRelative("m_players");
                if(IsLastNonGuestSessionPlayer(sessionPlayersProperty, sessionPlayerIndex))
                {
                    return;
                }

                sessionPlayersProperty.DeleteArrayElementAtIndex(sessionPlayerIndex);
                RepairSessionAuthoringData();
            },
            rebuildSection: true);
    }

    private void SetSessionPlayerProfile(int sessionPlayerIndex, string playerId)
    {
        UpdateSerializedValue(
            "Set Emulator Session Player",
            () =>
            {
                SerializedProperty sessionPlayersProperty = FindRequiredProperty(SessionPropertyPath).FindPropertyRelative("m_players");
                SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(sessionPlayerIndex);
                sessionPlayerProperty.FindPropertyRelative("m_type").enumValueIndex = (int)BoardPlayerType.Profile;
                sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue = playerId;
                sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue = string.Empty;
                sessionPlayerProperty.FindPropertyRelative("m_avatarId").stringValue = string.Empty;
                sessionPlayerProperty.FindPropertyRelative("m_avatar").objectReferenceValue = null;
                sessionPlayerProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue = default;
                RepairSessionAuthoringData();
            },
            rebuildSection: true);
    }

    private void SetSessionPlayerType(int sessionPlayerIndex, BoardPlayerType playerType)
    {
        UpdateSerializedValue(
            "Set Emulator Session Player Type",
            () =>
            {
                SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
                SerializedProperty sessionPlayersProperty = FindRequiredProperty(SessionPropertyPath).FindPropertyRelative("m_players");
                SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(sessionPlayerIndex);
                BoardPlayerType currentType = GetSessionPlayerType(sessionPlayerProperty);
                if(currentType == playerType)
                {
                    return;
                }

                if(currentType != BoardPlayerType.Guest
                && playerType == BoardPlayerType.Guest
                && IsLastNonGuestSessionPlayer(sessionPlayersProperty, sessionPlayerIndex))
                {
                    return;
                }

                switch(playerType)
                {
                    case BoardPlayerType.Profile:
                        string nextProfileId = GetFirstAvailableUnassignedProfileId(sessionPlayersProperty, sessionPlayerIndex);
                        if(string.IsNullOrWhiteSpace(nextProfileId))
                        {
                            return;
                        }

                        sessionPlayerProperty.FindPropertyRelative("m_type").enumValueIndex = (int)BoardPlayerType.Profile;
                        sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue = nextProfileId;
                        sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue = string.Empty;
                        sessionPlayerProperty.FindPropertyRelative("m_avatarId").stringValue = string.Empty;
                        sessionPlayerProperty.FindPropertyRelative("m_avatar").objectReferenceValue = null;
                        sessionPlayerProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue = default;
                        break;

                    case BoardPlayerType.Guest:
                        InitializeGuestSessionPlayer(profilesProperty, sessionPlayersProperty, sessionPlayerProperty, sessionPlayerIndex);
                        break;

                    default:
                        return;
                }

                RepairSessionAuthoringData();
            },
            rebuildSection: true);
    }

    private void SetGuestSessionPlayerDisplayName(int sessionPlayerIndex, string displayName)
    {
        UpdateSerializedValue(
            "Set Emulator Guest Session Player Name",
            () =>
            {
                string normalizedDisplayName = NormalizeGuestDisplayName(displayName);
                if(IsGuestDisplayNameAvailable(normalizedDisplayName, sessionPlayerIndex) == false)
                {
                    return;
                }

                SerializedProperty sessionPlayersProperty = FindRequiredProperty(SessionPropertyPath).FindPropertyRelative("m_players");
                SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(sessionPlayerIndex);
                sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue = normalizedDisplayName;
                RepairSessionAuthoringData();
            },
            rebuildSection: true);
    }

    private void SetProfileDisplayName(string playerId, string displayName)
    {
        UpdateSerializedValue(
            "Edit Emulator Profile",
            () =>
            {
                SerializedProperty currentProfileProperty = FindProfileProperty(playerId);
                if(currentProfileProperty == null)
                {
                    return;
                }

                currentProfileProperty.FindPropertyRelative("m_displayName").stringValue = displayName;
                RepairSessionAuthoringData();
            },
            rebuildSection: true);
    }

    private void AddSaveGame()
    {
        UpdateSerializedValue(
            "Add Emulator Save",
            () =>
            {
                SerializedProperty saveGamesProperty = FindRequiredProperty(SaveDataPropertyPath);
                int saveIndex = saveGamesProperty.arraySize;
                saveGamesProperty.arraySize++;

                SerializedProperty saveGameProperty = saveGamesProperty.GetArrayElementAtIndex(saveIndex);
                ulong now = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                saveGameProperty.FindPropertyRelative("m_saveId").stringValue = Guid.NewGuid().ToString("N");
                saveGameProperty.FindPropertyRelative("m_description").stringValue = $"Save {saveIndex + 1}";
                saveGameProperty.FindPropertyRelative("m_createdAt").ulongValue = now;
                saveGameProperty.FindPropertyRelative("m_updatedAt").ulongValue = now;
                saveGameProperty.FindPropertyRelative("m_playedTime").ulongValue = 0UL;
                saveGameProperty.FindPropertyRelative("m_gameVersion").stringValue = Application.version;
                saveGameProperty.FindPropertyRelative("m_coverImage").objectReferenceValue = null;
                saveGameProperty.FindPropertyRelative("m_payload").ClearArray();
                saveGameProperty.FindPropertyRelative("m_payloadChecksum").stringValue = string.Empty;
                saveGameProperty.FindPropertyRelative("m_coverImageChecksum").stringValue = string.Empty;

                SerializedProperty savePlayersProperty = saveGameProperty.FindPropertyRelative("m_players");
                savePlayersProperty.arraySize = 0;

                SerializedProperty sessionPlayersProperty = FindRequiredProperty(SessionPropertyPath).FindPropertyRelative("m_players");
                for(int sessionIndex = 0; sessionIndex < sessionPlayersProperty.arraySize; sessionIndex++)
                {
                    string playerId = sessionPlayersProperty.GetArrayElementAtIndex(sessionIndex).FindPropertyRelative("m_playerId").stringValue;
                    if(string.IsNullOrWhiteSpace(playerId))
                    {
                        continue;
                    }

                    int playerIndex = savePlayersProperty.arraySize;
                    savePlayersProperty.arraySize++;
                    ApplyProfileSnapshotToSaveGamePlayer(savePlayersProperty.GetArrayElementAtIndex(playerIndex), playerId);
                }
            },
            rebuildSection: true);
    }

    private void RemoveSaveGame(int saveGameIndex)
    {
        UpdateSerializedValue(
            "Remove Emulator Save",
            () => FindRequiredProperty(SaveDataPropertyPath).DeleteArrayElementAtIndex(saveGameIndex),
            rebuildSection: true);
    }

    private void AddSaveGamePlayer(SerializedProperty saveGameProperty)
    {
        UpdateSerializedValue(
            "Add Emulator Saved Player",
            () =>
            {
                SerializedProperty playersProperty = saveGameProperty.FindPropertyRelative("m_players");
                string nextProfileId = GetFirstAvailableUnassignedProfileId(playersProperty);
                if(string.IsNullOrWhiteSpace(nextProfileId))
                {
                    return;
                }

                int nextIndex = playersProperty.arraySize;
                playersProperty.arraySize++;
                ApplyProfileSnapshotToSaveGamePlayer(playersProperty.GetArrayElementAtIndex(nextIndex), nextProfileId);
            },
            rebuildSection: true);
    }

    private void RemoveSaveGamePlayer(SerializedProperty saveGameProperty, int playerIndex)
    {
        UpdateSerializedValue(
            "Remove Emulator Saved Player",
            () => saveGameProperty.FindPropertyRelative("m_players").DeleteArrayElementAtIndex(playerIndex),
            rebuildSection: true);
    }

    private void SetSaveGamePlayerProfile(SerializedProperty saveGameProperty, int playerIndex, string playerId)
    {
        UpdateSerializedValue(
            "Set Emulator Saved Player",
            () => ApplyProfileSnapshotToSaveGamePlayer(saveGameProperty.FindPropertyRelative("m_players").GetArrayElementAtIndex(playerIndex), playerId));
    }

    private PropertyField CreateBoundPropertyField(SerializedProperty property, string labelText, string tooltip)
    {
        PropertyField field = new(property, labelText);
        field.AddToClassList(PropertyField.inspectorElementUssClassName);
        field.tooltip = tooltip;
        field.BindProperty(property);
        field.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
        {
            if(_isRebuildingSection)
            {
                return;
            }

            PersistMockDataAssetChanges();
        });
        field.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            ApplyInspectorFieldPresentation(field);
            ApplyTooltip(field, tooltip);
        });
        return field;
    }

    private VisualElement CreateSessionPlayerEditor(
        SerializedProperty sessionPlayersProperty,
        SerializedProperty sessionPlayerProperty,
        int sessionPlayerIndex)
    {
        VisualElement content = new();
        BoardPlayerType playerType = GetSessionPlayerType(sessionPlayerProperty);
        bool isLastNonGuestSessionPlayer = IsLastNonGuestSessionPlayer(sessionPlayersProperty, sessionPlayerIndex);

        EnumField typeField = new("Type", playerType);
        ConfigureAlignedField(
            typeField,
            isLastNonGuestSessionPlayer
                ? LastNonGuestSessionPlayerTooltip
                : "Choose whether this session slot is backed by a persisted Board profile or a session-only guest.");
        typeField.SetEnabled(isLastNonGuestSessionPlayer == false);
        typeField.RegisterValueChangedCallback(evt =>
        {
            if(evt.newValue is not BoardPlayerType newType || newType == playerType)
            {
                return;
            }

            SetSessionPlayerType(sessionPlayerIndex, newType);
        });
        content.Add(typeField);

        if(playerType == BoardPlayerType.Guest)
        {
            TextField displayNameField = new("Display Name")
            {
                isDelayed = true,
                value = sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue
            };
            ConfigureAlignedField(displayNameField, "The display name returned for this session-only guest player.");
            displayNameField.RegisterValueChangedCallback(evt =>
            {
                if(string.Equals(evt.previousValue, evt.newValue, StringComparison.Ordinal))
                {
                    return;
                }

                SetGuestSessionPlayerDisplayName(sessionPlayerIndex, evt.newValue);
            });
            content.Add(displayNameField);
            return content;
        }

        string currentPlayerId = sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue;
        content.Add(CreateProfileSelectionField(
            labelText: "Profile",
            currentPlayerId: currentPlayerId,
            tooltip: "Select which persisted profile is present in this mock session slot.",
            getReservedPlayerIds: () => GetReservedSessionPlayerIds(sessionPlayersProperty, sessionPlayerIndex),
            includeMissingCurrentValue: true,
            onSelectionChanged: selectedPlayerId => SetSessionPlayerProfile(sessionPlayerIndex, selectedPlayerId)));
        return content;
    }

    private VisualElement CreateProfileSelectionField(
        string labelText,
        string currentPlayerId,
        string tooltip,
        Func<IEnumerable<string>> getReservedPlayerIds,
        bool includeMissingCurrentValue,
        Action<string> onSelectionChanged)
    {
        List<string> choices = BuildProfileChoices(
            currentPlayerId,
            getReservedPlayerIds?.Invoke() ?? Array.Empty<string>(),
            includeMissingCurrentValue);
        if(choices.Count == 0)
        {
            return CreateReadOnlyTextField(labelText, "No profiles available", tooltip);
        }

        int selectedIndex = Mathf.Max(0, choices.IndexOf(currentPlayerId));
        PopupField<string> popupField = new(
            labelText,
            choices,
            selectedIndex,
            GetProfileChoiceLabel,
            GetProfileChoiceLabel);
        ConfigureAlignedField(popupField, tooltip);
        popupField.RegisterValueChangedCallback(evt =>
        {
            if(string.Equals(evt.previousValue, evt.newValue, StringComparison.Ordinal))
            {
                return;
            }

            onSelectionChanged(evt.newValue);
        });
        return popupField;
    }

    private TextField CreateReadOnlyTextField(string labelText, string value, string tooltip)
    {
        TextField field = new(labelText)
        {
            value = value ?? string.Empty,
            isReadOnly = true
        };
        field.focusable = false;
        ConfigureAlignedField(field, tooltip);
        return field;
    }

    private static VisualElement CreateMetadataSummaryRow(params (string Label, string Value, string Tooltip)[] items)
    {
        VisualElement row = new();
        row.style.flexDirection = FlexDirection.Row;
        row.style.flexWrap = Wrap.Wrap;
        row.style.marginBottom = 10f;

        foreach((string itemLabel, string itemValue, string itemTooltip) in items)
        {
            VisualElement group = new();
            group.style.flexDirection = FlexDirection.Row;
            group.style.alignItems = Align.Center;
            group.style.marginRight = 18f;
            group.style.marginBottom = 4f;
            group.tooltip = itemTooltip;

            Label label = new($"{itemLabel}:")
            {
                tooltip = itemTooltip
            };
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            group.Add(label);

            Label value = new(string.IsNullOrWhiteSpace(itemValue) ? "None" : itemValue)
            {
                tooltip = itemTooltip
            };
            value.style.marginLeft = 4f;
            value.style.color = new StyleColor(new Color(0.72f, 0.72f, 0.72f));
            group.Add(value);

            row.Add(group);
        }

        return row;
    }

    private static ScrollView CreateScrollView()
    {
        ScrollView scrollView = new();
        scrollView.style.flexGrow = 1f;
        scrollView.AddToClassList(InspectorElement.ussClassName);
        return scrollView;
    }

    private static Box CreateCard()
    {
        Box card = new();
        card.style.marginBottom = 12f;
        card.style.paddingLeft = 12f;
        card.style.paddingRight = 12f;
        card.style.paddingTop = 10f;
        card.style.paddingBottom = 10f;
        return card;
    }

    private static Box CreateNestedCard()
    {
        Box card = CreateCard();
        card.style.marginBottom = 8f;
        card.style.paddingTop = 8f;
        card.style.paddingBottom = 8f;
        return card;
    }

    private static VisualElement CreateCardHeader(string titleText, string removeButtonTooltip, bool removeButtonEnabled, Action onRemove)
    {
        Label titleLabel = new(titleText);
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        return CreateCardHeader(titleLabel, removeButtonTooltip, removeButtonEnabled, onRemove);
    }

    private static VisualElement CreateCardHeader(Label titleLabel, string removeButtonTooltip, bool removeButtonEnabled, Action onRemove)
    {
        VisualElement header = new();
        header.style.flexDirection = FlexDirection.Row;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.alignItems = Align.Center;
        header.style.marginBottom = 8f;
        header.Add(titleLabel);

        Button removeButton = new(onRemove)
        {
            text = "Remove",
            tooltip = removeButtonTooltip
        };
        removeButton.SetEnabled(removeButtonEnabled);
        header.Add(removeButton);
        return header;
    }

    private static VisualElement CreateFooterButtonRow(Button button)
    {
        VisualElement footer = new();
        footer.style.flexDirection = FlexDirection.Row;
        footer.style.justifyContent = Justify.FlexStart;
        footer.style.marginTop = 6f;
        footer.Add(button);
        return footer;
    }

    private void UpdateSerializedValue(string undoLabel, Action mutation, bool rebuildSection = false)
    {
        if(_serializedMockDataAsset == null)
        {
            return;
        }

        Undo.RecordObject(_mockDataAsset, undoLabel);
        _serializedMockDataAsset.UpdateIfRequiredOrScript();
        mutation();
        PersistMockDataAssetChanges();

        if(rebuildSection)
        {
            ScheduleRebuildSection();
        }
    }

    private void PersistMockDataAssetChanges()
    {
        if(_serializedMockDataAsset == null)
        {
            return;
        }

        _suppressedAssetChangedEventsRemaining += 2;
        _serializedMockDataAsset.ApplyModifiedProperties();
        EmulatorProjectSettingsAssetUtility.EnsureMockDataAssetNameMatchesFileName(_mockDataAsset);
        EditorUtility.SetDirty(_mockDataAsset);
        AssetDatabase.SaveAssets();
        _mockDataAsset.NotifyChanged();
        _serializedMockDataAsset.UpdateIfRequiredOrScript();
        _lastSectionStructureSignature = CaptureSectionStructureSignature();
    }

    private void OnMockDataAssetChanged(object sender, EventArgs eventArgs)
    {
        RefreshSerializedMockDataAsset();

        if(_suppressedAssetChangedEventsRemaining > 0)
        {
            _suppressedAssetChangedEventsRemaining--;
            if(HasSectionStructureChanged())
            {
                ScheduleRebuildSection();
            }

            return;
        }

        ScheduleRebuildSection();
    }

    private void RefreshSerializedMockDataAsset()
    {
        _serializedMockDataAsset = _mockDataAsset != null ? new SerializedObject(_mockDataAsset) : null;
    }

    private bool HasSectionStructureChanged()
    {
        if(_serializedMockDataAsset == null || _sectionRoot == null || _isRebuildingSection)
        {
            return false;
        }

        return string.Equals(_lastSectionStructureSignature, CaptureSectionStructureSignature(), StringComparison.Ordinal) == false;
    }

    private string CaptureSectionStructureSignature()
    {
        if(_serializedMockDataAsset == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        switch(_serializedPropertyPath)
        {
            case ProfilesPropertyPath:
                SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
                builder.Append("profiles:").Append(profilesProperty.arraySize);
                for(int index = 0; index < profilesProperty.arraySize; index++)
                {
                    SerializedProperty profileProperty = profilesProperty.GetArrayElementAtIndex(index);
                    builder.Append('|')
                        .Append(profileProperty.FindPropertyRelative("m_playerId").stringValue)
                        .Append(':')
                        .Append(profileProperty.FindPropertyRelative("m_displayName").stringValue)
                        .Append(':')
                        .Append(profileProperty.FindPropertyRelative("m_avatarId").stringValue);
                }

                break;

            case SessionPropertyPath:
                SerializedProperty sessionProperty = FindRequiredProperty(SessionPropertyPath);
                SerializedProperty sessionPlayersProperty = sessionProperty.FindPropertyRelative("m_players");
                builder.Append("session:")
                    .Append(sessionProperty.FindPropertyRelative("m_activeProfileId").stringValue)
                    .Append(':')
                    .Append(sessionPlayersProperty.arraySize);
                for(int index = 0; index < sessionPlayersProperty.arraySize; index++)
                {
                    SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(index);
                    builder.Append('|')
                        .Append(sessionPlayerProperty.FindPropertyRelative("m_sessionId").intValue)
                        .Append(':')
                        .Append(sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue)
                        .Append(':')
                        .Append(sessionPlayerProperty.FindPropertyRelative("m_type").enumValueIndex);
                }

                break;

            case SaveDataPropertyPath:
                SerializedProperty saveGamesProperty = FindRequiredProperty(SaveDataPropertyPath);
                builder.Append("save:").Append(saveGamesProperty.arraySize);
                for(int saveIndex = 0; saveIndex < saveGamesProperty.arraySize; saveIndex++)
                {
                    SerializedProperty saveGameProperty = saveGamesProperty.GetArrayElementAtIndex(saveIndex);
                    SerializedProperty playersProperty = saveGameProperty.FindPropertyRelative("m_players");
                    builder.Append('|')
                        .Append(saveGameProperty.FindPropertyRelative("m_saveId").stringValue)
                        .Append(':')
                        .Append(saveGameProperty.FindPropertyRelative("m_description").stringValue)
                        .Append(':')
                        .Append(playersProperty.arraySize);
                    for(int playerIndex = 0; playerIndex < playersProperty.arraySize; playerIndex++)
                    {
                        builder.Append('>')
                            .Append(playersProperty.GetArrayElementAtIndex(playerIndex).FindPropertyRelative("m_playerId").stringValue);
                    }
                }

                break;

            default:
                builder.Append(_serializedPropertyPath);
                break;
        }

        return builder.ToString();
    }

    private void NormalizeAuthoringDataIfNeeded()
    {
        bool changed = false;
        SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
        HashSet<string> assignedProfileIds = new(StringComparer.Ordinal);
        HashSet<string> assignedAvatarIds = new(StringComparer.Ordinal);
        for(int index = 0; index < profilesProperty.arraySize; index++)
        {
            SerializedProperty profileProperty = profilesProperty.GetArrayElementAtIndex(index);
            SerializedProperty playerIdProperty = profileProperty.FindPropertyRelative("m_playerId");
            SerializedProperty displayNameProperty = profileProperty.FindPropertyRelative("m_displayName");
            SerializedProperty avatarIdProperty = profileProperty.FindPropertyRelative("m_avatarId");

            if(string.IsNullOrWhiteSpace(playerIdProperty.stringValue) || assignedProfileIds.Contains(playerIdProperty.stringValue))
            {
                playerIdProperty.stringValue = AllocateNextProfileId(profilesProperty, assignedProfileIds);
                changed = true;
            }

            assignedProfileIds.Add(playerIdProperty.stringValue);

            if(string.IsNullOrWhiteSpace(displayNameProperty.stringValue))
            {
                displayNameProperty.stringValue = $"Player{index + 1}";
                changed = true;
            }

            SerializedProperty typeProperty = profileProperty.FindPropertyRelative("m_type");
            if(typeProperty.enumValueIndex != (int)BoardPlayerType.Profile)
            {
                typeProperty.enumValueIndex = (int)BoardPlayerType.Profile;
                changed = true;
            }

            if(string.IsNullOrWhiteSpace(avatarIdProperty.stringValue)
            || int.TryParse(avatarIdProperty.stringValue, out _) == false
            || assignedAvatarIds.Contains(avatarIdProperty.stringValue))
            {
                avatarIdProperty.stringValue = AllocateNextAvatarId(profilesProperty, assignedAvatarIds);
                changed = true;
            }

            assignedAvatarIds.Add(avatarIdProperty.stringValue);
        }

        changed |= RepairSessionAuthoringData();
        if(changed)
        {
            PersistMockDataAssetChanges();
        }
    }

    private bool RepairSessionAuthoringData()
    {
        SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
        SerializedProperty sessionProperty = FindRequiredProperty(SessionPropertyPath);
        SerializedProperty activeProfileIdProperty = sessionProperty.FindPropertyRelative("m_activeProfileId");
        SerializedProperty sessionPlayersProperty = sessionProperty.FindPropertyRelative("m_players");

        List<string> profileIds = GetAllProfileIds(profilesProperty);
        string fallbackActiveProfileId = profileIds.FirstOrDefault() ?? string.Empty;
        bool changed = false;

        if(string.IsNullOrWhiteSpace(activeProfileIdProperty.stringValue)
        || profileIds.Contains(activeProfileIdProperty.stringValue) == false)
        {
            activeProfileIdProperty.stringValue = fallbackActiveProfileId;
            changed = true;
        }

        HashSet<string> reservedIds = new(profileIds, StringComparer.Ordinal);
        HashSet<int> assignedSessionIds = new();
        List<SessionAuthoringPlayer> normalizedPlayers = new();
        HashSet<string> reservedGuestDisplayNames = new(StringComparer.OrdinalIgnoreCase);
        for(int profileIndex = 0; profileIndex < profilesProperty.arraySize; profileIndex++)
        {
            string profileDisplayName = profilesProperty.GetArrayElementAtIndex(profileIndex)
                .FindPropertyRelative("m_displayName")
                .stringValue;
            if(string.IsNullOrWhiteSpace(profileDisplayName) == false)
            {
                reservedGuestDisplayNames.Add(profileDisplayName);
            }
        }

        for(int index = 0; index < sessionPlayersProperty.arraySize; index++)
        {
            SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(index);
            SessionAuthoringPlayer player = ReadSessionAuthoringPlayer(sessionPlayerProperty);
            if(player == null)
            {
                changed = true;
                continue;
            }

            if(player.SessionId <= 0 || assignedSessionIds.Add(player.SessionId) == false)
            {
                player.SessionId = AllocateNextSessionId(normalizedPlayers);
                assignedSessionIds.Add(player.SessionId);
                changed = true;
            }

            if(player.Type == BoardPlayerType.Guest)
            {
                if(string.IsNullOrWhiteSpace(player.PlayerId)
                || reservedIds.Add(player.PlayerId) == false)
                {
                    player.PlayerId = AllocateNextGuestPlayerId(profilesProperty, normalizedPlayers);
                    reservedIds.Add(player.PlayerId);
                    changed = true;
                }

                string normalizedDisplayName = NormalizeGuestDisplayName(player.DisplayName);
                string defaultDisplayName = GetDefaultGuestDisplayName(player.PlayerId);
                string uniqueDisplayName = ResolveUniqueGuestDisplayName(
                    normalizedDisplayName,
                    defaultDisplayName,
                    reservedGuestDisplayNames);
                if(string.Equals(player.DisplayName, uniqueDisplayName, StringComparison.Ordinal) == false)
                {
                    player.DisplayName = uniqueDisplayName;
                    changed = true;
                }
                reservedGuestDisplayNames.Add(player.DisplayName);

                if(string.IsNullOrWhiteSpace(player.AvatarId))
                {
                    player.AvatarId = AllocateNextSessionAvatarId(profilesProperty, normalizedPlayers);
                    changed = true;
                }

                if(EmulatorAvatarUtility.HasSerializedColor(player.AvatarBackgroundColor) == false)
                {
                    player.AvatarBackgroundColor = ResolveDefaultSessionAvatarBackgroundColor(player.PlayerId, player.AvatarId);
                    changed = true;
                }

                normalizedPlayers.Add(player);
                continue;
            }

            if(string.IsNullOrWhiteSpace(player.PlayerId)
            || profileIds.Contains(player.PlayerId) == false
            || normalizedPlayers.Any(existing =>
                existing.Type == BoardPlayerType.Profile
                && string.Equals(existing.PlayerId, player.PlayerId, StringComparison.Ordinal)))
            {
                changed = true;
                continue;
            }

            player.Type = BoardPlayerType.Profile;
            player.DisplayName = string.Empty;
            player.AvatarId = string.Empty;
            player.Avatar = null;
            player.AvatarBackgroundColor = default;
            normalizedPlayers.Add(player);
        }

        if(normalizedPlayers.Any(player => player.Type != BoardPlayerType.Guest) == false
        && string.IsNullOrWhiteSpace(activeProfileIdProperty.stringValue) == false)
        {
            normalizedPlayers.Add(new SessionAuthoringPlayer
            {
                SessionId = AllocateNextSessionId(normalizedPlayers),
                PlayerId = activeProfileIdProperty.stringValue,
                Type = BoardPlayerType.Profile
            });
            changed = true;
        }

        if(sessionPlayersProperty.arraySize != normalizedPlayers.Count)
        {
            sessionPlayersProperty.arraySize = normalizedPlayers.Count;
            changed = true;
        }

        for(int index = 0; index < normalizedPlayers.Count; index++)
        {
            SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(index);
            changed |= WriteSessionAuthoringPlayer(sessionPlayerProperty, normalizedPlayers[index]);
        }

        return changed;
    }

    private void ApplyProfileSnapshotToSaveGamePlayer(SerializedProperty saveGamePlayerProperty, string playerId)
    {
        SerializedProperty profileProperty = FindProfileProperty(playerId);
        if(profileProperty == null)
        {
            return;
        }

        saveGamePlayerProperty.FindPropertyRelative("m_playerId").stringValue = profileProperty.FindPropertyRelative("m_playerId").stringValue;
        saveGamePlayerProperty.FindPropertyRelative("m_displayName").stringValue = profileProperty.FindPropertyRelative("m_displayName").stringValue;
        saveGamePlayerProperty.FindPropertyRelative("m_avatarId").stringValue = profileProperty.FindPropertyRelative("m_avatarId").stringValue;
        saveGamePlayerProperty.FindPropertyRelative("m_type").intValue = profileProperty.FindPropertyRelative("m_type").intValue;
        saveGamePlayerProperty.FindPropertyRelative("m_avatar").objectReferenceValue = profileProperty.FindPropertyRelative("m_avatar").objectReferenceValue;
        saveGamePlayerProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue = profileProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue;
    }

    private string AllocateNextProfileId(SerializedProperty profilesProperty, ISet<string> reservedIds = null)
    {
        HashSet<string> assignedIds = reservedIds != null
            ? new HashSet<string>(reservedIds, StringComparer.Ordinal)
            : new HashSet<string>(GetAllProfileIds(profilesProperty), StringComparer.Ordinal);

        int index = 1;
        string candidate = $"profile-{index:000}";
        while(assignedIds.Contains(candidate))
        {
            index++;
            candidate = $"profile-{index:000}";
        }

        return candidate;
    }

    private string AllocateNextAvatarId(SerializedProperty profilesProperty, ISet<string> reservedIds = null)
    {
        HashSet<string> assignedIds = reservedIds != null
            ? new HashSet<string>(reservedIds, StringComparer.Ordinal)
            : new HashSet<string>(GetAllAvatarIds(profilesProperty), StringComparer.Ordinal);

        int index = 0;
        string candidate = index.ToString();
        while(assignedIds.Contains(candidate))
        {
            index++;
            candidate = index.ToString();
        }

        return candidate;
    }

    private static int AllocateNextSessionId(SerializedProperty sessionPlayersProperty, int ignoredIndex = -1)
    {
        int nextSessionId = 1;
        for(int index = 0; index < sessionPlayersProperty.arraySize; index++)
        {
            if(index == ignoredIndex)
            {
                continue;
            }

            SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(index);
            int sessionId = sessionPlayerProperty.FindPropertyRelative("m_sessionId").intValue;
            nextSessionId = Mathf.Max(nextSessionId, sessionId + 1);
        }

        return nextSessionId;
    }

    private static int AllocateNextSessionId(IEnumerable<SessionAuthoringPlayer> players)
    {
        return players.Select(player => player?.SessionId ?? 0).DefaultIfEmpty(0).Max() + 1;
    }

    private bool HasAvailableUnassignedProfiles(SerializedProperty referencedPlayersProperty)
    {
        return string.IsNullOrWhiteSpace(GetFirstAvailableUnassignedProfileId(referencedPlayersProperty)) == false;
    }

    private static int GetNonGuestSessionPlayerCount(SerializedProperty sessionPlayersProperty)
    {
        int count = 0;
        for(int index = 0; index < sessionPlayersProperty.arraySize; index++)
        {
            if(GetSessionPlayerType(sessionPlayersProperty.GetArrayElementAtIndex(index)) != BoardPlayerType.Guest)
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsLastNonGuestSessionPlayer(SerializedProperty sessionPlayersProperty, int sessionPlayerIndex)
    {
        if(sessionPlayerIndex < 0 || sessionPlayerIndex >= sessionPlayersProperty.arraySize)
        {
            return false;
        }

        return GetSessionPlayerType(sessionPlayersProperty.GetArrayElementAtIndex(sessionPlayerIndex)) != BoardPlayerType.Guest
            && GetNonGuestSessionPlayerCount(sessionPlayersProperty) <= 1;
    }

    private string GetFirstAvailableUnassignedProfileId(SerializedProperty referencedPlayersProperty, int ignoredIndex = -1)
    {
        HashSet<string> assignedIds = GetReferencedProfileIds(referencedPlayersProperty, ignoredIndex);
        foreach(string profileId in GetAllProfileIds(FindRequiredProperty(ProfilesPropertyPath)))
        {
            if(assignedIds.Contains(profileId) == false)
            {
                return profileId;
            }
        }

        return null;
    }

    private IEnumerable<string> GetReservedSessionPlayerIds(SerializedProperty sessionPlayersProperty, int currentIndex)
    {
        return GetReservedReferencedProfileIds(sessionPlayersProperty, currentIndex);
    }

    private IEnumerable<string> GetReservedSavePlayerIds(SerializedProperty savePlayersProperty, int currentIndex)
    {
        return GetReservedReferencedPlayerIds(savePlayersProperty, currentIndex, "m_playerId");
    }

    private static IEnumerable<string> GetReservedReferencedPlayerIds(SerializedProperty playersProperty, int currentIndex, string playerIdPropertyName)
    {
        for(int index = 0; index < playersProperty.arraySize; index++)
        {
            if(index == currentIndex)
            {
                continue;
            }

            string playerId = playersProperty.GetArrayElementAtIndex(index).FindPropertyRelative(playerIdPropertyName).stringValue;
            if(string.IsNullOrWhiteSpace(playerId) == false)
            {
                yield return playerId;
            }
        }
    }

    private static IEnumerable<string> GetReservedReferencedProfileIds(SerializedProperty playersProperty, int currentIndex)
    {
        for(int index = 0; index < playersProperty.arraySize; index++)
        {
            if(index == currentIndex)
            {
                continue;
            }

            SerializedProperty sessionPlayerProperty = playersProperty.GetArrayElementAtIndex(index);
            if(GetSessionPlayerType(sessionPlayerProperty) != BoardPlayerType.Profile)
            {
                continue;
            }

            string playerId = sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue;
            if(string.IsNullOrWhiteSpace(playerId) == false)
            {
                yield return playerId;
            }
        }
    }

    private static HashSet<string> GetReferencedPlayerIds(SerializedProperty playersProperty)
    {
        HashSet<string> playerIds = new(StringComparer.Ordinal);
        for(int index = 0; index < playersProperty.arraySize; index++)
        {
            string playerId = playersProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_playerId").stringValue;
            if(string.IsNullOrWhiteSpace(playerId) == false)
            {
                playerIds.Add(playerId);
            }
        }

        return playerIds;
    }

    private static HashSet<string> GetReferencedProfileIds(SerializedProperty playersProperty, int ignoredIndex = -1)
    {
        HashSet<string> playerIds = new(StringComparer.Ordinal);
        for(int index = 0; index < playersProperty.arraySize; index++)
        {
            if(index == ignoredIndex)
            {
                continue;
            }

            SerializedProperty sessionPlayerProperty = playersProperty.GetArrayElementAtIndex(index);
            if(GetSessionPlayerType(sessionPlayerProperty) != BoardPlayerType.Profile)
            {
                continue;
            }

            string playerId = sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue;
            if(string.IsNullOrWhiteSpace(playerId) == false)
            {
                playerIds.Add(playerId);
            }
        }

        return playerIds;
    }

    private List<string> BuildProfileChoices(string currentPlayerId, IEnumerable<string> reservedPlayerIds, bool includeMissingCurrentValue)
    {
        HashSet<string> reservedIds = new(reservedPlayerIds ?? Array.Empty<string>(), StringComparer.Ordinal);
        List<string> choices = new();

        foreach(string playerId in GetAllProfileIds(FindRequiredProperty(ProfilesPropertyPath)))
        {
            if(reservedIds.Contains(playerId) && string.Equals(playerId, currentPlayerId, StringComparison.Ordinal) == false)
            {
                continue;
            }

            choices.Add(playerId);
        }

        if(includeMissingCurrentValue
        && string.IsNullOrWhiteSpace(currentPlayerId) == false
        && choices.Contains(currentPlayerId) == false)
        {
            choices.Add(currentPlayerId);
        }

        return choices;
    }

    private List<string> GetAllProfileIds(SerializedProperty profilesProperty)
    {
        List<string> profileIds = new();
        for(int index = 0; index < profilesProperty.arraySize; index++)
        {
            string playerId = profilesProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_playerId").stringValue;
            if(string.IsNullOrWhiteSpace(playerId) == false)
            {
                profileIds.Add(playerId);
            }
        }

        return profileIds;
    }

    private List<string> GetAllAvatarIds(SerializedProperty profilesProperty)
    {
        List<string> avatarIds = new();
        for(int index = 0; index < profilesProperty.arraySize; index++)
        {
            string avatarId = profilesProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_avatarId").stringValue;
            if(string.IsNullOrWhiteSpace(avatarId) == false)
            {
                avatarIds.Add(avatarId);
            }
        }

        return avatarIds;
    }

    private SerializedProperty FindProfileProperty(string playerId)
    {
        if(string.IsNullOrWhiteSpace(playerId))
        {
            return null;
        }

        SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
        for(int index = 0; index < profilesProperty.arraySize; index++)
        {
            SerializedProperty profileProperty = profilesProperty.GetArrayElementAtIndex(index);
            if(string.Equals(profileProperty.FindPropertyRelative("m_playerId").stringValue, playerId, StringComparison.Ordinal))
            {
                return profileProperty;
            }
        }

        return null;
    }

    private string GetProfileChoiceLabel(string playerId)
    {
        SerializedProperty profileProperty = FindProfileProperty(playerId);
        if(profileProperty == null)
        {
            return $"Missing Profile ({playerId})";
        }

        string displayName = profileProperty.FindPropertyRelative("m_displayName").stringValue;
        return string.IsNullOrWhiteSpace(displayName) ? playerId : displayName;
    }

    private static BoardPlayerType GetSessionPlayerType(SerializedProperty sessionPlayerProperty)
    {
        return (BoardPlayerType)sessionPlayerProperty.FindPropertyRelative("m_type").enumValueIndex;
    }

    private void InitializeGuestSessionPlayer(
        SerializedProperty profilesProperty,
        SerializedProperty sessionPlayersProperty,
        SerializedProperty sessionPlayerProperty,
        int currentIndex)
    {
        string guestPlayerId = AllocateNextGuestPlayerId(profilesProperty, sessionPlayersProperty, currentIndex);
        string avatarId = AllocateNextSessionAvatarId(profilesProperty, sessionPlayersProperty, currentIndex);
        sessionPlayerProperty.FindPropertyRelative("m_type").enumValueIndex = (int)BoardPlayerType.Guest;
        sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue = guestPlayerId;
        sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue = GetDefaultGuestDisplayName(guestPlayerId);
        sessionPlayerProperty.FindPropertyRelative("m_avatarId").stringValue = avatarId;
        sessionPlayerProperty.FindPropertyRelative("m_avatar").objectReferenceValue = null;
        sessionPlayerProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue = ResolveDefaultSessionAvatarBackgroundColor(guestPlayerId, avatarId);
    }

    private string AllocateNextGuestPlayerId(
        SerializedProperty profilesProperty,
        SerializedProperty sessionPlayersProperty,
        int ignoredIndex = -1)
    {
        HashSet<string> reservedIds = new(GetAllProfileIds(profilesProperty), StringComparer.Ordinal);
        for(int index = 0; index < sessionPlayersProperty.arraySize; index++)
        {
            if(index == ignoredIndex)
            {
                continue;
            }

            string playerId = sessionPlayersProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_playerId").stringValue;
            if(string.IsNullOrWhiteSpace(playerId) == false)
            {
                reservedIds.Add(playerId);
            }
        }

        int guestIndex = 1;
        string candidate = $"guest-{guestIndex:000}";
        while(reservedIds.Contains(candidate))
        {
            guestIndex++;
            candidate = $"guest-{guestIndex:000}";
        }

        return candidate;
    }

    private string AllocateNextGuestPlayerId(SerializedProperty profilesProperty, IEnumerable<SessionAuthoringPlayer> players)
    {
        HashSet<string> reservedIds = new(GetAllProfileIds(profilesProperty), StringComparer.Ordinal);
        foreach(SessionAuthoringPlayer player in players)
        {
            if(string.IsNullOrWhiteSpace(player?.PlayerId) == false)
            {
                reservedIds.Add(player.PlayerId);
            }
        }

        int guestIndex = 1;
        string candidate = $"guest-{guestIndex:000}";
        while(reservedIds.Contains(candidate))
        {
            guestIndex++;
            candidate = $"guest-{guestIndex:000}";
        }

        return candidate;
    }

    private string AllocateNextSessionAvatarId(
        SerializedProperty profilesProperty,
        SerializedProperty sessionPlayersProperty,
        int ignoredIndex = -1)
    {
        HashSet<string> assignedAvatarIds = new(GetAllAvatarIds(profilesProperty), StringComparer.Ordinal);
        for(int index = 0; index < sessionPlayersProperty.arraySize; index++)
        {
            if(index == ignoredIndex)
            {
                continue;
            }

            string avatarId = sessionPlayersProperty.GetArrayElementAtIndex(index).FindPropertyRelative("m_avatarId").stringValue;
            if(string.IsNullOrWhiteSpace(avatarId) == false)
            {
                assignedAvatarIds.Add(avatarId);
            }
        }

        return AllocateNextAvatarId(profilesProperty, assignedAvatarIds);
    }

    private string AllocateNextSessionAvatarId(SerializedProperty profilesProperty, IEnumerable<SessionAuthoringPlayer> players)
    {
        HashSet<string> assignedAvatarIds = new(GetAllAvatarIds(profilesProperty), StringComparer.Ordinal);
        foreach(SessionAuthoringPlayer player in players)
        {
            if(string.IsNullOrWhiteSpace(player?.AvatarId) == false)
            {
                assignedAvatarIds.Add(player.AvatarId);
            }
        }

        return AllocateNextAvatarId(profilesProperty, assignedAvatarIds);
    }

    private static string GetDefaultGuestDisplayName(string playerId)
    {
        if(string.IsNullOrWhiteSpace(playerId) == false
        && playerId.StartsWith("guest-", StringComparison.Ordinal)
        && int.TryParse(playerId["guest-".Length..], out int guestIndex))
        {
            return $"Player {guestIndex}";
        }

        return "Player";
    }

    private static string NormalizeGuestDisplayName(string displayName)
    {
        return displayName?.Trim();
    }

    private static string ResolveUniqueGuestDisplayName(
        string desiredDisplayName,
        string defaultDisplayName,
        ISet<string> reservedDisplayNames)
    {
        if(string.IsNullOrWhiteSpace(desiredDisplayName) == false
        && reservedDisplayNames.Contains(desiredDisplayName) == false)
        {
            return desiredDisplayName;
        }

        string normalizedDefaultDisplayName = NormalizeGuestDisplayName(defaultDisplayName);
        if(string.IsNullOrWhiteSpace(normalizedDefaultDisplayName) == false
        && reservedDisplayNames.Contains(normalizedDefaultDisplayName) == false)
        {
            return normalizedDefaultDisplayName;
        }

        int suffix = 1;
        while(true)
        {
            string candidate = $"Player {suffix}";
            if(reservedDisplayNames.Contains(candidate) == false)
            {
                return candidate;
            }

            suffix++;
        }
    }

    private bool IsGuestDisplayNameAvailable(string displayName, int excludedSessionPlayerIndex)
    {
        if(string.IsNullOrWhiteSpace(displayName))
        {
            return false;
        }

        SerializedProperty profilesProperty = FindRequiredProperty(ProfilesPropertyPath);
        for(int profileIndex = 0; profileIndex < profilesProperty.arraySize; profileIndex++)
        {
            string existingProfileDisplayName = profilesProperty.GetArrayElementAtIndex(profileIndex)
                .FindPropertyRelative("m_displayName")
                .stringValue;
            if(string.Equals(existingProfileDisplayName, displayName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        SerializedProperty sessionPlayersProperty = FindRequiredProperty(SessionPropertyPath).FindPropertyRelative("m_players");
        for(int sessionIndex = 0; sessionIndex < sessionPlayersProperty.arraySize; sessionIndex++)
        {
            if(sessionIndex == excludedSessionPlayerIndex)
            {
                continue;
            }

            SerializedProperty sessionPlayerProperty = sessionPlayersProperty.GetArrayElementAtIndex(sessionIndex);
            if(GetSessionPlayerType(sessionPlayerProperty) != BoardPlayerType.Guest)
            {
                continue;
            }

            string existingGuestDisplayName = NormalizeGuestDisplayName(sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue);
            if(string.Equals(existingGuestDisplayName, displayName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static Color ResolveDefaultSessionAvatarBackgroundColor(string playerId, string avatarId)
    {
        if(int.TryParse(avatarId, out int avatarIndex))
        {
            return EmulatorAvatarUtility.GetPaletteColor(avatarIndex);
        }

        if(string.IsNullOrWhiteSpace(playerId) == false
        && playerId.StartsWith("guest-", StringComparison.Ordinal)
        && int.TryParse(playerId["guest-".Length..], out int guestIndex))
        {
            return EmulatorAvatarUtility.GetPaletteColor(Math.Max(0, guestIndex - 1));
        }

        return EmulatorAvatarUtility.GetPaletteColor(0);
    }

    private static SessionAuthoringPlayer ReadSessionAuthoringPlayer(SerializedProperty sessionPlayerProperty)
    {
        if(sessionPlayerProperty == null)
        {
            return null;
        }

        return new SessionAuthoringPlayer
        {
            SessionId = sessionPlayerProperty.FindPropertyRelative("m_sessionId").intValue,
            PlayerId = sessionPlayerProperty.FindPropertyRelative("m_playerId").stringValue,
            Type = GetSessionPlayerType(sessionPlayerProperty),
            DisplayName = sessionPlayerProperty.FindPropertyRelative("m_displayName").stringValue,
            AvatarId = sessionPlayerProperty.FindPropertyRelative("m_avatarId").stringValue,
            Avatar = sessionPlayerProperty.FindPropertyRelative("m_avatar").objectReferenceValue as Texture2D,
            AvatarBackgroundColor = sessionPlayerProperty.FindPropertyRelative("m_avatarBackgroundColor").colorValue
        };
    }

    private static bool WriteSessionAuthoringPlayer(SerializedProperty sessionPlayerProperty, SessionAuthoringPlayer player)
    {
        bool changed = false;
        changed |= SetSerializedInt(sessionPlayerProperty.FindPropertyRelative("m_sessionId"), player.SessionId);
        changed |= SetSerializedString(sessionPlayerProperty.FindPropertyRelative("m_playerId"), player.PlayerId);
        changed |= SetSerializedEnum(sessionPlayerProperty.FindPropertyRelative("m_type"), (int)player.Type);
        changed |= SetSerializedString(sessionPlayerProperty.FindPropertyRelative("m_displayName"), player.DisplayName);
        changed |= SetSerializedString(sessionPlayerProperty.FindPropertyRelative("m_avatarId"), player.AvatarId);
        changed |= SetSerializedObjectReference(sessionPlayerProperty.FindPropertyRelative("m_avatar"), player.Avatar);
        changed |= SetSerializedColor(sessionPlayerProperty.FindPropertyRelative("m_avatarBackgroundColor"), player.AvatarBackgroundColor);
        return changed;
    }

    private static bool SetSerializedString(SerializedProperty property, string value)
    {
        string normalizedValue = value ?? string.Empty;
        if(string.Equals(property.stringValue, normalizedValue, StringComparison.Ordinal))
        {
            return false;
        }

        property.stringValue = normalizedValue;
        return true;
    }

    private static bool SetSerializedInt(SerializedProperty property, int value)
    {
        if(property.intValue == value)
        {
            return false;
        }

        property.intValue = value;
        return true;
    }

    private static bool SetSerializedEnum(SerializedProperty property, int value)
    {
        if(property.enumValueIndex == value)
        {
            return false;
        }

        property.enumValueIndex = value;
        return true;
    }

    private static bool SetSerializedObjectReference(SerializedProperty property, UnityEngine.Object value)
    {
        if(property.objectReferenceValue == value)
        {
            return false;
        }

        property.objectReferenceValue = value;
        return true;
    }

    private static bool SetSerializedColor(SerializedProperty property, Color value)
    {
        if(property.colorValue == value)
        {
            return false;
        }

        property.colorValue = value;
        return true;
    }

    private SerializedProperty FindRequiredProperty(string propertyPath)
    {
        SerializedProperty property = _serializedMockDataAsset.FindProperty(propertyPath);
        if(property == null)
        {
            throw new InvalidOperationException($"Required serialized property '{propertyPath}' was not found on '{nameof(EmulatorMockDataAsset)}'.");
        }

        return property;
    }

    private static string GetProfileCardTitle(string displayName, int index)
    {
        return string.IsNullOrWhiteSpace(displayName) ? $"Profile {index + 1}" : displayName;
    }

    private static void ConfigureAlignedField(VisualElement field, string tooltip)
    {
        field.AddToClassList(AlignedFieldUssClassName);
        ApplyTooltip(field, tooltip);
    }

    private static void ApplyTooltip(VisualElement field, string tooltip)
    {
        if(string.IsNullOrWhiteSpace(tooltip))
        {
            return;
        }

        field.tooltip = tooltip;
        Label fieldLabel = field.Q<Label>(className: BaseFieldLabelUssClassName)
                           ?? field.Q<Label>(className: PropertyFieldLabelUssClassName);
        if(fieldLabel != null)
        {
            fieldLabel.tooltip = tooltip;
        }
    }

    private static void ApplyInspectorFieldPresentation(VisualElement root, bool flattenTopLevelFoldout = false)
    {
        if(root == null)
        {
            return;
        }

        if(flattenTopLevelFoldout)
        {
            FlattenTopLevelFoldout(root);
        }

        foreach(VisualElement field in root.Query<VisualElement>(className: BaseField<string>.ussClassName).Build())
        {
            field.AddToClassList(AlignedFieldUssClassName);
        }

        foreach(BindableElement bindableElement in root.Query<BindableElement>().Build().Where(element => string.IsNullOrWhiteSpace(element.bindingPath) == false))
        {
            if(TooltipByPropertyName.TryGetValue(GetPropertyName(bindableElement.bindingPath), out string tooltip) == false)
            {
                continue;
            }

            bindableElement.tooltip = tooltip;

            Label fieldLabel = bindableElement.Q<Label>(className: BaseFieldLabelUssClassName)
                               ?? bindableElement.Q<Label>(className: PropertyFieldLabelUssClassName);
            if(fieldLabel != null)
            {
                fieldLabel.tooltip = tooltip;
            }
        }
    }

    private static string GetPropertyName(string bindingPath)
    {
        string[] segments = bindingPath.Split('.');
        for(int i = segments.Length - 1; i >= 0; i--)
        {
            string segment = segments[i];
            if(segment == "Array" || segment.StartsWith("data[", StringComparison.Ordinal))
            {
                continue;
            }

            return segment;
        }

        return bindingPath;
    }

    private static void FlattenTopLevelFoldout(VisualElement root)
    {
        Foldout foldout = root.Children().OfType<Foldout>().FirstOrDefault();
        if(foldout == null)
        {
            return;
        }

        foldout.value = true;

        Toggle headerToggle = foldout.Q<Toggle>();
        if(headerToggle != null)
        {
            headerToggle.style.display = DisplayStyle.None;
        }

        foldout.style.marginTop = 0f;
        foldout.style.paddingTop = 0f;
    }

    private sealed class SessionAuthoringPlayer
    {
        public int SessionId { get; set; }
        public string PlayerId { get; set; }
        public BoardPlayerType Type { get; set; }
        public string DisplayName { get; set; }
        public string AvatarId { get; set; }
        public Texture2D Avatar { get; set; }
        public Color AvatarBackgroundColor { get; set; }
    }
}
}
