using System.Reflection;

using Board.Input;
using BoardGDK.BoardAdapters;

using NUnit.Framework;

using UnityEngine;

namespace BoardGDK.Tests
{
[TestFixture]
public class SerializableBoardContactTests
{
    [Test]
    public void ExplicitConversion_PreservesValues()
    {
        SerializableBoardContact serializable = new();
        Vector2 screenPosition = new(120.5f, 340.25f);
        Vector2 previousScreenPosition = new(110.5f, 330.25f);
        Bounds bounds = new(new Vector3(5f, 6f, 0f), new Vector3(12f, 14f, 0f));
        BoardContactPhase phase = BoardContactPhase.Moved;
        BoardContactType type = BoardContactType.Glyph;
        bool isNoneEndedOrCanceled = phase == BoardContactPhase.None || phase == BoardContactPhase.Ended || phase == BoardContactPhase.Canceled;
        bool isInProgress = phase == BoardContactPhase.Began || phase == BoardContactPhase.Moved || phase == BoardContactPhase.Stationary;

        SetField(ref serializable, "m_contactId", 42);
        SetField(ref serializable, "m_glyphId", 7);
        SetField(ref serializable, "m_screenPosition", screenPosition);
        SetField(ref serializable, "m_previousScreenPosition", previousScreenPosition);
        SetField(ref serializable, "m_orientation", 1.2f);
        SetField(ref serializable, "m_previousOrientation", 0.9f);
        SetField(ref serializable, "m_timestamp", 123.456);
        SetField(ref serializable, "m_phase", phase);
        SetField(ref serializable, "m_type", type);
        SetField(ref serializable, "m_isTouched", true);
        SetField(ref serializable, "m_bounds", bounds);
        SetField(ref serializable, "m_isNoneEndedOrCanceled", isNoneEndedOrCanceled);
        SetField(ref serializable, "m_isInProgress", isInProgress);

        BoardContact boardContact = (BoardContact)serializable;

        Assert.AreEqual(42, boardContact.contactId);
        Assert.AreEqual(7, boardContact.glyphId);
        Assert.AreEqual(screenPosition, boardContact.screenPosition);
        Assert.AreEqual(previousScreenPosition, boardContact.previousScreenPosition);
        Assert.AreEqual(1.2f, boardContact.orientation, 0.0001f);
        Assert.AreEqual(0.9f, boardContact.previousOrientation, 0.0001f);
        Assert.AreEqual(123.456, boardContact.timestamp, 0.0001);
        Assert.AreEqual(phase, boardContact.phase);
        Assert.AreEqual(type, boardContact.type);
        Assert.AreEqual(true, boardContact.isTouched);
        Assert.AreEqual(bounds.center, boardContact.bounds.center);
        Assert.AreEqual(bounds.size, boardContact.bounds.size);
        Assert.AreEqual(isNoneEndedOrCanceled, boardContact.isNoneEndedOrCanceled);
        Assert.AreEqual(isInProgress, boardContact.isInProgress);

        SerializableBoardContact roundTrip = new(boardContact);

        Assert.AreEqual(serializable.ContactId, roundTrip.ContactId);
        Assert.AreEqual(serializable.GlyphId, roundTrip.GlyphId);
        Assert.AreEqual(serializable.ScreenPosition, roundTrip.ScreenPosition);
        Assert.AreEqual(serializable.PreviousScreenPosition, roundTrip.PreviousScreenPosition);
        Assert.AreEqual(serializable.Orientation, roundTrip.Orientation, 0.0001f);
        Assert.AreEqual(serializable.PreviousOrientation, roundTrip.PreviousOrientation, 0.0001f);
        Assert.AreEqual(serializable.Timestamp, roundTrip.Timestamp, 0.0001);
        Assert.AreEqual(serializable.Phase, roundTrip.Phase);
        Assert.AreEqual(serializable.Type, roundTrip.Type);
        Assert.AreEqual(serializable.IsTouched, roundTrip.IsTouched);
        Assert.AreEqual(serializable.Bounds.center, roundTrip.Bounds.center);
        Assert.AreEqual(serializable.Bounds.size, roundTrip.Bounds.size);
        Assert.AreEqual(isNoneEndedOrCanceled, roundTrip.IsNoneEndedOrCanceled);
        Assert.AreEqual(isInProgress, roundTrip.IsInProgress);
    }

