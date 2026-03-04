using System;

using Board.Input;

using Rahmen.Extensions;

using UnityEngine;

namespace BoardGDK.BoardAdapters
{
/// <summary>
/// Interface for a read-only version of <see cref="BoardContact"/> to be used when adapting Board data for use with the
/// <see cref="BoardGDK"/>.
/// </summary>
public interface IBoardContact
{
    /// <inheritdoc cref="BoardContact.contactId"/>
    public int ContactId { get; }

    /// <inheritdoc cref="BoardContact.glyphId"/>
    public int GlyphId { get; }

    /// <inheritdoc cref="BoardContact.screenPosition"/>
    public Vector2 ScreenPosition { get; }

    /// <inheritdoc cref="BoardContact.previousScreenPosition"/>
    public Vector2 PreviousScreenPosition { get; }

    /// <inheritdoc cref="BoardContact.orientation"/>
    public float Orientation { get; }

    /// <summary>
    /// The <see cref="Orientation"/> in degrees.
    /// </summary>
    public float OrientationDegrees => Orientation.ToDegrees();

    /// <inheritdoc cref="BoardContact.previousOrientation"/>
    public float PreviousOrientation { get; }

    /// <summary>
    /// The <see cref="PreviousOrientation"/> in degrees.
    /// </summary>
    public float PreviousOrientationDegrees => PreviousOrientation.ToDegrees();

    /// <inheritdoc cref="BoardContact.timestamp"/>
    public double Timestamp { get; }

    /// <inheritdoc cref="BoardContact.phase"/>
    public BoardContactPhase Phase { get; }

    /// <inheritdoc cref="BoardContact.type"/>
    public BoardContactType Type { get; }

    /// <inheritdoc cref="BoardContact.isTouched"/>
    public bool IsTouched { get; }

    /// <inheritdoc cref="BoardContact.bounds"/>
    public Bounds Bounds { get; }

    /// <inheritdoc cref="BoardContact.isNoneEndedOrCanceled"/>
    public bool IsNoneEndedOrCanceled { get; }

    /// <inheritdoc cref="BoardContact.isInProgress"/>
    public bool IsInProgress { get; }
}
}
