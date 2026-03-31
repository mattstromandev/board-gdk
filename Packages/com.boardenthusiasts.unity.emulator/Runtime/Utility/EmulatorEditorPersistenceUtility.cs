using System;
using System.Reflection;

using UnityEngine;

namespace BE.Emulator.Utility
{
/// <summary>
/// Editor-aware helpers for persisting emulator assets when running inside the Unity editor.
/// </summary>
internal static class EmulatorEditorPersistenceUtility
{
    /// <summary>
    /// Saves the provided asset when the Unity editor asset database is available.
    /// </summary>
    /// <param name="asset">The asset to save.</param>
    public static void SaveAssetIfPossible(UnityEngine.Object asset)
    {
        if(asset == null)
        {
            return;
        }

        Type editorUtilityType = Type.GetType("UnityEditor.EditorUtility, UnityEditor");
        MethodInfo setDirtyMethod = editorUtilityType?.GetMethod("SetDirty", BindingFlags.Public | BindingFlags.Static);
        setDirtyMethod?.Invoke(null, new object[] { asset });

        Type assetDatabaseType = Type.GetType("UnityEditor.AssetDatabase, UnityEditor");
        MethodInfo saveAssetsMethod = assetDatabaseType?.GetMethod("SaveAssets", BindingFlags.Public | BindingFlags.Static);
        saveAssetsMethod?.Invoke(null, null);
    }
}
}
