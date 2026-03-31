using System;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator.Services
{
/// <summary>
/// Owns the runtime <see cref="UIDocument"/> host used to render the emulator shell.
/// </summary>
internal sealed class EmulatorPanelHost : IDisposable
{
    private readonly UIDocument _uiDocument;
    private readonly GameObject _gameObject;

    /// <summary>
    /// Creates the panel host.
    /// </summary>
    /// <param name="settings">The settings that configure the host document.</param>
    public EmulatorPanelHost([NotNull] IEmulatorSettings settings)
    {
        if(settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _gameObject = new GameObject(nameof(EmulatorPanelHost));
        UnityEngine.Object.DontDestroyOnLoad(_gameObject);
        _uiDocument = _gameObject.AddComponent<UIDocument>();
        _uiDocument.panelSettings = settings.PanelSettings;
        _uiDocument.sortingOrder = settings.SortOrder;
    }

    /// <summary>
    /// Gets the root visual element for the host document.
    /// </summary>
    public VisualElement Root => _uiDocument.rootVisualElement;

    /// <inheritdoc />
    public void Dispose()
    {
        if(_gameObject != null)
        {
            UnityEngine.Object.Destroy(_gameObject);
        }
    }
}
}
