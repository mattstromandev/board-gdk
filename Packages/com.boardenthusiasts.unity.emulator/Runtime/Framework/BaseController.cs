using System;

using JetBrains.Annotations;

using Rahmen.Logging;

namespace BE.Emulator.Framework
{
/// <inheritdoc cref="IController{TModel}"/>
/// <summary>
/// Base class for an <see cref="IController{TModel}"/> handling default functionality.
/// </summary>
public abstract class BaseController<TModel> : IController<TModel> where TModel : IModel
{
    TModel IController<TModel>.Model => _model;
    
    private readonly IRahmenLogger _logger;
    private readonly TModel _model;

    protected BaseController([NotNull] ILoggerFactory loggerFactory, [NotNull]  TModel model)
    {
        _logger = loggerFactory?.Get<LogChannels.BoardEmulation>(this) ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger.Trace()?.Log("");
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    /// <inheritdoc />
    public virtual void Initialize()
    {
        _logger.Trace()?.Log("");
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        _logger.Trace()?.Log("");
    }
}
}
