using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

namespace BE.Emulator.Framework
{
/// <summary>
/// Base <see cref="IView{T}"/> handling the majority of the general common functionality.
/// </summary>
/// <typeparam name="TViewModel">The type of view model used by the view.</typeparam>
public abstract class BaseView<TViewModel> : IView<TViewModel> where TViewModel : IViewModel
{
    /// <inheritdoc />
    public event EventHandler ReadyForAttachment;
    /// <inheritdoc />
    public event EventHandler<ViewActionEventArgs> ViewActionTriggered;

    /// <inheritdoc />
    public bool IsReady { get; private set; }
    /// <inheritdoc />
    public bool IsAttached => _root != null;
    /// <inheritdoc />
    public bool IsVisible { get; private set; }

    TViewModel IView<TViewModel>.ViewModel => _viewModel;
    protected TViewModel ViewModel => _viewModel;

    /// <summary>
    /// The name of the style class that will be applied to the root <see cref="VisualElement"/> of this <see cref="IDisplayable"/>.
    /// </summary>
    protected abstract string StyleClassName { get; }

    private readonly IRahmenLogger _logger;
    private readonly TViewModel _viewModel;
    private readonly VisualTreeAsset _sourceTemplate;
    private readonly List<Action> _unbindViewActions = new();
    private VisualElement _host;
    private VisualElement _root;
    private bool _hasAppliedVisibility;

    /// <summary>
    /// Creates the base view.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="sourceTemplate">The source template used to clone the view tree.</param>
    /// <param name="viewModel">The view model bound to this view.</param>
    protected BaseView([NotNull] ILoggerFactory loggerFactory, [NotNull] VisualTreeAsset sourceTemplate, [NotNull] TViewModel viewModel)
    {
        _logger = loggerFactory?.Get<LogChannels.BoardEmulation>(this) ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger.Trace()?.Log("");
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _sourceTemplate = sourceTemplate ?? throw new ArgumentNullException(nameof(sourceTemplate));
    }

    /// <inheritdoc />
    public virtual void Initialize()
    {
        _logger.Trace()?.Log("");

        IsReady = true;
        ReadyForAttachment?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        _logger.Trace()?.Log("");

        Detach();
        IsReady = false;
    }

    /// <inheritdoc />
    public void Attach(VisualElement host, bool clearHost = true, bool hideAfterAttach = true)
    {
        _logger.Trace()?.Log("");

        if(host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        if(IsAttached && ReferenceEquals(_host, host))
        {
            _logger.Warning()?.Log("Attempted to attach to the same host element while already attached. Ignoring.");
            return;
        }

        Detach();

        _host = host;
        if(clearHost)
        {
            _host.Clear();
        }

        _sourceTemplate.CloneTree(_host);

        _root = _host.Q(className: StyleClassName) ?? _host.Children().ElementAtOrDefault(0);

        if(_root == null)
        {
            _logger.Error()?.Log($"Cloning the source template <{_sourceTemplate.name}> failed to produce a root element either with style class name <{StyleClassName}> or first child. The view will not be functional.");
            return;
        }

        Bind(_host, _root);

        if(hideAfterAttach)
        {
            Hide();
        }
    }

    /// <inheritdoc />
    public void Show()
    {
        _logger.Trace()?.Log("");

        if(IsVisible && _hasAppliedVisibility)
        {
            _logger.Warning()?.Log("Attempted to show already visible view. Ignoring.");
            return;
        }

        _logger.Debug()?.Log("Showing view.");
        ShowView();
        IsVisible = true;
        _hasAppliedVisibility = true;
    }

    /// <inheritdoc />
    public void Hide()
    {
        _logger.Trace()?.Log("");

        if(IsVisible == false && _hasAppliedVisibility)
        {
            _logger.Warning()?.Log("Attempted to hide already hidden view. Ignoring.");
            return;
        }

        _logger.Debug()?.Log("Hiding view.");
        HideView();
        IsVisible = false;
        _hasAppliedVisibility = true;
    }

    /// <inheritdoc />
    public void Detach()
    {
        _logger.Trace()?.Log("");

        if(IsAttached == false)
        {
            _logger.Info()?.Log("Attempted to detach while already detached. Ignoring.");
            return;
        }

        Unbind(_host, _root);
        UnbindViewActions();

        _root.RemoveFromHierarchy();
        _root = null;
        _host = null;
        _hasAppliedVisibility = false;
    }

    /// <summary>
    /// Invoked immediately after successful attachment to a host. Implementors should use this method to bind view
    /// elements to the view model and set up any necessary event handlers.
    /// </summary>
    /// <param name="host">The host <see cref="VisualElement"/> that contains this view, if there is one.</param>
    /// <param name="root">The root <see cref="VisualElement"/> for the view.</param>
    protected abstract void Bind(VisualElement host, [NotNull] VisualElement root);

    /// <summary>
    /// Invoked just before detaching from a host. Implementors should use this method to perform any steps required to
    /// properly unbind from view elements.
    /// </summary>
    /// <param name="host">The host <see cref="VisualElement"/> that contains this view, if there is one.</param>
    /// <param name="root">The root element currently bound by the view.</param>
    protected abstract void Unbind(VisualElement host, [NotNull] VisualElement root);

    /// <summary>
    /// Invoked when the view should make itself visible within its host. Implementors should use this method to
    /// perform any necessary actions to make the view visible such as adding/removing class names.
    /// </summary>
    protected abstract void ShowView();

    /// <summary>
    /// Invoked when the view should hide itself within its host. Implementors should use this method to perform any
    /// necessary actions to hide the view such as adding/removing class names.
    /// </summary>
    protected abstract void HideView();

    /// <summary>
    /// Publish the provided <paramref name="action"/> to the view's controller.
    /// </summary>
    /// <param name="action">The action to publish.</param>
    protected void PublishViewAction([NotNull] IViewAction action)
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        ViewActionTriggered?.Invoke(this, new ViewActionEventArgs(action));
    }

