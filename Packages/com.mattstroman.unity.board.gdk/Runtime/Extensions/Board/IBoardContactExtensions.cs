using System.Runtime.InteropServices;

using Board.Input;

using BoardGDK.BoardAdapters;

using UnityEngine;

namespace BoardGDK.Extensions.Board
{
/// <summary>
/// Extension methods for the <see cref="IBoardContact"/> class.
/// </summary>
public static class IBoardContactExtensions
{
    [StructLayout(LayoutKind.Explicit, Size = _boardContactEventSize)]
    private struct BoardContactEventData
    {
        [FieldOffset(0)] public int contactId;
        [FieldOffset(4)] public Vector2 position;
        [FieldOffset(12)] public float orientation;
        [FieldOffset(16)] public byte typeId;
        [FieldOffset(17)] public byte phaseId;
        [FieldOffset(18)] public int glyphId;
        [FieldOffset(22)] public Vector2 center;
        [FieldOffset(30)] public Vector2 extents;
        [FieldOffset(38)] public bool isTouched;
    }

    [StructLayout(LayoutKind.Explicit, Size = _boardContactEventSize + 21)]
    private struct BoardContactData
    {
        [FieldOffset(0)] public BoardContactEventData inputEvent;
        [FieldOffset(_previousScreenPositionOffset)] public Vector2 previousScreenPosition;
        [FieldOffset(_previousOrientationOffset)] public float previousOrientation;
        [FieldOffset(_timestampOffset)] public double timestamp;
        [FieldOffset(_previousPhaseOffset)] public byte previousPhase;
    }
    
    private const int _boardContactEventSize = 39;
    private const int _previousScreenPositionOffset = _boardContactEventSize;
    private const int _previousOrientationOffset = _boardContactEventSize + 8;
    private const int _timestampOffset = _boardContactEventSize + 12;
    private const int _previousPhaseOffset = _boardContactEventSize + 20;
    
    /// <summary>
    /// Convert this <see cref="IBoardContact"/> to a <see cref="BoardContact"/>.
    /// </summary>
    /// <returns>A <see cref="BoardContact"/> with the values from this <see cref="IBoardContact"/>.</returns>
    public static BoardContact AsBoardContact(this IBoardContact me)
    {
        BoardContactData data = new()
        {
            inputEvent = new BoardContactEventData
            {
                contactId = me.ContactId,
                position = me.ScreenPosition,
                orientation = me.Orientation,
                typeId = (byte)me.Type,
                phaseId = (byte)me.Phase,
                glyphId = me.GlyphId,
                center = me.Bounds.center,
                // BoardContact bounds layout stores size at this offset.
                extents = me.Bounds.size,
                isTouched = me.IsTouched
            },
            previousScreenPosition = me.PreviousScreenPosition,
            previousOrientation = me.PreviousOrientation,
            timestamp = me.Timestamp,
            previousPhase = (byte)me.Phase
        };

        return MemoryMarshal.Cast<BoardContactData, BoardContact>(MemoryMarshal.CreateSpan(ref data, 1))[0];
    }

    /// <summary>
    /// Convert this <see cref="BoardContact"/> to an <see cref="IBoardContact"/>.
    /// </summary>
    /// <returns>An <see cref="IBoardContact"/> with the values from this <see cref="BoardContact"/>.</returns>
    public static IBoardContact AsIBoardContact(this BoardContact me)
    {
        return new SerializableBoardContact(me);
    }
    
    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me)
    {
        return me.AsBoardContact().GetWorldPosition();
    }
    
    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Vector3)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Vector3 worldUpAxis)
    {
        return me.AsBoardContact().GetWorldPosition(worldUpAxis);
    }
    
    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Vector3, LayerMask)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Vector3 worldUpAxis, LayerMask surfaceLayers)
    {
        return me.AsBoardContact().GetWorldPosition(worldUpAxis, surfaceLayers);
    }
    
    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Vector3, LayerMask, Vector3)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Vector3 worldUpAxis, LayerMask surfaceLayers, Vector3 referenceWorldPosition)
    {
        return me.AsBoardContact().GetWorldPosition(worldUpAxis, surfaceLayers, referenceWorldPosition);
    }

    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Camera)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Camera camera)
    {
        return me.AsBoardContact().GetWorldPosition(camera);
    }

    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Camera, Vector3)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Camera camera, Vector3 worldUpAxis)
    {
        return me.AsBoardContact().GetWorldPosition(camera, worldUpAxis);
    }

    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Camera, Vector3, Vector3)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Camera camera, Vector3 worldUpAxis, Vector3 referenceWorldPosition)
    {
        return me.AsBoardContact().GetWorldPosition(camera, worldUpAxis, referenceWorldPosition);
    }

    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Camera, Vector3, Vector2, Vector3)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Camera camera, Vector3 worldUpAxis, Vector2 screenSpaceOffset, Vector3 referenceWorldPosition)
    {
        return me.AsBoardContact().GetWorldPosition(camera, worldUpAxis, screenSpaceOffset, referenceWorldPosition);
    }

    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Camera, Vector3, LayerMask)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Camera camera, Vector3 worldUpAxis, LayerMask surfaceLayers)
    {
        return me.AsBoardContact().GetWorldPosition(camera, worldUpAxis, surfaceLayers);
    }

    /// <inheritdoc cref="BoardContactExtensions.GetWorldPosition(BoardContact, Camera, Vector3, LayerMask, Vector3)"/>
    public static Vector3 GetWorldPosition(this IBoardContact me, Camera camera, Vector3 worldUpAxis, LayerMask surfaceLayers, Vector3 referenceWorldPosition)
    {
        return me.AsBoardContact().GetWorldPosition(camera, worldUpAxis, surfaceLayers, referenceWorldPosition);
    }
}
}
