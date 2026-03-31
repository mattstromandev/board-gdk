using System;
using System.Reflection;

using BE.Emulator.Data;

namespace BE.Emulator.Persistence
{
/// <summary>
/// Runtime-visible bridge used by the editor settings provider to expose the active project settings asset.
/// </summary>
public static class EmulatorProjectSettingsBridge
{
    internal const string SettingsRootPath = "Assets/Settings";
    internal const string PackageDisplayName = "BE Emulator for Board";
    internal const string MockDataAssetFileName = "BE Emulator Mock Data.asset";
    internal const string MockDataSettingsFolderPath = SettingsRootPath + "/" + PackageDisplayName;
    internal const string DefaultMockDataAssetPath = MockDataSettingsFolderPath + "/" + MockDataAssetFileName;
    internal const string LegacyMockDataSettingsFolderPath = SettingsRootPath + "/Emulator";
    internal const string LegacyMockDataAssetPath = LegacyMockDataSettingsFolderPath + "/EmulatorMockData.asset";

    private static EmulatorMockDataAsset _activeMockDataAsset;

    /// <summary>
    /// Raised when the active persisted mock data asset reference changes.
    /// </summary>
    public static event EventHandler ActiveMockDataAssetChanged;

    /// <summary>
    /// Gets the active persisted mock data asset for the emulator, if one is available.
    /// </summary>
    public static EmulatorMockDataAsset ActiveMockDataAsset => _activeMockDataAsset ??= TryLoadActiveMockDataAsset();

    /// <summary>
    /// Sets the active persisted mock data asset for the emulator.
    /// </summary>
    /// <param name="asset">The asset that should be treated as active.</param>
    public static void SetActiveMockDataAsset(EmulatorMockDataAsset asset)
    {
        if(ReferenceEquals(_activeMockDataAsset, asset))
        {
            return;
        }

        _activeMockDataAsset = asset;
        ActiveMockDataAssetChanged?.Invoke(null, EventArgs.Empty);
    }

    private static EmulatorMockDataAsset TryLoadActiveMockDataAsset()
    {
#if UNITY_EDITOR
        return TryLoadAssetAtPath(DefaultMockDataAssetPath) ?? TryLoadAssetAtPath(LegacyMockDataAssetPath);
#else
        return null;
#endif
    }

#if UNITY_EDITOR
    private static EmulatorMockDataAsset TryLoadAssetAtPath(string path)
    {
        Type assetDatabaseType = Type.GetType("UnityEditor.AssetDatabase, UnityEditor");
        MethodInfo loadAssetAtPathMethod = assetDatabaseType?.GetMethod("LoadAssetAtPath", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(Type) }, null);
        return loadAssetAtPathMethod?.Invoke(null, new object[] { path, typeof(EmulatorMockDataAsset) }) as EmulatorMockDataAsset;
    }
#endif
}
}
