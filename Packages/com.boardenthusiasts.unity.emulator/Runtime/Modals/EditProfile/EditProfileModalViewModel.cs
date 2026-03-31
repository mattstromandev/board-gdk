using System;

using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.EditProfile
{
/// <summary>
/// View model for the shared edit-profile modal.
/// </summary>
[Serializable]
internal sealed class EditProfileModalViewModel : IViewModel
{
    /// <summary>
    /// The template to clone when constructing the shared edit-profile modal.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }
}
}
