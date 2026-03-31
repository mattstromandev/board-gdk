using System;
using System.Collections.Generic;

using BE.Emulator.Actions;
using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;
using Rahmen.UIToolkit.Extensions;

using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator.Modals.ManageProfiles
{
/// <summary>
/// View wrapper for the manage profiles modal.
/// </summary>
internal sealed class ManageProfilesModalView : BaseView<ManageProfilesModalViewModel>
{
    /// <summary>
    /// The USS root class applied to the modal.
    /// </summary>
    public const string UssClassName = "manage-profiles-modal";
    /// <summary>
    /// The element name used for the close button.
    /// </summary>
    public const string CloseButtonName = "close-manage-profiles";
    /// <summary>
    /// The element name used for the new-profile button.
    /// </summary>
    public const string NewProfileButtonName = "new-profile";
    /// <summary>
    /// The USS class applied to the profile list scroll view.
    /// </summary>
    public const string ProfileListClassName = UssClassName + "__profiles";
    /// <summary>
    /// The USS class applied to individual profile items.
    /// </summary>
    public const string ProfileItemClassName = "manage-profiles-profile-item";
    public const string ProfileItemDisplayNameClassName = "manage-profiles-profile-item__display-name";
    /// <summary>
    /// The modifier applied when the modal is visible.
    /// </summary>
    public const string OpenModifierName = "--open";
    private const float ProfileListDragThreshold = 8f;

    /// <inheritdoc />
    protected override string StyleClassName => UssClassName;

    private VisualElement _root;
    private VisualElement _closeButton;
    private Button _newProfileButton;
    private ScrollView _profilesScrollView;
    private EventCallback<PointerDownEvent> _stopPropagationCallback;
    private readonly List<Action> _profileListInteractionUnregisters = new();
    private int _profileListPointerId = -1;
    private Vector2 _profileListStartPosition;
    private float _profileListStartOffsetY;
    private string _pendingProfileSelectionId;
    private bool _isProfileListDragging;
    private bool _isMouseListTracking;

    /// <summary>
    /// Creates the manage profiles modal view.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="viewModel">The view model bound to this view.</param>
    public ManageProfilesModalView([NotNull] ILoggerFactory loggerFactory, [NotNull] ManageProfilesModalViewModel viewModel)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
    }

    /// <summary>
    /// Rebuilds the rendered manage-profiles list items from the supplied view models.
    /// </summary>
    /// <param name="profileItems">The profile items to render.</param>
    public void SetProfileItems(IEnumerable<ManageProfilesProfileItemViewModel> profileItems)
    {
        ViewModel.SetProfileItems(profileItems);
        RefreshProfileItems();
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root;
        VisualElement dismissTarget = host?.parent ?? host;
        _closeButton = root.Q(CloseButtonName, EmulatorView.ButtonUssClassName);
        _newProfileButton = root.Q<Button>(NewProfileButtonName);
        _profilesScrollView = root.Q<ScrollView>(className: ProfileListClassName);

        _stopPropagationCallback = evt => evt.StopPropagation();
        _root.RegisterCallback(_stopPropagationCallback);

        RefreshProfileItems();
        RegisterProfileListInteractions();

        RegisterViewAction<PointerDownEvent>(dismissTarget, new CloseManageProfilesViewAction());
        RegisterViewAction<ClickEvent>(_closeButton, new CloseManageProfilesViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction(_newProfileButton, () => new OpenEditProfileViewAction
        {
            Mode = EditProfileMode.Create
        });
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        UnregisterProfileListInteractions();
        ResetProfileListInteractionState();

        if(_root != null && _stopPropagationCallback != null)
        {
            _root.UnregisterCallback(_stopPropagationCallback);
        }

        _closeButton = null;
        _newProfileButton = null;
        _profilesScrollView = null;
        _stopPropagationCallback = null;
        _root = null;
    }

    /// <inheritdoc />
    protected override void ShowView()
    {
        _root.AddToClassList(OpenModifierName);
    }

    /// <inheritdoc />
    protected override void HideView()
    {
        _root.RemoveFromClassList(OpenModifierName);
    }

    private void RefreshProfileItems()
    {
        if(_profilesScrollView == null)
        {
            return;
        }

        _profilesScrollView.contentContainer.Clear();

        if(ViewModel.ProfileItemTemplate == null)
        {
            return;
        }

        foreach(ManageProfilesProfileItemViewModel profileItem in ViewModel.ProfileItems)
        {
            TemplateContainer itemInstance = ViewModel.ProfileItemTemplate.CloneTree();
            VisualElement itemRoot = itemInstance.Q(className: ProfileItemClassName) ?? itemInstance;
            itemRoot.AddStyleSheets(itemInstance);
            itemRoot.dataSource = profileItem;
            itemRoot.userData = profileItem.PlayerId;

            Label displayNameLabel = itemRoot.Q<Label>(className: ProfileItemDisplayNameClassName);
            if(displayNameLabel != null)
            {
                displayNameLabel.text = profileItem.DisplayName ?? string.Empty;
            }

            _profilesScrollView.contentContainer.Add(itemRoot);
        }
    }

    private void RegisterProfileListInteractions()
    {
        if(_profilesScrollView?.contentViewport == null)
        {
            return;
        }

        UnregisterProfileListInteractions();

        VisualElement viewport = _profilesScrollView.contentViewport;

        EventCallback<PointerDownEvent> pointerDown = OnProfileListPointerDown;
        EventCallback<PointerMoveEvent> pointerMove = OnProfileListPointerMove;
        EventCallback<PointerUpEvent> pointerUp = OnProfileListPointerUp;
        EventCallback<PointerCancelEvent> pointerCancel = OnProfileListPointerCancel;
        EventCallback<PointerCaptureOutEvent> pointerCaptureOut = OnProfileListPointerCaptureOut;
        EventCallback<MouseDownEvent> mouseDown = OnProfileListMouseDown;
        EventCallback<MouseMoveEvent> mouseMove = OnProfileListMouseMove;
        EventCallback<MouseUpEvent> mouseUp = OnProfileListMouseUp;
        EventCallback<MouseCaptureOutEvent> mouseCaptureOut = OnProfileListMouseCaptureOut;

        viewport.RegisterCallback(pointerDown, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerMove, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerUp, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerCancel, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerCaptureOut, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseDown, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseMove, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseUp, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseCaptureOut, TrickleDown.TrickleDown);

        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerDown, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerMove, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerUp, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerCancel, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerCaptureOut, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseDown, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseMove, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseUp, TrickleDown.TrickleDown));
        _profileListInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseCaptureOut, TrickleDown.TrickleDown));
    }

    private void UnregisterProfileListInteractions()
    {
        foreach(Action unregister in _profileListInteractionUnregisters)
        {
            unregister();
        }

        _profileListInteractionUnregisters.Clear();
    }

    private void OnProfileListPointerDown(PointerDownEvent evt)
    {
        if(evt.button != 0 || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        BeginProfileListTracking(evt.pointerId, evt.position, TryGetProfileIdFromEventTarget(evt.target));
        _profilesScrollView.contentViewport.CapturePointer(evt.pointerId);
    }

    private void OnProfileListPointerMove(PointerMoveEvent evt)
    {
        if(evt.pointerId != _profileListPointerId || _profilesScrollView == null)
        {
            return;
        }

        HandleProfileListDrag(evt.position, evt.StopPropagation);
    }

    private void OnProfileListPointerUp(PointerUpEvent evt)
    {
        if(evt.pointerId != _profileListPointerId || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _profilesScrollView.contentViewport.ReleasePointer(evt.pointerId);
        PublishPendingProfileSelection();
        ResetProfileListInteractionState();
    }

    private void OnProfileListPointerCancel(PointerCancelEvent evt)
    {
        if(evt.pointerId != _profileListPointerId || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _profilesScrollView.contentViewport.ReleasePointer(evt.pointerId);
        ResetProfileListInteractionState();
    }

    private void OnProfileListPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        ResetProfileListInteractionState();
    }

    private void OnProfileListMouseDown(MouseDownEvent evt)
    {
        if(evt.button != 0 || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _isMouseListTracking = true;
        BeginProfileListTracking(pointerId: 0, evt.mousePosition, TryGetProfileIdFromEventTarget(evt.target));
        _profilesScrollView.contentViewport.CaptureMouse();
    }

    private void OnProfileListMouseMove(MouseMoveEvent evt)
    {
        if(_isMouseListTracking == false || _profilesScrollView == null)
        {
            return;
        }

        HandleProfileListDrag(evt.mousePosition, evt.StopPropagation);
    }

    private void OnProfileListMouseUp(MouseUpEvent evt)
    {
        if(_isMouseListTracking == false || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _profilesScrollView.contentViewport.ReleaseMouse();
        PublishPendingProfileSelection();
        ResetProfileListInteractionState();
    }

    private void OnProfileListMouseCaptureOut(MouseCaptureOutEvent evt)
    {
        ResetProfileListInteractionState();
    }

    private void BeginProfileListTracking(int pointerId, Vector2 pointerPosition, string pendingProfileSelectionId)
    {
        _profileListPointerId = pointerId;
        _profileListStartPosition = pointerPosition;
        _profileListStartOffsetY = _profilesScrollView.scrollOffset.y;
        _pendingProfileSelectionId = pendingProfileSelectionId;
        _isProfileListDragging = false;
    }

    private void HandleProfileListDrag(Vector2 pointerPosition, Action stopPropagation)
    {
        Vector2 dragDelta = pointerPosition - _profileListStartPosition;
        if(_isProfileListDragging == false)
        {
            if(Mathf.Abs(dragDelta.y) < ProfileListDragThreshold || Mathf.Abs(dragDelta.y) <= Mathf.Abs(dragDelta.x))
            {
                return;
            }

            _isProfileListDragging = true;
        }

        Vector2 scrollOffset = _profilesScrollView.scrollOffset;
        scrollOffset.y = Mathf.Max(0f, _profileListStartOffsetY - dragDelta.y);
        _profilesScrollView.scrollOffset = scrollOffset;
        stopPropagation?.Invoke();
    }

    private void PublishPendingProfileSelection()
    {
        if(_isProfileListDragging || string.IsNullOrWhiteSpace(_pendingProfileSelectionId))
        {
            return;
        }

        PublishViewAction(new OpenProfileSettingsViewAction
        {
            SelectedProfileId = _pendingProfileSelectionId
        });
    }

    private void ResetProfileListInteractionState()
    {
        _profileListPointerId = -1;
        _profileListStartPosition = Vector2.zero;
        _profileListStartOffsetY = 0f;
        _pendingProfileSelectionId = null;
        _isProfileListDragging = false;
        _isMouseListTracking = false;
    }

    private string TryGetProfileIdFromEventTarget(object eventTarget)
    {
        VisualElement element = eventTarget as VisualElement;
        while(element != null)
        {
            if(element.ClassListContains(ProfileItemClassName) && element.userData is string playerId && string.IsNullOrWhiteSpace(playerId) == false)
            {
                return playerId;
            }

            element = element.parent;
        }

        return null;
    }
}
}
