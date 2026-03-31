using System;

namespace BE.Emulator.Framework
{
/// <summary>
/// Stores the desired display state independently from the current UI Toolkit attachment lifecycle.
/// </summary>
public interface IEmulatorDisplayStateStore
{
    /// <summary>
    /// Returns whether the target display type is currently intended to be visible.
    /// </summary>
    /// <param name="displayType">The display type to query.</param>
    /// <returns><see langword="true"/> if the display should be visible; otherwise <see langword="false"/>.</returns>
    public bool IsVisible(Type displayType);

    /// <summary>
    /// Returns whether the target display type is currently intended to be visible.
    /// </summary>
    /// <typeparam name="TDisplay">The display type to query.</typeparam>
    /// <returns><see langword="true"/> if the display should be visible; otherwise <see langword="false"/>.</returns>
    public bool IsVisible<TDisplay>() where TDisplay : IEmulatorDisplay;

    /// <summary>
    /// Updates the desired visibility for the target display type.
    /// </summary>
    /// <param name="displayType">The display type to update.</param>
    /// <param name="isVisible">Whether the display should be visible.</param>
    public void SetVisible(Type displayType, bool isVisible);
}
}