    [Test]
    public void ImplicitConversion_RoundTripPreservesValues()
    {
        SerializableBoardContact serializable = new();
        Vector2 screenPosition = new(120.5f, 340.25f);
        Vector2 previousScreenPosition = new(110.5f, 330.25f);
        Bounds bounds = new(new Vector3(5f, 6f, 0f), new Vector3(12f, 14f, 0f));
        BoardContactPhase phase = BoardContactPhase.Moved;
        BoardContactType type = BoardContactType.Glyph;
        bool isNoneEndedOrCanceled = phase == BoardContactPhase.None || phase == BoardContactPhase.Ended || phase == BoardContactPhase.Canceled;
        bool isInProgress = phase == BoardContactPhase.Began || phase == BoardContactPhase.Moved || phase == BoardContactPhase.Stationary;

        SetField(ref serializable, "m_contactId", 42);
        SetField(ref serializable, "m_glyphId", 7);
        SetField(ref serializable, "m_screenPosition", screenPosition);
        SetField(ref serializable, "m_previousScreenPosition", previousScreenPosition);
        SetField(ref serializable, "m_orientation", 1.2f);
        SetField(ref serializable, "m_previousOrientation", 0.9f);
        SetField(ref serializable, "m_timestamp", 123.456);
        SetField(ref serializable, "m_phase", phase);
        SetField(ref serializable, "m_type", type);
        SetField(ref serializable, "m_isTouched", true);
        SetField(ref serializable, "m_bounds", bounds);
        SetField(ref serializable, "m_isNoneEndedOrCanceled", isNoneEndedOrCanceled);
        SetField(ref serializable, "m_isInProgress", isInProgress);

        BoardContact boardContact = (BoardContact)serializable;
        SerializableBoardContact roundTrip = boardContact;

        Assert.AreEqual(serializable.ContactId, roundTrip.ContactId);
        Assert.AreEqual(serializable.GlyphId, roundTrip.GlyphId);
        Assert.AreEqual(serializable.ScreenPosition, roundTrip.ScreenPosition);
        Assert.AreEqual(serializable.PreviousScreenPosition, roundTrip.PreviousScreenPosition);
        Assert.AreEqual(serializable.Orientation, roundTrip.Orientation, 0.0001f);
        Assert.AreEqual(serializable.PreviousOrientation, roundTrip.PreviousOrientation, 0.0001f);
        Assert.AreEqual(serializable.Timestamp, roundTrip.Timestamp, 0.0001);
        Assert.AreEqual(serializable.Phase, roundTrip.Phase);
        Assert.AreEqual(serializable.Type, roundTrip.Type);
        Assert.AreEqual(serializable.IsTouched, roundTrip.IsTouched);
        Assert.AreEqual(serializable.Bounds.center, roundTrip.Bounds.center);
        Assert.AreEqual(serializable.Bounds.size, roundTrip.Bounds.size);
        Assert.AreEqual(isNoneEndedOrCanceled, roundTrip.IsNoneEndedOrCanceled);
        Assert.AreEqual(isInProgress, roundTrip.IsInProgress);
    }

    private static void SetField<T>(ref SerializableBoardContact contact, string fieldName, T value)
    {
        FieldInfo field = typeof(SerializableBoardContact).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field, $"Field '{fieldName}' was not found.");
        object boxed = contact;
        field.SetValue(boxed, value);
        contact = (SerializableBoardContact)boxed;
    }
}
}
