using System;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

using Zenject;

namespace BE.Emulator.Framework
{
/// <inheritdoc cref="BaseController{TModel}"/>
/// <summary>
/// Base class for an <see cref="IController{TModel}"/> that is associated with an <see cref="IView{T}"/>.
/// </summary>
/// <typeparam name="TView">The type of <see cref="IView{T}"/> used by the controller.</typeparam>
/// <typeparam name="TViewModel">The type of <see cref="IViewModel"/> used by the <see cref="IView{T}"/> of type <typeparamref name="TView"/>.</typeparam>
// ReSharper disable once InvalidXmlDocComment - docs for TModel inherited
public abstract class DisplayableController<TModel, TView, TViewModel>
    : BaseController<TModel>, IDisplayableController<TModel, TView, TViewModel>
    where TModel : IModel where TView : IView<TViewModel> where TViewModel : IViewModel
{
    /// <inheritdoc />
    public event EventHandler ReadyForAttachment
    {
        add => _view.ReadyForAttachment += value;
        remove => _view.ReadyForAttachment -= value;
    }

    /// <inheritdoc />
    public bool IsReady => _view.IsReady;
    /// <inheritdoc />
    public bool IsAttached => _view.IsAttached;
    /// <inheritdoc />
    public bool IsVisible => _view.IsVisible;

    TView IDisplayableController<TModel, TView, TViewModel>.View => _view;
    protected TView View => _view;

    private readonly IRahmenLogger _logger;
    private readonly TView _view;
    private readonly LazyInject<IDisplayActionRouter> _displayActionRouter;

    /// <summary>
    /// Creates the displayable controller.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="model">The model used by the controller.</param>
    /// <param name="view">The view controlled by this controller.</param>
    /// <param name="displayActionRouter">The router that should handle published view actions.</param>
    protected DisplayableController(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] TModel model,
        [NotNull] TView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter)
        : base(loggerFactory, model)
    {
        _logger = loggerFactory?.Get<LogChannels.BoardEmulation>(this) ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger.Trace()?.Log("");
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _displayActionRouter = displayActionRouter ?? throw new ArgumentNullException(nameof(displayActionRouter));
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        _view.ViewActionTriggered += OnViewActionTriggered;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _view.ViewActionTriggered -= OnViewActionTriggered;
        base.Dispose();
    }

    /// <inheritdoc />
    public void Attach(VisualElement host, bool clearHost = true, bool hideAfterAttach = true)
    {
        _view.Attach(host, clearHost, hideAfterAttach);
    }

    /// <inheritdoc />
    public void Show()
    {
        _view.Show();
    }

    /// <inheritdoc />
    public void Hide()
    {
        _view.Hide();
    }

    /// <inheritdoc />
    public void Detach()
    {
        _view.Detach();
    }

    /// <summary>
    /// Handle a view action published by the associated view.
    /// </summary>
    /// <param name="sender">The source that published the action.</param>
    /// <param name="action">The published action.</param>
    protected virtual void HandleViewAction([CanBeNull] object sender, [NotNull] IViewAction action)
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _displayActionRouter.Value.Route(sender, action);
    }

    /// <summary>
    /// Route the provided <paramref name="action"/> through the configured action router.
    /// </summary>
    protected void RouteViewAction([CanBeNull] object sender, [NotNull] IViewAction action)
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _displayActionRouter.Value.Route(sender, action);
    }

    private void OnViewActionTriggered(object sender, ViewActionEventArgs eventArgs)
    {
        if(eventArgs == null)
        {
            throw new ArgumentNullException(nameof(eventArgs));
        }

        _logger.Debug()?.Log($"Routing view action <{eventArgs.Action.GetType().FullName}> from <{typeof(TView).FullName}>.");
        HandleViewAction(sender, eventArgs.Action);
    }
}
}
