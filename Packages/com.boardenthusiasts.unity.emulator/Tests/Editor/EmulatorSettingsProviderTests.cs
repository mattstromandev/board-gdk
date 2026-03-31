using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Board.Core;
using BE.Emulator.Data;
using BE.Emulator.Editor.Settings;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator.Editor.Tests
{
public sealed class EmulatorSettingsProviderTests
{
    [Test]
    public void ProfilesProvider_RebuildsAfterExternallyDeletedProfile_WhenInitialChangedEventIsSuppressed()
    {
        EmulatorMockDataAsset mockDataAsset = ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        mockDataAsset.Replace(CreateMockData());

        EmulatorSettingsProvider provider = EmulatorSettingsProvider.CreateProfiles(mockDataAsset, Array.Empty<string>());
        VisualElement root = new();

        try
        {
            provider.OnActivate(string.Empty, root);
            Assert.That(GetProfileCardCount(root), Is.EqualTo(3));

            SetSuppressedChangedEventCount(provider, 1);
            mockDataAsset.Replace(new EmulatorMockData
            {
                Profiles = new List<EmulatorProfileData>
                {
                    new()
                    {
                        PlayerId = "profile-001",
                        DisplayName = "Player1",
                        AvatarId = "0"
                    },
                    new()
                    {
                        PlayerId = "profile-003",
                        DisplayName = "Player3",
                        AvatarId = "2"
                    }
                },
                Session = new EmulatorSessionData
                {
                    ActiveProfileId = "profile-001"
                }
            });

            provider.OnInspectorUpdate();
            InvokePrivate(provider, "RebuildSectionDelayed");

            Assert.That(GetProfileCardCount(root), Is.EqualTo(2));
        }
        finally
        {
            provider.OnDeactivate();
            UnityEngine.Object.DestroyImmediate(mockDataAsset);
        }
    }

    [Test]
    public void ProfilesProvider_ProfileRenameTargetsCurrentAssetAfterExternalReplace()
    {
        EmulatorMockDataAsset mockDataAsset = ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        mockDataAsset.Replace(CreateMockData());

        EmulatorSettingsProvider provider = EmulatorSettingsProvider.CreateProfiles(mockDataAsset, Array.Empty<string>());
        VisualElement root = new();

        try
        {
            provider.OnActivate(string.Empty, root);

            mockDataAsset.Replace(CreateMockData());
            InvokePrivate(provider, "SetProfileDisplayName", "profile-002", "MyProfile");

            Assert.That(mockDataAsset.Data.Profiles[1].DisplayName, Is.EqualTo("MyProfile"));
        }
        finally
        {
            provider.OnDeactivate();
            UnityEngine.Object.DestroyImmediate(mockDataAsset);
        }
    }

    [Test]
    public void SessionProvider_FirstSessionPlayerProfileChange_DoesNotRewriteActiveProfile()
    {
        EmulatorMockDataAsset mockDataAsset = ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        mockDataAsset.Replace(CreateSessionMockData());

        EmulatorSettingsProvider provider = EmulatorSettingsProvider.CreateSession(mockDataAsset, Array.Empty<string>());
        VisualElement root = new();

        try
        {
            provider.OnActivate(string.Empty, root);

            InvokePrivate(provider, "SetSessionPlayerProfile", 0, "profile-002");

            Assert.That(mockDataAsset.Data.Session.ActiveProfileId, Is.EqualTo("profile-003"));
            Assert.That(mockDataAsset.Data.Session.Players[0].PlayerId, Is.EqualTo("profile-002"));
        }
        finally
        {
            provider.OnDeactivate();
            UnityEngine.Object.DestroyImmediate(mockDataAsset);
        }
    }

    [Test]
    public void SessionProvider_LastNonGuestPlayer_DisablesTypeAndRemoveWithTooltip()
    {
        EmulatorMockDataAsset mockDataAsset = ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        mockDataAsset.Replace(new EmulatorMockData
        {
            Profiles = CreateMockData().Profiles,
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-003",
                Players = new List<EmulatorSessionPlayerData>
                {
                    new()
                    {
                        SessionId = 1,
                        PlayerId = "profile-001",
                        Type = BoardPlayerType.Profile
                    }
                }
            }
        });

        EmulatorSettingsProvider provider = EmulatorSettingsProvider.CreateSession(mockDataAsset, Array.Empty<string>());
        VisualElement root = new();

        try
        {
            provider.OnActivate(string.Empty, root);

            Button removeButton = root.Query<Button>().ToList().First(button => button.text == "Remove");
            EnumField typeField = root.Q<EnumField>();

            Assert.That(removeButton.enabledSelf, Is.False);
            Assert.That(removeButton.tooltip, Is.EqualTo("Session must included at least one non-guest player"));
            Assert.That(typeField, Is.Not.Null);
            Assert.That(typeField.enabledSelf, Is.False);
            Assert.That(typeField.tooltip, Is.EqualTo("Session must included at least one non-guest player"));
        }
        finally
        {
            provider.OnDeactivate();
            UnityEngine.Object.DestroyImmediate(mockDataAsset);
        }
    }

    [Test]
    public void SessionProvider_GuestPlayerDefaultsToPlayerNumbering()
    {
        EmulatorMockDataAsset mockDataAsset = ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        mockDataAsset.Replace(CreateSessionMockData());

        EmulatorSettingsProvider provider = EmulatorSettingsProvider.CreateSession(mockDataAsset, Array.Empty<string>());
        VisualElement root = new();

        try
        {
            provider.OnActivate(string.Empty, root);

            InvokePrivate(provider, "SetSessionPlayerType", 1, BoardPlayerType.Guest);

            Assert.That(mockDataAsset.Data.Session.Players[1].Type, Is.EqualTo(BoardPlayerType.Guest));
            Assert.That(mockDataAsset.Data.Session.Players[1].DisplayName, Is.EqualTo("Player 1"));
        }
        finally
        {
            provider.OnDeactivate();
            UnityEngine.Object.DestroyImmediate(mockDataAsset);
        }
    }

    [Test]
    public void ProjectSettingsAssetUtility_NormalizesMockDataAssetName()
    {
        EmulatorMockDataAsset mockDataAsset = ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        mockDataAsset.name = "BoardOSEmulatorMockData";

        try
        {
            bool changed = EmulatorProjectSettingsAssetUtility.EnsureMockDataAssetNameMatchesFileName(mockDataAsset);

            Assert.That(changed, Is.True);
            Assert.That(mockDataAsset.name, Is.EqualTo("BE Emulator Mock Data"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(mockDataAsset);
        }
    }

    private static EmulatorMockData CreateMockData()
    {
        return new EmulatorMockData
        {
            Profiles = new List<EmulatorProfileData>
            {
                new()
                {
                    PlayerId = "profile-001",
                    DisplayName = "Player1",
                    AvatarId = "0"
                },
                new()
                {
                    PlayerId = "profile-002",
                    DisplayName = "Test1",
                    AvatarId = "1"
                },
                new()
                {
                    PlayerId = "profile-003",
                    DisplayName = "Player3",
                    AvatarId = "2"
                }
            },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-001"
            }
        };
    }

    private static EmulatorMockData CreateSessionMockData()
    {
        EmulatorMockData mockData = CreateMockData();
        mockData.Session = new EmulatorSessionData
        {
            ActiveProfileId = "profile-003",
            Players = new List<EmulatorSessionPlayerData>
            {
                new()
                {
                    SessionId = 1,
                    PlayerId = "profile-001",
                    Type = BoardPlayerType.Profile
                },
                new()
                {
                    SessionId = 2,
                    PlayerId = "profile-002",
                    Type = BoardPlayerType.Profile
                }
            }
        };
        return mockData;
    }

    private static int GetProfileCardCount(VisualElement root)
    {
        ScrollView scrollView = root.Q<ScrollView>();
        Assert.That(scrollView, Is.Not.Null);
        return scrollView.Query<Box>().ToList().Count;
    }

    private static void SetSuppressedChangedEventCount(EmulatorSettingsProvider provider, int count)
    {
        FieldInfo field = typeof(EmulatorSettingsProvider).GetField("_suppressedAssetChangedEventsRemaining", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        field.SetValue(provider, count);
    }

    private static void InvokePrivate(object instance, string methodName, params object[] arguments)
    {
        MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        method.Invoke(instance, arguments);
    }
}
}
