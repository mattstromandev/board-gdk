using BoardGDK.Pieces;

namespace BoardGDK
{
    /// <summary>
    /// Houses pre-defined strings for use with Unity menus.
    /// </summary>
    public static class Menus
    {
        /// <summary>
        /// The standard menu separator used in Unity menus.
        /// </summary>
        public const string MenuSeparator = "/";
        
        /// <summary>
        /// The standard menu root used by the <see cref="BoardGDK"/> package.
        /// </summary>
        public const string MenuRoot = nameof(BoardGDK) + MenuSeparator;
        
        /// <summary>
        /// The menu path to use when adding menu items to the general Assets context menu.
        /// </summary>
        public const string AssetsContextMenuRoot = Pathing.Assets + MenuSeparator + MenuRoot;
        
        /// <summary>
        /// The menu path to use when adding menu items related to the <see cref="PieceSystem"/>.
        /// </summary>
        public const string PiecesMenuRoot = MenuRoot + nameof(Pieces) + MenuSeparator;
    }
}
