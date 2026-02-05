using System.Linq;

using Board.Input;

namespace BoardGDK.Utilities
{
/// <summary>
/// Utilities for working with <see cref="BoardInput"/>.
/// </summary>
public static class BoardInputUtil
{
    /// <summary>
    /// Retrieve the <see cref="BoardContact"/> for the given contact ID.
    /// </summary>
    /// <param name="contactID">The <see cref="BoardContact.contactId"/> of the <see cref="BoardContact"/> to retrieve.</param>
    /// <returns>The <see cref="BoardContact"/> for the given <paramref name="contactID"/>, if it is active; null otherwise.</returns>
    // TODO: It would be nice if there were an active contact cache for lookup rather than querying BoardInput each time
    public static BoardContact GetBoardContactByID(int contactID)
    {
        return BoardInput.GetActiveContacts().SingleOrDefault(contact => contact.contactId == contactID);
    }
}
}
