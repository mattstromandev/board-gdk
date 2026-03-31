using System;

using BoardGDK.Pieces;
using BoardGDK.Pieces.Behaviors;

using Rahmen.Logging;

namespace BoardGDK.LogChannels
{
/// <summary>
/// <see cref="LogChannel"/> for the <see cref="IPieceSystem"/>.
/// </summary>
[Serializable]
public class PieceSystem : LogChannel
{
    /// <inheritdoc />
    protected override string InitialDescription => $"Channel for {nameof(PieceSystem)} logging.";
}

/// <summary>
/// <see cref="LogChannel"/> for the <see cref="IPieceBehaviorSystem"/>.
/// </summary>
[Serializable]
public class PieceBehaviorSystem : LogChannel
{
    /// <inheritdoc />
    protected override string InitialDescription => $"Channel for {nameof(PieceBehaviorSystem)} logging.";
}
}
