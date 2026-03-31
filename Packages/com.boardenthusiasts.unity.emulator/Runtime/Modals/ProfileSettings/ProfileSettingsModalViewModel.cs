using System;

using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.ProfileSettings
{
/// <summary>
/// View model for the profile settings modal.
/// </summary>
[Serializable]
internal sealed class ProfileSettingsModalViewModel : IViewModel
{
    /// <summary>
    /// The template to clone when constructing the profile settings modal.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }
}
}
