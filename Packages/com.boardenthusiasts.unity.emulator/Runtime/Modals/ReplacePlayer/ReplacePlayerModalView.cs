using System;
using System.Collections.Generic;

using BE.Emulator.Actions;
using BE.Emulator.Framework;
using BE.Emulator.Modals.AddPlayer;

using JetBrains.Annotations;

using Rahmen.Logging;
using Rahmen.UIToolkit.Extensions;

using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator.Modals.ReplacePlayer
{
/// <summary>
/// View wrapper for the replace-player modal.
/// </summary>
internal sealed class ReplacePlayerModalView : BaseView<ReplacePlayerModalViewModel>
{
    /// <summary>
    /// The USS root class applied to the modal.
    /// </summary>
    public const string RootUssClassName = "replace-player-modal-root";

    /// <summary>
    /// The USS class applied to the modal content.
    /// </summary>
    public const string PanelUssClassName = "replace-player-modal";

    /// <summary>
    /// The element name used for the backdrop dismiss target.
    /// </summary>
    public const string BackdropName = "dismiss-replace-player";

    /// <summary>
    /// The element name used for the back button.
    /// </summary>
    public const string BackButtonName = "close-replace-player";

    /// <summary>
    /// The element name used for the remove-player button.
    /// </summary>
    public const string RemovePlayerButtonName = "remove-player";

    /// <summary>
    /// The element name used for the current-player avatar.
    /// </summary>
    public const string CurrentPlayerAvatarName = "replace-player-current-avatar";

    /// <summary>
    /// The element name used for the current-player display name label.
    /// </summary>
    public const string CurrentPlayerDisplayNameName = "replace-player-current-display-name";

    /// <summary>
    /// The USS class applied to the replacement-card scroll view.
    /// </summary>
    public const string CardsScrollViewClassName = PanelUssClassName + "__cards";

    /// <summary>
    /// The modifier applied when the modal is visible.
    /// </summary>
    public const string OpenModifierName = "--open";

    private const float CardsDragThreshold = 8f;

    /// <inheritdoc />
    protected override string StyleClassName => RootUssClassName;

    private VisualElement _root;
    private VisualElement _backdrop;
    private VisualElement _backButton;
    private Button _removePlayerButton;
    private VisualElement _currentPlayerAvatar;
    private Label _currentPlayerDisplayName;
    private ScrollView _cardsScrollView;
    private EventCallback<PointerDownEvent> _stopPropagationCallback;
    private readonly List<Action> _cardInteractionUnregisters = new();
    private int _cardsPointerId = -1;
    private Vector2 _cardsStartPosition;
    private float _cardsStartOffsetY;
    private AddPlayerCardViewModel _pendingCardSelection;
    private bool _isCardsDragging;
    private bool _isMouseCardsTracking;

    /// <summary>
    /// Creates the replace-player modal view.
    /// </summary>
    public ReplacePlayerModalView([NotNull] ILoggerFactory loggerFactory, [NotNull] ReplacePlayerModalViewModel viewModel)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
    }

    /// <summary>
    /// Rebuilds the modal using the supplied target player and replacement cards.
    /// </summary>
    public void SetState(int targetSessionId, AddPlayerCardViewModel currentPlayer, bool canRemoveCurrentPlayer, IEnumerable<AddPlayerCardViewModel> replacementCards)
    {
        ViewModel.SetState(targetSessionId, currentPlayer, canRemoveCurrentPlayer, replacementCards);
        RefreshCurrentPlayer();
        RefreshCards();
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root;
        _backdrop = root.Q(BackdropName);
        _backButton = root.Q(BackButtonName, EmulatorView.ButtonUssClassName);
        _removePlayerButton = root.Q<Button>(RemovePlayerButtonName);
        _currentPlayerAvatar = root.Q(CurrentPlayerAvatarName);
        _currentPlayerDisplayName = root.Q<Label>(CurrentPlayerDisplayNameName);
        _cardsScrollView = root.Q<ScrollView>(className: CardsScrollViewClassName);

        _stopPropagationCallback = evt => evt.StopPropagation();
        _root.RegisterCallback(_stopPropagationCallback);

        RefreshCurrentPlayer();
        RefreshCards();
        RegisterCardInteractions();

        RegisterViewAction<PointerDownEvent>(_backdrop, new CloseReplacePlayerSelectorViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<PointerDownEvent>(_backButton, new CloseReplacePlayerSelectorViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(_backButton, new CloseReplacePlayerSelectorViewAction(), TrickleDown.TrickleDown);

        if(_removePlayerButton != null)
        {
            _removePlayerButton.clicked += OnRemovePlayerClicked;
        }
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        UnregisterCardInteractions();
        ResetCardInteractionState();

        if(_root != null && _stopPropagationCallback != null)
        {
            _root.UnregisterCallback(_stopPropagationCallback);
        }

        if(_removePlayerButton != null)
        {
            _removePlayerButton.clicked -= OnRemovePlayerClicked;
        }

        _backdrop = null;
        _backButton = null;
        _removePlayerButton = null;
        _currentPlayerAvatar = null;
        _currentPlayerDisplayName = null;
        _cardsScrollView = null;
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

    private void RefreshCurrentPlayer()
    {
        if(_currentPlayerAvatar == null || _currentPlayerDisplayName == null)
        {
            return;
        }

        AddPlayerCardViewModel currentPlayer = ViewModel.CurrentPlayer;
        _currentPlayerDisplayName.text = currentPlayer?.DisplayName ?? string.Empty;
        _currentPlayerAvatar.style.backgroundColor = currentPlayer?.AvatarBackgroundColor ?? new StyleColor(Color.clear);
        _currentPlayerAvatar.style.backgroundImage = currentPlayer?.AvatarImage ?? new StyleBackground();
        if(_removePlayerButton != null)
        {
            _removePlayerButton.style.display = ViewModel.CanRemoveCurrentPlayer ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void RefreshCards()
    {
        if(_cardsScrollView == null)
        {
            return;
        }

        _cardsScrollView.contentContainer.Clear();
        if(ViewModel.CardTemplate == null)
        {
            return;
        }

        foreach(AddPlayerCardViewModel card in ViewModel.ReplacementCards)
        {
            TemplateContainer cardInstance = ViewModel.CardTemplate.CloneTree();
            VisualElement cardRoot = cardInstance.Q(className: AddPlayerModalView.CardClassName) ?? cardInstance;
            cardRoot.AddStyleSheets(cardInstance);
            cardRoot.dataSource = card;
            cardRoot.userData = card;
            _cardsScrollView.contentContainer.Add(cardRoot);
        }
    }

    private void RegisterCardInteractions()
    {
        if(_cardsScrollView?.contentViewport == null)
        {
            return;
        }

        UnregisterCardInteractions();

        VisualElement viewport = _cardsScrollView.contentViewport;

        EventCallback<PointerDownEvent> pointerDown = OnCardsPointerDown;
        EventCallback<PointerMoveEvent> pointerMove = OnCardsPointerMove;
        EventCallback<PointerUpEvent> pointerUp = OnCardsPointerUp;
        EventCallback<PointerCancelEvent> pointerCancel = OnCardsPointerCancel;
        EventCallback<PointerCaptureOutEvent> pointerCaptureOut = OnCardsPointerCaptureOut;
        EventCallback<MouseDownEvent> mouseDown = OnCardsMouseDown;
        EventCallback<MouseMoveEvent> mouseMove = OnCardsMouseMove;
        EventCallback<MouseUpEvent> mouseUp = OnCardsMouseUp;
        EventCallback<MouseCaptureOutEvent> mouseCaptureOut = OnCardsMouseCaptureOut;

        viewport.RegisterCallback(pointerDown, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerMove, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerUp, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerCancel, TrickleDown.TrickleDown);
        viewport.RegisterCallback(pointerCaptureOut, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseDown, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseMove, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseUp, TrickleDown.TrickleDown);
        viewport.RegisterCallback(mouseCaptureOut, TrickleDown.TrickleDown);

        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerDown, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerMove, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerUp, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerCancel, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(pointerCaptureOut, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseDown, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseMove, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseUp, TrickleDown.TrickleDown));
        _cardInteractionUnregisters.Add(() => viewport.UnregisterCallback(mouseCaptureOut, TrickleDown.TrickleDown));
    }

    private void UnregisterCardInteractions()
    {
        foreach(Action unregister in _cardInteractionUnregisters)
        {
            unregister();
        }

        _cardInteractionUnregisters.Clear();
    }

    private void OnCardsPointerDown(PointerDownEvent evt)
    {
        if(evt.button != 0 || _cardsScrollView?.contentViewport == null)
        {
            return;
        }

        BeginCardsTracking(evt.pointerId, evt.position, TryGetCardFromEventTarget(evt.target));
        _cardsScrollView.contentViewport.CapturePointer(evt.pointerId);
    }

    private void OnCardsPointerMove(PointerMoveEvent evt)
    {
        if(evt.pointerId != _cardsPointerId || _cardsScrollView == null)
        {
            return;
        }

        HandleCardsDrag(evt.position, evt.StopPropagation);
    }

    private void OnCardsPointerUp(PointerUpEvent evt)
    {
        if(evt.pointerId != _cardsPointerId || _cardsScrollView?.contentViewport == null)
        {
            return;
        }

        _cardsScrollView.contentViewport.ReleasePointer(evt.pointerId);
        PublishPendingCardSelection();
        ResetCardInteractionState();
    }

    private void OnCardsPointerCancel(PointerCancelEvent evt)
    {
        if(evt.pointerId != _cardsPointerId || _cardsScrollView?.contentViewport == null)
        {
            return;
        }

        _cardsScrollView.contentViewport.ReleasePointer(evt.pointerId);
        ResetCardInteractionState();
    }

    private void OnCardsPointerCaptureOut(PointerCaptureOutEvent evt)
    {
        ResetCardInteractionState();
    }

    private void OnCardsMouseDown(MouseDownEvent evt)
    {
        if(evt.button != 0 || _cardsScrollView?.contentViewport == null)
        {
            return;
        }

        _isMouseCardsTracking = true;
        BeginCardsTracking(pointerId: 0, evt.mousePosition, TryGetCardFromEventTarget(evt.target));
        _cardsScrollView.contentViewport.CaptureMouse();
    }

    private void OnCardsMouseMove(MouseMoveEvent evt)
    {
        if(_isMouseCardsTracking == false || _cardsScrollView == null)
        {
            return;
        }

        HandleCardsDrag(evt.mousePosition, evt.StopPropagation);
    }

    private void OnCardsMouseUp(MouseUpEvent evt)
    {
        if(_isMouseCardsTracking == false || _cardsScrollView?.contentViewport == null)
        {
            return;
        }

        _cardsScrollView.contentViewport.ReleaseMouse();
        PublishPendingCardSelection();
        ResetCardInteractionState();
    }

    private void OnCardsMouseCaptureOut(MouseCaptureOutEvent evt)
    {
        ResetCardInteractionState();
    }

    private void BeginCardsTracking(int pointerId, Vector2 pointerPosition, AddPlayerCardViewModel pendingCardSelection)
    {
        _cardsPointerId = pointerId;
        _cardsStartPosition = pointerPosition;
        _cardsStartOffsetY = _cardsScrollView.scrollOffset.y;
        _pendingCardSelection = pendingCardSelection;
        _isCardsDragging = false;
    }

    private void HandleCardsDrag(Vector2 pointerPosition, Action stopPropagation)
    {
        Vector2 dragDelta = pointerPosition - _cardsStartPosition;
        if(_isCardsDragging == false)
        {
            if(Mathf.Abs(dragDelta.y) < CardsDragThreshold || Mathf.Abs(dragDelta.y) <= Mathf.Abs(dragDelta.x))
            {
                return;
            }

            _isCardsDragging = true;
        }

        Vector2 scrollOffset = _cardsScrollView.scrollOffset;
        scrollOffset.y = Mathf.Max(0f, _cardsStartOffsetY - dragDelta.y);
        _cardsScrollView.scrollOffset = scrollOffset;
        stopPropagation?.Invoke();
    }

    private void PublishPendingCardSelection()
    {
        if(_isCardsDragging || _pendingCardSelection == null || ViewModel.TargetSessionId <= 0)
        {
            return;
        }

        PublishViewAction(new PlayerReplacedViewAction
        {
            TargetSessionId = ViewModel.TargetSessionId,
            SelectedProfileId = _pendingCardSelection.PlayerId
        });
    }

    private void OnRemovePlayerClicked()
    {
        if(ViewModel.TargetSessionId <= 0)
        {
            return;
        }

        PublishViewAction(new PlayerRemovedViewAction
        {
            TargetSessionId = ViewModel.TargetSessionId
        });
    }

    private void ResetCardInteractionState()
    {
        _cardsPointerId = -1;
        _cardsStartPosition = Vector2.zero;
        _cardsStartOffsetY = 0f;
        _pendingCardSelection = null;
        _isCardsDragging = false;
        _isMouseCardsTracking = false;
    }

    private static AddPlayerCardViewModel TryGetCardFromEventTarget(object eventTarget)
    {
        VisualElement element = eventTarget as VisualElement;
        while(element != null)
        {
            if(element.ClassListContains(AddPlayerModalView.CardClassName) && element.userData is AddPlayerCardViewModel cardViewModel)
            {
                return cardViewModel;
            }

            element = element.parent;
        }

        return null;
    }
}
}
