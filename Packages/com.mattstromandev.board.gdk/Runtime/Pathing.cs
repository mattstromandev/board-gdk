using System.IO;

namespace BoardGDK
{
    /// <summary>
    /// Houses pre-defined strings for use with directories in the Unity project.
    /// </summary>
    public static class Pathing
    {
        /// <summary>
        /// The extension of an asset file.
        /// </summary>
        public const string AssetExtension = ".asset";
        
        /// <summary>
        /// The name of the Assets folder in the Unity project.
        /// </summary>
        public const string Assets = nameof(Assets);
        
        /// <summary>
        /// A standard name for a folder containing generated items.
        /// </summary>
        public const string Generated = "_" + nameof(Generated);
        
        /// <summary>
        /// The path to a standard generated assets folder. 
        /// </summary>
        public static readonly string GeneratedAssets = Path.Combine(Assets, Generated);
    }
}
