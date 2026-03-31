using System;
using System.Collections.Generic;

using BE.Emulator.Actions;
using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;
using Rahmen.UIToolkit.Extensions;

using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator.Screens.ProfileSwitcher
{
/// <summary>
/// View wrapper for the profile switcher overlay.
/// </summary>
internal sealed class ProfileSwitcherView : BaseView<ProfileSwitcherViewModel>
{
    /// <summary>
    /// The USS root class applied to the profile switcher.
    /// </summary>
    public const string UssClassName = "profile-switcher";
    /// <summary>
    /// The USS class applied to the profile-card scroll view.
    /// </summary>
    public const string ProfileScrollViewClassName = UssClassName + "__profile-cards";
    /// <summary>
    /// The element name used for the close button.
    /// </summary>
    public const string CloseButtonName = "close-switch-profile";
    /// <summary>
    /// The element name used for the manage-profiles button.
    /// </summary>
    public const string ManageProfilesButtonName = "manage-profiles";
    /// <summary>
    /// The modifier applied when the profile switcher is visible.
    /// </summary>
    public const string OpenModifierName = "--open";
    /// <summary>
    /// The USS class applied to individual profile cards.
    /// </summary>
    public const string ProfileCardClassName = "profile-card";
    public const string ProfileCardDisplayNameClassName = "profile-card__display-name";
    /// <summary>
    /// The modifier applied to the currently active profile card.
    /// </summary>
    public const string ActiveProfileCardModifierName = "--active";
    private const float ProfileScrollDragThreshold = 8f;

    /// <inheritdoc />
    protected override string StyleClassName => UssClassName;

    private VisualElement _root;
    private VisualElement _closeButton;
    private VisualElement _manageProfilesButton;
    private ScrollView _profilesScrollView;
    private readonly List<Action> _profileCardActionUnregisters = new();
    private readonly List<Action> _profileScrollInteractionUnregisters = new();
    private int _profileScrollPointerId = -1;
    private Vector2 _profileScrollStartPosition;
    private float _profileScrollStartOffsetX;
    private string _pendingProfileSelectionId;
    private bool _isProfileScrollDragging;
    private bool _isMouseScrollTracking;

    /// <summary>
    /// Creates the profile switcher view.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="viewModel">The view model bound to this view.</param>
    public ProfileSwitcherView([NotNull] ILoggerFactory loggerFactory, [NotNull] ProfileSwitcherViewModel viewModel)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
    }

    /// <summary>
    /// Rebuilds the rendered profile cards from the supplied view models.
    /// </summary>
    /// <param name="profileCards">The profile cards to render.</param>
    public void SetProfileCards(IEnumerable<ProfileCardViewModel> profileCards)
    {
        ViewModel.SetProfileCards(profileCards);
        RefreshProfileCards();
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root;
        _closeButton = root.Q(CloseButtonName, EmulatorView.ButtonUssClassName);
        _manageProfilesButton = root.Q(ManageProfilesButtonName, EmulatorView.ButtonUssClassName);
        _profilesScrollView = root.Q<ScrollView>(className: ProfileScrollViewClassName);

        RefreshProfileCards();
        RegisterProfileScrollInteractions();

        RegisterViewAction<ClickEvent>(_closeButton, new CloseProfileSwitcherViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(_manageProfilesButton, new ManageProfilesViewAction(), TrickleDown.TrickleDown);
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        UnregisterProfileCardActions();
        UnregisterProfileScrollInteractions();
        ResetProfileScrollInteractionState();
        _closeButton = null;
        _manageProfilesButton = null;
        _profilesScrollView = null;
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

    private void RefreshProfileCards()
    {
        if(_profilesScrollView == null)
        {
            return;
        }

        UnregisterProfileCardActions();

        _profilesScrollView.contentContainer.Clear();
        if(ViewModel.ProfileCardTemplate == null)
        {
            return;
        }

        foreach(ProfileCardViewModel profileCard in ViewModel.ProfileCards)
        {
            TemplateContainer cardInstance = ViewModel.ProfileCardTemplate.CloneTree();
            VisualElement cardRoot = cardInstance.Q(className: ProfileCardClassName) ?? cardInstance;
            cardRoot.AddStyleSheets(cardInstance);
            cardRoot.dataSource = profileCard;
            cardRoot.userData = profileCard.PlayerId;
            cardRoot.EnableInClassList(ActiveProfileCardModifierName, profileCard.IsActive);

            Label displayNameLabel = cardRoot.Q<Label>(className: ProfileCardDisplayNameClassName);
            if(displayNameLabel != null)
            {
                displayNameLabel.text = profileCard.DisplayName ?? string.Empty;
            }

            _profilesScrollView.contentContainer.Add(cardRoot);
        }
    }

    private void UnregisterProfileCardActions()
    {
        foreach(Action unregisterViewAction in _profileCardActionUnregisters)
        {
            UnregisterViewAction(unregisterViewAction);
        }

        _profileCardActionUnregisters.Clear();
    }

    private void RegisterProfileScrollInteractions()
    {
        if(_profilesScrollView?.contentViewport == null)
        {
            return;
        }

        UnregisterProfileScrollInteractions();

        VisualElement viewport = _profilesScrollView.contentViewport;

        EventCallback<PointerDownEvent> pointerDown = OnProfileScrollPointerDown;
        EventCallback<PointerMoveEvent> pointerMove = OnProfileScrollPointerMove;
        EventCallback<PointerUpEvent> pointerUp = OnProfileScrollPointerUp;
        EventCallback<PointerCancelEvent> pointerCancel = OnProfileScrollPointerCancel;
        EventCallback<PointerCaptureOutEvent> pointerCaptureOut = OnProfileScrollPointerCaptureOut;
        EventCallback<MouseDownEvent> mouseDown = OnProfileScrollMouseDown;
        EventCallback<MouseMoveEvent> mouseMove = OnProfileScrollMouseMove;
        EventCallback<MouseUpEvent> mouseUp = OnProfileScrollMouseUp;
        EventCallback<MouseCaptureOutEvent> mouseCaptureOut = OnProfileScrollMouseCaptureOut;

        viewport.RegisterCallback(pointerDown, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerMove, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerUp, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerCancel, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerCaptureOut, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseDown, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseMove, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseUp, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseCaptureOut, TrickleDown.TrickleDown);

        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerDown, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerMove, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerUp, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerCancel, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerCaptureOut, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseDown, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseMove, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseUp, TrickleDown.TrickleDown));
        _profileScrollInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseCaptureOut, TrickleDown.TrickleDown));
    }

    private void UnregisterProfileScrollInteractions()
    {
        foreach(Action unregisterInteraction in _profileScrollInteractionUnregisters)
        {
            unregisterInteraction();
        }

        _profileScrollInteractionUnregisters.Clear();
    }

    private void OnProfileScrollPointerDown(PointerDownEvent evt)
    {
        if(evt.button != 0 || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        BeginProfileScrollTracking(evt.pointerId, evt.position, TryGetProfileIdFromEventTarget(evt.target));
        _profilesScrollView.contentViewport.CapturePointer(evt.pointerId);
    }

    private void OnProfileScrollPointerMove(PointerMoveEvent evt)
    {
        if(evt.pointerId != _profileScrollPointerId || _profilesScrollView == null)
        {
            return;
        }

        HandleProfileScrollDrag(evt.position, stopPropagation: evt.StopPropagation);
    }

    private void OnProfileScrollPointerUp(PointerUpEvent evt)
    {
        if(evt.pointerId != _profileScrollPointerId || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _profilesScrollView.contentViewport.ReleasePointer(evt.pointerId);
        PublishPendingProfileSelection();
        ResetProfileScrollInteractionState();
    }

    private void OnProfileScrollPointerCancel(PointerCancelEvent evt)
    {
        if(evt.pointerId != _profileScrollPointerId || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _profilesScrollView.contentViewport.ReleasePointer(evt.pointerId);
        ResetProfileScrollInteractionState();
    }

    private void OnProfileScrollPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        ResetProfileScrollInteractionState();
    }

    private void OnProfileScrollMouseDown(MouseDownEvent evt)
    {
        if(evt.button != 0 || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _isMouseScrollTracking = true;
        BeginProfileScrollTracking(pointerId: 0, evt.mousePosition, TryGetProfileIdFromEventTarget(evt.target));
        _profilesScrollView.contentViewport.CaptureMouse();
    }

    private void OnProfileScrollMouseMove(MouseMoveEvent evt)
    {
        if(_isMouseScrollTracking == false || _profilesScrollView == null)
        {
            return;
        }

        HandleProfileScrollDrag(evt.mousePosition, stopPropagation: evt.StopPropagation);
    }

    private void OnProfileScrollMouseUp(MouseUpEvent evt)
    {
        if(_isMouseScrollTracking == false || _profilesScrollView?.contentViewport == null)
        {
            return;
        }

        _profilesScrollView.contentViewport.ReleaseMouse();
        ResetProfileScrollInteractionState();
    }

    private void OnProfileScrollMouseCaptureOut(MouseCaptureOutEvent evt)
    {
        ResetProfileScrollInteractionState();
    }

    private void BeginProfileScrollTracking(int pointerId, Vector2 pointerPosition, string pendingProfileSelectionId)
    {
        _profileScrollPointerId = pointerId;
        _profileScrollStartPosition = pointerPosition;
        _profileScrollStartOffsetX = _profilesScrollView.scrollOffset.x;
        _pendingProfileSelectionId = pendingProfileSelectionId;
        _isProfileScrollDragging = false;
    }

    private void HandleProfileScrollDrag(Vector2 pointerPosition, Action stopPropagation)
    {
        Vector2 dragDelta = pointerPosition - _profileScrollStartPosition;
        if(_isProfileScrollDragging == false)
        {
            if(Mathf.Abs(dragDelta.x) < ProfileScrollDragThreshold || Mathf.Abs(dragDelta.x) <= Mathf.Abs(dragDelta.y))
            {
                return;
            }

            _isProfileScrollDragging = true;
        }

        Vector2 scrollOffset = _profilesScrollView.scrollOffset;
        scrollOffset.x = Mathf.Max(0f, _profileScrollStartOffsetX - dragDelta.x);
        _profilesScrollView.scrollOffset = scrollOffset;
        stopPropagation?.Invoke();
    }

    private void ResetProfileScrollInteractionState()
    {
        _profileScrollPointerId = -1;
        _profileScrollStartPosition = Vector2.zero;
        _profileScrollStartOffsetX = 0f;
        _pendingProfileSelectionId = null;
        _isProfileScrollDragging = false;
        _isMouseScrollTracking = false;
    }

    private void PublishPendingProfileSelection()
    {
        if(_isProfileScrollDragging || string.IsNullOrWhiteSpace(_pendingProfileSelectionId))
        {
            return;
        }

        PublishViewAction(new ProfileSelectedViewAction
        {
            SelectedProfileId = _pendingProfileSelectionId
        });
    }

    private string TryGetProfileIdFromEventTarget(object eventTarget)
    {
        VisualElement element = eventTarget as VisualElement;
        while(element != null)
        {
            if(element.ClassListContains(ProfileCardClassName) && element.userData is string playerId && string.IsNullOrWhiteSpace(playerId) == false)
            {
                return playerId;
            }

            element = element.parent;
        }

        return null;
    }
}
}
