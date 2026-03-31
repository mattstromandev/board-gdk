using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Rahmen.Logging;

namespace BE.Emulator.Framework
{
/// <inheritdoc cref="IDisplayActionRouter"/>
public sealed class DisplayActionRouter : IDisplayActionRouter
{
    private readonly IRahmenLogger _logger;
    private readonly IReadOnlyList<IDisplayActionRoute> _routes;
    private readonly IReadOnlyList<IViewActionHandlerRoute> _handlerRoutes;
    private readonly IReadOnlyDictionary<Type, IEmulatorDisplay> _displaysByType;
    private readonly IReadOnlyDictionary<Type, IViewActionHandler> _handlersByType;
    private readonly IDisplayHostResolver _displayHostResolver;
    private readonly IEmulatorDisplayStateStore _displayStateStore;
    private readonly IEmulatorDisplayStateSynchronizer _displayStateSynchronizer;

    /// <summary>
    /// Creates the display action router.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for routing diagnostics.</param>
    /// <param name="routes">The configured display routes.</param>
    /// <param name="displays">The displays that may be targeted by routes.</param>
    /// <param name="displayHostResolver">The resolver used to attach displays when required.</param>
    /// <param name="displayStateStore">The desired display visibility state store.</param>
    /// <param name="displayStateSynchronizer">The shell state synchronizer that should run after display operations.</param>
    public DisplayActionRouter(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] IEnumerable<IDisplayActionRoute> routes,
        [NotNull] IEnumerable<IEmulatorDisplay> displays,
        [CanBeNull] IEnumerable<IViewActionHandlerRoute> handlerRoutes,
        [CanBeNull] IEnumerable<IViewActionHandler> handlers,
        [NotNull] IDisplayHostResolver displayHostResolver,
        [NotNull] IEmulatorDisplayStateStore displayStateStore,
        [NotNull] IEmulatorDisplayStateSynchronizer displayStateSynchronizer)
    {
        if(loggerFactory == null)
        {
            throw new ArgumentNullException(nameof(loggerFactory));
        }

        _logger = loggerFactory.Get<LogChannels.BoardEmulation>(this);
        _routes = routes?.ToArray() ?? throw new ArgumentNullException(nameof(routes));
        _handlerRoutes = handlerRoutes?.ToArray() ?? Array.Empty<IViewActionHandlerRoute>();
        _displayHostResolver = displayHostResolver ?? throw new ArgumentNullException(nameof(displayHostResolver));
        _displayStateStore = displayStateStore ?? throw new ArgumentNullException(nameof(displayStateStore));
        _displayStateSynchronizer = displayStateSynchronizer ?? throw new ArgumentNullException(nameof(displayStateSynchronizer));
        _displaysByType = displays?.ToDictionary(display => display.GetType()) ?? throw new ArgumentNullException(nameof(displays));
        _handlersByType = handlers?.ToDictionary(handler => handler.GetType()) ?? new Dictionary<Type, IViewActionHandler>();
    }

    /// <inheritdoc />
    public void Route(object sender, IViewAction action)
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        Type actionType = action.GetType();
        IDisplayActionRoute[] matchingRoutes = _routes
            .Where(route => route.ActionType.IsAssignableFrom(actionType))
            .ToArray();
        IViewActionHandlerRoute[] matchingHandlerRoutes = _handlerRoutes
            .Where(route => route.ActionType.IsAssignableFrom(actionType))
            .ToArray();

        if(matchingRoutes.Length == 0 && matchingHandlerRoutes.Length == 0)
        {
            _logger.Info()?.Log($"No action routes were configured for action <{actionType.FullName}>.");
            return;
        }

        foreach(IViewActionHandlerRoute handlerRoute in matchingHandlerRoutes)
        {
            if(_handlersByType.TryGetValue(handlerRoute.TargetHandlerType, out IViewActionHandler targetHandler) == false)
            {
                _logger.Error()?.Log($"Could not resolve target action handler type <{handlerRoute.TargetHandlerType.FullName}> for action route <{actionType.FullName}>.");
                continue;
            }

            targetHandler.HandleRoutedViewAction(sender, action);
        }

        foreach(IDisplayActionRoute route in matchingRoutes)
        {
            if(_displaysByType.TryGetValue(route.TargetDisplayType, out IEmulatorDisplay targetDisplay) == false)
            {
                _logger.Error()?.Log($"Could not resolve target display type <{route.TargetDisplayType.FullName}> for action route <{actionType.FullName}>.");
                continue;
            }

            ApplyRoute(targetDisplay, route);
        }

        _displayStateSynchronizer.Refresh();
    }

    private void ApplyRoute([NotNull] IEmulatorDisplay display, [NotNull] IDisplayActionRoute route)
    {
        switch(route.Operation)
        {
            case EmulatorDisplayOperation.Attach:
                EnsureAttached(display);
                break;

            case EmulatorDisplayOperation.Show:
                _displayStateStore.SetVisible(route.TargetDisplayType, true);
                if(EnsureAttached(display))
                {
                    if(display.IsVisible == false)
                    {
                        display.Show();
                    }
                }
                break;

            case EmulatorDisplayOperation.Hide:
                _displayStateStore.SetVisible(route.TargetDisplayType, false);
                if(display.IsAttached && display.IsVisible)
                {
                    display.Hide();
                }
                break;

            case EmulatorDisplayOperation.Detach:
                _displayStateStore.SetVisible(route.TargetDisplayType, false);
                if(display.IsAttached)
                {
                    display.Detach();
                }
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool EnsureAttached([NotNull] IEmulatorDisplay display)
    {
        if(display.IsAttached)
        {
            return true;
        }

        try
        {
            DisplayHostResolution resolution = _displayHostResolver.Resolve(display);
            display.Attach(resolution.Host, resolution.ClearHostOnAttach);
            return true;
        }
        catch(InvalidOperationException exception)
        {
            _logger.Warning()?.Log($"Display <{display.Name}> could not be attached yet: {exception.Message}");
            return false;
        }
    }
}
}
