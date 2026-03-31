using System;

using Zenject;

namespace BE.Emulator.Framework
{
/// <summary>
/// Interface for a view which uses an <see cref="IViewModel"/> and is controlled by an <see cref="IDisplayableController{TModel,TView,TViewModel}"/>.
/// </summary>
public interface IView<out T> : IInitializable, IDisposable, IDisplayable where T : IViewModel
{
    /// <summary>
    /// Raised when the view publishes an <see cref="IViewAction"/> for its controller to handle.
    /// </summary>
    public event EventHandler<ViewActionEventArgs> ViewActionTriggered;

    /// <summary>
    /// The <see cref="IViewModel"/> used by this view.
    /// </summary>
    protected T ViewModel { get; }
}
}