    /// <summary>
    /// Register a UI Toolkit callback that publishes the provided <paramref name="action"/>.
    /// </summary>
    /// <typeparam name="TEventType">The UI Toolkit event type to register.</typeparam>
    /// <param name="element">The element that should publish the action.</param>
    /// <param name="action">The action that should be published.</param>
    /// <param name="trickleDown">The trickle-down behavior used for callback registration.</param>
    /// <param name="stopPropagation">Whether the UI Toolkit event propagation should be stopped before publishing the action.</param>
    /// <param name="beforePublish">Optional pre-publish event logic.</param>
    protected Action RegisterViewAction<TEventType>(
        [CanBeNull] VisualElement element,
        [NotNull] IViewAction action,
        TrickleDown trickleDown = TrickleDown.NoTrickleDown,
        bool stopPropagation = true,
        [CanBeNull] Action<TEventType> beforePublish = null)
        where TEventType : EventBase<TEventType>, new()
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return RegisterViewAction(element, _ => action, trickleDown, stopPropagation, beforePublish);
    }

    /// <summary>
    /// Register a UI Toolkit callback that publishes a dynamically created action.
    /// </summary>
    /// <typeparam name="TEventType">The UI Toolkit event type to register.</typeparam>
    /// <param name="element">The element that should publish the action.</param>
    /// <param name="actionFactory">The factory used to create an action from the callback event.</param>
    /// <param name="trickleDown">The trickle-down behavior used for callback registration.</param>
    /// <param name="stopPropagation">Whether the UI Toolkit event propagation should be stopped before publishing the action.</param>
    /// <param name="beforePublish">Optional pre-publish event logic.</param>
    protected Action RegisterViewAction<TEventType>(
        [CanBeNull] VisualElement element,
        [NotNull] Func<TEventType, IViewAction> actionFactory,
        TrickleDown trickleDown = TrickleDown.NoTrickleDown,
        bool stopPropagation = true,
        [CanBeNull] Action<TEventType> beforePublish = null)
        where TEventType : EventBase<TEventType>, new()
    {
        if(element == null)
        {
            return null;
        }

        if(actionFactory == null)
        {
            throw new ArgumentNullException(nameof(actionFactory));
        }

        EventCallback<TEventType> callback = evt =>
        {
            beforePublish?.Invoke(evt);

            if(stopPropagation)
            {
                evt.StopPropagation();
            }

            PublishViewAction(actionFactory(evt));
        };

        element.RegisterCallback(callback, trickleDown);

        Action unregister = () => element.UnregisterCallback(callback, trickleDown);
        _unbindViewActions.Add(unregister);
        return () =>
        {
            if(_unbindViewActions.Remove(unregister) == false)
            {
                return;
            }

            unregister.Invoke();
        };
    }

    /// <summary>
    /// Register a button click handler that publishes the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="button">The button that should publish the action.</param>
    /// <param name="action">The action that should be published.</param>
    /// <param name="beforePublish">Optional pre-publish logic.</param>
    protected Action RegisterViewAction([CanBeNull] Button button, [NotNull] IViewAction action, [CanBeNull] Action beforePublish = null)
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return RegisterViewAction(button, () => action, beforePublish);
    }

    /// <summary>
    /// Register a button click handler that publishes a dynamically created action.
    /// </summary>
    /// <param name="button">The button that should publish the action.</param>
    /// <param name="actionFactory">The factory used to create an action.</param>
    /// <param name="beforePublish">Optional pre-publish logic.</param>
    protected Action RegisterViewAction([CanBeNull] Button button, [NotNull] Func<IViewAction> actionFactory, [CanBeNull] Action beforePublish = null)
    {
        if(button == null)
        {
            return null;
        }

        if(actionFactory == null)
        {
            throw new ArgumentNullException(nameof(actionFactory));
        }

        void Handler()
        {
            beforePublish?.Invoke();
            PublishViewAction(actionFactory());
        }

        button.clicked += Handler;

        Action unregister = () => button.clicked -= Handler;
        _unbindViewActions.Add(unregister);
        return () =>
        {
            if(_unbindViewActions.Remove(unregister) == false)
            {
                return;
            }

            unregister.Invoke();
        };
    }

    /// <summary>
    /// Unregister a previously registered view-action callback.
    /// </summary>
    /// <param name="unregisterViewAction">The unregister token returned from <see cref="RegisterViewAction(UnityEngine.UIElements.Button,System.Func{BE.Emulator.Framework.IViewAction},System.Action)"/> or a related overload.</param>
    protected void UnregisterViewAction([CanBeNull] Action unregisterViewAction)
    {
        unregisterViewAction?.Invoke();
    }

    /// <summary>
    /// Remove all action callbacks that were registered through the base view helpers.
    /// </summary>
    protected void UnbindViewActions()
    {
        foreach(Action unbindAction in _unbindViewActions)
        {
            unbindAction.Invoke();
        }

        _unbindViewActions.Clear();
    }
}
}
