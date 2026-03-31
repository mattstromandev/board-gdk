using System;

using Zenject;

namespace BE.Emulator.Framework
{
/// <inheritdoc cref="IController{TModel}"/>
/// <summary>
/// Interface for a controller which operates on an <see cref="IModel"/> of type <typeparamref name="TModel"/> and controls
/// an <see cref="IView{T}"/> of type <typeparamref name="TView"/>.
/// </summary>
/// <typeparam name="TView">The type of <see cref="IView{T}"/> that is used.</typeparam>
/// <typeparam name="TViewModel">The type of <see cref="IViewModel"/> that is used by the <see cref="IView{T}"/>.</typeparam>
// ReSharper disable once InvalidXmlDocComment - docs for TModel inherited
public interface IDisplayableController<out TModel, out TView, TViewModel>
    : IController<TModel>, IInitializable, IDisposable, IDisplayable
    where TModel : IModel where TView : IView<TViewModel> where TViewModel : IViewModel
{
    /// <summary>
    /// The <see cref="TView"/> used by this controller.
    /// </summary>
    protected TView View { get; }
}
}
