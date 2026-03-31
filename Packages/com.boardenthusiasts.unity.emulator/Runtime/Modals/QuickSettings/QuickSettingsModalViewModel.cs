using System;

using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.QuickSettings
{
/// <summary>
/// View model for the quick settings modal.
/// </summary>
[Serializable]
internal sealed class QuickSettingsModalViewModel : IViewModel
{
    /// <summary>
    /// The template to clone when constructing the quick settings modal view tree.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }
}
}
