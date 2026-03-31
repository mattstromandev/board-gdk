using System;

using BE.Emulator.Actions;
using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.DeleteProfile
{
/// <summary>
/// View wrapper for the delete-profile modal.
/// </summary>
internal sealed class DeleteProfileModalView : BaseView<DeleteProfileModalViewModel>
{
    /// <summary>
    /// The USS root class applied to the modal.
    /// </summary>
    public const string RootUssClassName = "delete-profile-modal-root";

    /// <summary>
    /// The USS panel class applied to the modal content.
    /// </summary>
    public const string PanelUssClassName = "delete-profile-modal";

    /// <summary>
    /// The element name used for the backdrop dismiss target.
    /// </summary>
    public const string BackdropName = "dismiss-delete-profile";

    /// <summary>
    /// The element name used for the title label.
    /// </summary>
    public const string TitleName = "delete-profile-title";

    /// <summary>
    /// The element name used for the body label.
    /// </summary>
    public const string BodyName = "delete-profile-body";

    /// <summary>
    /// The element name used for the delete button.
    /// </summary>
    public const string DeleteButtonName = "delete-profile";

    /// <summary>
    /// The element name used for the cancel button.
    /// </summary>
    public const string CancelButtonName = "cancel-delete-profile";

    /// <summary>
    /// The element name used for the delete button text label.
    /// </summary>
    public const string DeleteButtonTextName = "delete-profile-label";

    /// <summary>
    /// The element name used for the delete button dots label.
    /// </summary>
    public const string DeleteButtonDotsName = "delete-profile-dots";

    /// <summary>
    /// The modifier applied when the modal is visible.
    /// </summary>
    public const string OpenModifierName = "--open";

    /// <summary>
    /// The modifier applied while the delete operation is in progress.
    /// </summary>
    public const string DeletingModifierName = "--deleting";

    /// <inheritdoc />
    protected override string StyleClassName => RootUssClassName;

    private const int DeleteDelayMilliseconds = 650;

    private VisualElement _root;
    private VisualElement _backdrop;
    private Label _titleLabel;
    private Label _bodyLabel;
    private Button _deleteButton;
    private Button _cancelButton;
    private Label _deleteButtonTextLabel;
    private Label _deleteButtonDotsLabel;
    private EventCallback<PointerDownEvent> _stopPropagationCallback;
    private IVisualElementScheduledItem _deleteDotsSchedule;
    private IVisualElementScheduledItem _deleteCompletionSchedule;
    private DeleteProfileModalState _state = new();
    private int _deleteDotsFrame;
    private bool _isDeleting;

    /// <summary>
    /// Creates the delete-profile modal view.
    /// </summary>
    public DeleteProfileModalView([NotNull] ILoggerFactory loggerFactory, [NotNull] DeleteProfileModalViewModel viewModel)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
    }

    /// <summary>
    /// Applies the supplied modal state to the view.
    /// </summary>
    public void SetModalState(DeleteProfileModalState state)
    {
        _state = state ?? new DeleteProfileModalState();
        ApplyModalState();
    }

    /// <summary>
    /// Begins the temporary deleting state and invokes the supplied callback after the emulator delay.
    /// </summary>
    public void BeginDeleteFlow(Action onComplete)
    {
        if(_root == null || _isDeleting)
        {
            return;
        }

        _isDeleting = true;
        _deleteDotsFrame = 0;
        _root.EnableInClassList(DeletingModifierName, true);
        _backdrop?.SetEnabled(false);
        _deleteButton?.SetEnabled(false);
        _cancelButton?.SetEnabled(false);
        RefreshDeleteDots();

        _deleteDotsSchedule?.Pause();
        _deleteDotsSchedule = _root.schedule.Execute(UpdateDeleteDots).Every(180);

        _deleteCompletionSchedule?.Pause();
        _deleteCompletionSchedule = _root.schedule.Execute(() =>
        {
            _deleteDotsSchedule?.Pause();
            _deleteCompletionSchedule?.Pause();
            onComplete?.Invoke();
        }).StartingIn(DeleteDelayMilliseconds);
    }

    /// <summary>
    /// Ends the temporary deleting state without closing the modal.
    /// </summary>
    public void EndDeleteFlow()
    {
        _isDeleting = false;
        _deleteDotsFrame = 0;
        _deleteDotsSchedule?.Pause();
        _deleteCompletionSchedule?.Pause();

        if(_root != null)
        {
            _root.EnableInClassList(DeletingModifierName, false);
        }

        _backdrop?.SetEnabled(true);
        _deleteButton?.SetEnabled(true);
        _cancelButton?.SetEnabled(true);

        RefreshDeleteDots();
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root;
        _backdrop = root.Q(BackdropName);
        _titleLabel = root.Q<Label>(TitleName);
        _bodyLabel = root.Q<Label>(BodyName);
        _deleteButton = root.Q<Button>(DeleteButtonName);
        _cancelButton = root.Q<Button>(CancelButtonName);
        _deleteButtonTextLabel = root.Q<Label>(DeleteButtonTextName);
        _deleteButtonDotsLabel = root.Q<Label>(DeleteButtonDotsName);

        _stopPropagationCallback = evt => evt.StopPropagation();
        _root.RegisterCallback(_stopPropagationCallback);

        ApplyModalState();
        EndDeleteFlow();

        RegisterViewAction<PointerDownEvent>(_backdrop, new CloseDeleteProfileViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction(_cancelButton, new CloseDeleteProfileViewAction());
        RegisterViewAction(_deleteButton, () => new DeleteProfileViewAction
        {
            SelectedProfileId = _state.SelectedProfileId
        });
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        if(_root != null && _stopPropagationCallback != null)
        {
            _root.UnregisterCallback(_stopPropagationCallback);
        }

        EndDeleteFlow();
        _backdrop = null;
        _titleLabel = null;
        _bodyLabel = null;
        _deleteButton = null;
        _cancelButton = null;
        _deleteButtonTextLabel = null;
        _deleteButtonDotsLabel = null;
        _stopPropagationCallback = null;
        _root = null;
    }

    /// <inheritdoc />
    protected override void ShowView()
    {
        EndDeleteFlow();
        ApplyModalState();
        _root.AddToClassList(OpenModifierName);
    }

    /// <inheritdoc />
    protected override void HideView()
    {
        EndDeleteFlow();
        _root.RemoveFromClassList(OpenModifierName);
    }

    private void UpdateDeleteDots()
    {
        _deleteDotsFrame = (_deleteDotsFrame + 1) % 3;
        RefreshDeleteDots();
    }

    private void RefreshDeleteDots()
    {
        if(_deleteButtonDotsLabel == null)
        {
            return;
        }

        _deleteButtonDotsLabel.text = _deleteDotsFrame switch
        {
            0 => ". . .",
            1 => ".  .",
            _ => " . ."
        };
    }

    private void ApplyModalState()
    {
        if(_titleLabel != null)
        {
            _titleLabel.text = _state.TitleText ?? string.Empty;
        }

        if(_bodyLabel != null)
        {
            _bodyLabel.text = _state.BodyText ?? string.Empty;
        }
    }
}
}
