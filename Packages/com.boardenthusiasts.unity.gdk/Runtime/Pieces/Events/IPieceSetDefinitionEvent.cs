namespace BoardGDK.Pieces.Events
{
/// <summary>
/// Interface for events related to piece set definitions (e.g. when a piece set definition is changed).
/// </summary>
public interface IPieceSetDefinitionEvent
{
    /// <summary>
    /// The <see cref="IPieceSetDefinition"/> involved.
    /// </summary>
    public IPieceSetDefinition Definition { get; set; }
}
}
