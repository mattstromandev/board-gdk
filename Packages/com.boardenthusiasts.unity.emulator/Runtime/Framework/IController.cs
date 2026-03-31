using System;

using Zenject;

namespace BE.Emulator.Framework
{
/// <summary>
/// Interface for a controller which operates on an <see cref="IModel"/> of type <typeparamref name="TModel"/>.
/// </summary>
/// <typeparam name="TModel">The type of <see cref="IModel"/> used.</typeparam>
public interface IController<out TModel> : IInitializable, IDisposable where TModel : IModel
{
    /// <summary>
    /// The <see cref="IModel"/> used by this controller.
    /// </summary>
    protected TModel Model { get; }
}
}
