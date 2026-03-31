using System.IO;

using BE.Emulator.Data;
using BE.Emulator.Persistence;
using BE.Emulator.Utility;

using UnityEditor;
using UnityEngine;

namespace BE.Emulator.Editor.Settings
{
internal static class EmulatorProjectSettingsAssetUtility
{
    public const string SettingsFolderPath = EmulatorProjectSettingsBridge.MockDataSettingsFolderPath;
    public const string MockDataAssetPath = EmulatorProjectSettingsBridge.DefaultMockDataAssetPath;
    public const string LegacySettingsFolderPath = EmulatorProjectSettingsBridge.LegacyMockDataSettingsFolderPath;
    public const string LegacyMockDataAssetPath = EmulatorProjectSettingsBridge.LegacyMockDataAssetPath;

    public static EmulatorMockDataAsset GetOrCreateMockDataAsset()
    {
        EmulatorMockDataAsset asset = AssetDatabase.LoadAssetAtPath<EmulatorMockDataAsset>(MockDataAssetPath) ?? TryMigrateLegacyMockDataAsset();
        bool requiresSave = false;

        if(asset == null)
        {
            EnsureFolderExists();
            asset = ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
            EnsureMockDataAssetNameMatchesFileName(asset);
            asset.Replace(EmulatorDefaults.CreateMockData());
            AssetDatabase.CreateAsset(asset, MockDataAssetPath);
            requiresSave = true;
        }
        else
        {
            requiresSave = EnsureMockDataAssetNameMatchesFileName(asset);
        }

        if(requiresSave)
        {
            AssetDatabase.SaveAssets();
        }

        EmulatorProjectSettingsBridge.SetActiveMockDataAsset(asset);
        return asset;
    }

    public static bool EnsureMockDataAssetNameMatchesFileName(EmulatorMockDataAsset asset)
    {
        if(asset == null)
        {
            return false;
        }

        string expectedName = Path.GetFileNameWithoutExtension(MockDataAssetPath);
        if(asset.name == expectedName)
        {
            return false;
        }

        asset.name = expectedName;
        EditorUtility.SetDirty(asset);
        return true;
    }

    private static void EnsureFolderExists()
    {
        if(AssetDatabase.IsValidFolder(EmulatorProjectSettingsBridge.SettingsRootPath) == false)
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        if(AssetDatabase.IsValidFolder(SettingsFolderPath) == false)
        {
            AssetDatabase.CreateFolder(EmulatorProjectSettingsBridge.SettingsRootPath, EmulatorProjectSettingsBridge.PackageDisplayName);
        }
    }

    private static EmulatorMockDataAsset TryMigrateLegacyMockDataAsset()
    {
        EmulatorMockDataAsset legacyAsset = AssetDatabase.LoadAssetAtPath<EmulatorMockDataAsset>(LegacyMockDataAssetPath);
        if(legacyAsset == null)
        {
            return null;
        }

        EnsureFolderExists();

        string moveError = AssetDatabase.MoveAsset(LegacyMockDataAssetPath, MockDataAssetPath);
        if(string.IsNullOrEmpty(moveError) == false)
        {
            Debug.LogError($"Failed to migrate emulator mock data asset from '{LegacyMockDataAssetPath}' to '{MockDataAssetPath}': {moveError}");
            return legacyAsset;
        }

        return AssetDatabase.LoadAssetAtPath<EmulatorMockDataAsset>(MockDataAssetPath);
    }
}
}
