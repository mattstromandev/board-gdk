using System;

using BE.Emulator.Actions;
using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator.Modals.EditProfile
{
/// <summary>
/// View wrapper for the shared create and edit profile modal.
/// </summary>
internal sealed class EditProfileModalView : BaseView<EditProfileModalViewModel>
{
    /// <summary>
    /// The USS class applied to the full-screen edit-profile modal host.
    /// </summary>
    public const string RootUssClassName = "edit-profile-modal-root";

    /// <summary>
    /// The USS class applied to the right-side panel.
    /// </summary>
    public const string PanelUssClassName = "edit-profile-modal";

    /// <summary>
    /// The element name used for the backdrop dismiss target.
    /// </summary>
    public const string BackdropName = "dismiss-edit-profile";

    /// <summary>
    /// The element name used for the close button.
    /// </summary>
    public const string CloseButtonName = "close-edit-profile";

    /// <summary>
    /// The element name used for the title label.
    /// </summary>
    public const string TitleName = "edit-profile-title";

    /// <summary>
    /// The element name used for the avatar shuffle button.
    /// </summary>
    public const string ShuffleButtonName = "shuffle-avatar-color";

    /// <summary>
    /// The element name used for the nickname field.
    /// </summary>
    public const string NicknameFieldName = "profile-nickname";

    /// <summary>
    /// The element name used for the placeholder label.
    /// </summary>
    public const string PlaceholderName = "profile-nickname-placeholder";

    /// <summary>
    /// The element name used for the validation help text.
    /// </summary>
    public const string ValidationMessageName = "profile-nickname-help";

    /// <summary>
    /// The element name used for the avatar preview.
    /// </summary>
    public const string AvatarPreviewName = "edit-profile-avatar";

    /// <summary>
    /// The element name used for the save button.
    /// </summary>
    public const string SaveButtonName = "save-profile";

    /// <summary>
    /// The modifier applied when the modal is visible.
    /// </summary>
    public const string OpenModifierName = "--open";

    private const string PlaceholderHiddenModifierName = "--placeholder-hidden";
    private const string InvalidModifierName = "--invalid";
    private const string SaveEnabledModifierName = "--enabled";

    /// <inheritdoc />
    protected override string StyleClassName => RootUssClassName;

    private readonly EditProfileModalModel _model;
    private VisualElement _root;
    private VisualElement _panel;
    private VisualElement _avatarPreview;
    private Label _titleLabel;
    private VisualElement _placeholderLabel;
    private TextField _nicknameField;
    private Label _validationMessageLabel;
    private Button _shuffleAvatarButton;
    private Button _saveButton;
    private EventCallback<PointerDownEvent> _stopPropagationCallback;
    private bool _isNicknameFocused;
    private bool _hasDuplicateValidationError;
    private Color _avatarBackgroundColor;
    private EditProfileModalState _openState;

    /// <summary>
    /// Creates the shared edit-profile modal view.
    /// </summary>
    public EditProfileModalView(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] EditProfileModalViewModel viewModel,
        [NotNull] EditProfileModalModel model)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _openState = _model.CreateCreateState();
    }

    /// <summary>
    /// Applies the supplied modal open state.
    /// </summary>
    public void SetOpenState(EditProfileModalState openState)
    {
        _openState = openState ?? _model.CreateCreateState();
        if(_root != null)
        {
            ResetForm();
        }
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root;
        _panel = root.Q(className: PanelUssClassName);
        _avatarPreview = root.Q(AvatarPreviewName);
        _titleLabel = root.Q<Label>(TitleName);
        _placeholderLabel = root.Q(PlaceholderName);
        _nicknameField = root.Q<TextField>(NicknameFieldName);
        _validationMessageLabel = root.Q<Label>(ValidationMessageName);
        _shuffleAvatarButton = root.Q<Button>(ShuffleButtonName);
        _saveButton = root.Q<Button>(SaveButtonName);
        VisualElement backdrop = root.Q(BackdropName);
        VisualElement closeButton = root.Q(CloseButtonName, EmulatorView.ButtonUssClassName);

        _stopPropagationCallback = evt => evt.StopPropagation();
        _root.RegisterCallback(_stopPropagationCallback);

        ConfigureNicknameField();
        ResetForm();

        if(_shuffleAvatarButton != null)
        {
            _shuffleAvatarButton.clicked += ShuffleAvatarBackgroundColor;
        }

        RegisterViewAction<PointerDownEvent>(backdrop, new CloseEditProfileViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<PointerDownEvent>(closeButton, new CloseEditProfileViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(closeButton, new CloseEditProfileViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction(_saveButton, () => new EditProfileSavedViewAction
        {
            Mode = _openState.Mode,
            SelectedProfileId = _openState.SelectedProfileId,
            DisplayName = _nicknameField?.value,
            AvatarBackgroundColor = _avatarBackgroundColor
        });
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        if(_shuffleAvatarButton != null)
        {
            _shuffleAvatarButton.clicked -= ShuffleAvatarBackgroundColor;
        }

        if(_root != null && _stopPropagationCallback != null)
        {
            _root.UnregisterCallback(_stopPropagationCallback);
        }

        ResetForm();
        _avatarPreview = null;
        _titleLabel = null;
        _nicknameField = null;
        _placeholderLabel = null;
        _validationMessageLabel = null;
        _shuffleAvatarButton = null;
        _saveButton = null;
        _stopPropagationCallback = null;
        _panel = null;
        _root = null;
    }

    /// <inheritdoc />
    protected override void ShowView()
    {
        ResetForm();
        _root.AddToClassList(OpenModifierName);
    }

    /// <inheritdoc />
    protected override void HideView()
    {
        ResetForm();
        _root.RemoveFromClassList(OpenModifierName);
    }

    private void ConfigureNicknameField()
    {
        if(_nicknameField == null)
        {
            return;
        }

        _nicknameField.isDelayed = false;
        _nicknameField.RegisterValueChangedCallback(OnNicknameChanged);
        _nicknameField.RegisterCallback<FocusInEvent>(_ =>
        {
            _isNicknameFocused = true;
            RefreshPlaceholderVisibility();
        });
        _nicknameField.RegisterCallback<FocusOutEvent>(_ =>
        {
            _isNicknameFocused = false;
            RefreshPlaceholderVisibility();
        });
    }

    private void OnNicknameChanged(ChangeEvent<string> eventArgs)
    {
        _hasDuplicateValidationError = false;
        RefreshValidationState(eventArgs.newValue);
    }

    /// <summary>
    /// Shows the duplicate-nickname validation state after a failed save attempt.
    /// </summary>
    public void ShowDuplicateNicknameValidation()
    {
        _hasDuplicateValidationError = true;
        RefreshValidationState(_nicknameField?.value ?? string.Empty);
    }

    private void ShuffleAvatarBackgroundColor()
    {
        _avatarBackgroundColor = _model.GetRandomAvatarBackgroundColor(_avatarBackgroundColor);
        if(_avatarPreview != null)
        {
            _avatarPreview.style.backgroundColor = new StyleColor(_avatarBackgroundColor);
        }
    }

    private void ResetForm()
    {
        _isNicknameFocused = false;
        _hasDuplicateValidationError = false;
        _avatarBackgroundColor = BE.Emulator.Utility.EmulatorAvatarUtility.HasSerializedColor(_openState.AvatarBackgroundColor)
            ? _openState.AvatarBackgroundColor
            : _model.GetDefaultAvatarBackgroundColor();

        if(_titleLabel != null)
        {
            _titleLabel.text = _openState.Title;
        }

        if(_nicknameField != null)
        {
            _nicknameField.SetValueWithoutNotify(_openState.DisplayName ?? string.Empty);
        }

        if(_avatarPreview != null)
        {
            _avatarPreview.style.backgroundColor = new StyleColor(_avatarBackgroundColor);
        }

        RefreshValidationState(_nicknameField?.value ?? _openState.DisplayName ?? string.Empty);
    }

    private void RefreshValidationState(string nickname)
    {
        bool hasNickname = string.IsNullOrEmpty(nickname) == false;
        bool isFormatValid = _model.IsValidNickname(nickname);
        bool isValid = isFormatValid && _hasDuplicateValidationError == false;
        bool showInvalidState = hasNickname && (isFormatValid == false || _hasDuplicateValidationError);

        _panel?.EnableInClassList(InvalidModifierName, showInvalidState);
        _validationMessageLabel?.EnableInClassList(InvalidModifierName, showInvalidState);
        _saveButton?.EnableInClassList(SaveEnabledModifierName, isValid);
        _saveButton?.SetEnabled(isValid);

        if(_validationMessageLabel != null)
        {
            _validationMessageLabel.text = _hasDuplicateValidationError
                ? EditProfileModalModel.DuplicateValidationMessage
                : EditProfileModalModel.ValidationMessage;
        }

        RefreshPlaceholderVisibility();
    }

    private void RefreshPlaceholderVisibility()
    {
        bool hidePlaceholder = _isNicknameFocused || string.IsNullOrEmpty(_nicknameField?.value) == false;
        _placeholderLabel?.EnableInClassList(PlaceholderHiddenModifierName, hidePlaceholder);
    }
}
}
