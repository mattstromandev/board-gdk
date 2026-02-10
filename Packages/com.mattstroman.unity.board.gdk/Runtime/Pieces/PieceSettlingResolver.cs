using System;
using System.Collections.Generic;
using System.Linq;

using Board.Input;

using JetBrains.Annotations;

namespace BoardGDK.Pieces
{
/// <summary>
/// Resolver for <see cref="IPieceSettlingStrategy"/>s.
/// </summary>
internal class PieceSettlingResolver
{
    private readonly IReadOnlyCollection<IPieceSettlingStrategy> _settlingStrategies;
    
    /// <summary>
    /// Construct a new <see cref="PieceSettlingResolver"/>.
    /// </summary>
    /// <param name="strategies">The set of <see cref="IPieceSettlingStrategy"/>s that should be resolved.</param>
    public PieceSettlingResolver([NotNull] IReadOnlyCollection<IPieceSettlingStrategy> strategies)
    {
        _settlingStrategies = strategies ?? Array.Empty<IPieceSettlingStrategy>();
    }

    /// <summary>
    /// Determine if a <see cref="BoardContact"/> is considered settled according to all the injected strategies.
    /// </summary>
    /// <param name="context">The <see cref="IPieceSettlingContext"/> providing necessary information for settling logic.</param>
    /// <returns>True if the contact has settled; false otherwise.</returns>
    public bool HaveSettled(IPieceSettlingContext context)
    {
        return _settlingStrategies.All(strategy => strategy.HasSettled(context));
    }
}
}
