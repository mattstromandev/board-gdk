using System;
using System.Collections.Generic;

using Board.Core;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Serializable emulator data describing the application-facing Board state.
/// </summary>
[Serializable]
public sealed class EmulatorApplicationData
{
    [SerializeField]
    [HideInInspector]
    [Tooltip("Whether the emulator should currently consider the profile switcher visible.")]
    private bool m_isProfileSwitcherVisible;

    [SerializeField]
    [HideInInspector]
    [Tooltip("Whether a pause-screen context has been explicitly set in the emulator.")]
    private bool m_hasPauseScreenContext;

    [SerializeField]
    [Tooltip("The application name shown in the Board pause screen.")]
    private string m_applicationName;

    [SerializeField]
    [Tooltip("Whether Board should offer a save prompt when exiting through the pause screen.")]
    private bool m_showSaveOptionUponExit;

    [SerializeField]
    [Tooltip("The custom buttons shown in the Board pause screen.")]
    private List<BoardPauseCustomButton> m_customButtons = new();

    [SerializeField]
    [Tooltip("The audio tracks shown in the Board pause screen.")]
    private List<BoardPauseAudioTrack> m_audioTracks = new();

    /// <summary>
    /// Whether the emulator currently considers the profile switcher visible.
    /// </summary>
    public bool IsProfileSwitcherVisible
    {
        get => m_isProfileSwitcherVisible;
        set => m_isProfileSwitcherVisible = value;
    }

    /// <summary>
    /// Whether a pause-screen context has been explicitly established.
    /// </summary>
    public bool HasPauseScreenContext
    {
        get => m_hasPauseScreenContext;
        set => m_hasPauseScreenContext = value;
    }

    /// <summary>
    /// The application name shown in the Board pause screen.
    /// </summary>
    public string ApplicationName
    {
        get => m_applicationName;
        set => m_applicationName = value;
    }

    /// <summary>
    /// Whether Board should offer a save prompt when exiting through the pause screen.
    /// </summary>
    public bool ShowSaveOptionUponExit
    {
        get => m_showSaveOptionUponExit;
        set => m_showSaveOptionUponExit = value;
    }

    /// <summary>
    /// The custom buttons shown in the Board pause screen.
    /// </summary>
    public List<BoardPauseCustomButton> CustomButtons
    {
        get => m_customButtons;
        set => m_customButtons = value ?? new List<BoardPauseCustomButton>();
    }

    /// <summary>
    /// The audio tracks shown in the Board pause screen.
    /// </summary>
    public List<BoardPauseAudioTrack> AudioTracks
    {
        get => m_audioTracks;
        set => m_audioTracks = value ?? new List<BoardPauseAudioTrack>();
    }
}
}
