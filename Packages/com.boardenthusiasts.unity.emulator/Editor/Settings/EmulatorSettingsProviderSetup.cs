using System.Collections.Generic;

using BE.Emulator.Data;

using UnityEditor;

namespace BE.Emulator.Editor.Settings
{
internal static class EmulatorSettingsProviderSetup
{
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EmulatorProjectSettingsAssetUtility.GetOrCreateMockDataAsset();
    }

    [SettingsProviderGroup]
    private static SettingsProvider[] CreateSettingsProviders()
    {
        EmulatorMockDataAsset mockDataAsset = EmulatorProjectSettingsAssetUtility.GetOrCreateMockDataAsset();

        List<string> keywords = new()
        {
            "Board",
            "Emulator",
            "Application",
            "Profiles",
            "Session",
            "Save Data",
            "Storage"
        };

        return new SettingsProvider[]
        {
            EmulatorSettingsProvider.CreateApplication(mockDataAsset, keywords),
            EmulatorSettingsProvider.CreateProfiles(mockDataAsset, keywords),
            EmulatorSettingsProvider.CreateSession(mockDataAsset, keywords),
            EmulatorSettingsProvider.CreateSaveData(mockDataAsset, keywords),
            EmulatorSettingsProvider.CreateStorage(mockDataAsset, keywords)
        };
    }
}
}
