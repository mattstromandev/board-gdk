using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace BE.Emulator.Utility
{
/// <summary>
/// Shared helpers for avatar background colors used by the emulator UI.
/// </summary>
internal static class EmulatorAvatarUtility
{
    private static readonly IReadOnlyList<Color> s_backgroundPalette = new Color[]
    {
        new Color32(113, 31, 237, 255),
        new Color32(129, 166, 85, 255),
        new Color32(39, 95, 247, 255),
        new Color32(195, 220, 222, 255),
        new Color32(79, 192, 143, 255),
        new Color32(245, 151, 58, 255)
    };

    /// <summary>
    /// The curated avatar background colors used when a profile has not chosen a custom color yet.
    /// </summary>
    public static IReadOnlyList<Color> BackgroundPalette => s_backgroundPalette;

    /// <summary>
    /// Gets whether a serialized avatar background color has been assigned.
    /// </summary>
    /// <param name="color">The color to inspect.</param>
    /// <returns><see langword="true"/> when the color is considered assigned; otherwise, <see langword="false"/>.</returns>
    public static bool HasSerializedColor(Color color)
    {
        return color.a > 0f;
    }

    /// <summary>
    /// Resolves a palette color for the provided index.
    /// </summary>
    /// <param name="index">The zero-based palette index.</param>
    /// <returns>The resolved palette color.</returns>
    public static Color GetPaletteColor(int index)
    {
        int paletteCount = s_backgroundPalette.Count;
        if(paletteCount == 0)
        {
            return new Color(0.76f, 0.80f, 0.84f, 1f);
        }

        int normalizedIndex = Mathf.Abs(index) % paletteCount;
        return s_backgroundPalette[normalizedIndex];
    }

    /// <summary>
    /// Creates a flat avatar texture that can be used when no custom avatar icon has been configured.
    /// </summary>
    /// <param name="color">The background color that should fill the generated texture.</param>
    /// <param name="hideAndDontSave"><see langword="true"/> to mark the texture as transient runtime-only data.</param>
    /// <returns>The generated avatar texture.</returns>
    public static Texture2D CreateAvatarTexture(Color color, bool hideAndDontSave = false)
    {
        Texture2D texture = new(64, 64, TextureFormat.RGBA32, false)
        {
            name = $"EmulatorAvatar_{ColorUtility.ToHtmlStringRGB(color)}"
        };

        if(hideAndDontSave)
        {
            texture.hideFlags = HideFlags.HideAndDontSave;
        }

        Color[] pixels = new Color[64 * 64];
        for(int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Gets a random palette color that differs from the provided current color.
    /// </summary>
    /// <param name="currentColor">The current color that should be avoided when possible.</param>
    /// <returns>A random palette color.</returns>
    public static Color GetRandomPaletteColor(Color currentColor)
    {
        List<Color> candidateColors = s_backgroundPalette
            .Where(color => AreEquivalent(color, currentColor) == false)
            .ToList();
        if(candidateColors.Count == 0)
        {
            return GetPaletteColor(0);
        }

        return candidateColors[Random.Range(0, candidateColors.Count)];
    }

    /// <summary>
    /// Gets whether the supplied colors should be treated as equivalent for emulator UI purposes.
    /// </summary>
    /// <param name="first">The first color to compare.</param>
    /// <param name="second">The second color to compare.</param>
    /// <returns><see langword="true"/> when the colors are effectively the same; otherwise, <see langword="false"/>.</returns>
    public static bool AreEquivalent(Color first, Color second)
    {
        return Mathf.Approximately(first.r, second.r)
            && Mathf.Approximately(first.g, second.g)
            && Mathf.Approximately(first.b, second.b)
            && Mathf.Approximately(first.a, second.a);
    }
}
}
