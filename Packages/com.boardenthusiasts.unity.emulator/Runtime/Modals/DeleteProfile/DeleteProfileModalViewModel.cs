using System;

using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.DeleteProfile
{
/// <summary>
/// View model for the delete-profile modal.
/// </summary>
[Serializable]
internal sealed class DeleteProfileModalViewModel : IViewModel
{
    /// <summary>
    /// The template to clone when constructing the delete-profile modal.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }
}
}
