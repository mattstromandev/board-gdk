# Board GDK

This repository is for the development and testing of a game development kit (GDK) for use with the Board platform and its SDK. The GDK builds on top of the SDK to add useful tooling, extensions, and systems which provide improved quality of life for game developers working with Board.

## Goals

- Prioritize clean, flexible architecture so that everything is very modular and DI-ready.
- For every addition, focus first on engineer-friendly, code-only APIs so that everything can be done from code if necessary, following up with Unity editor tooling and MonoBehaviors/ScriptableObjects that expose the code-first APIs to empower game designers and artists to work with the same constructs without having to touch code as much as possible.
- Prefer generic, modular/composable "building blocks" (always backed by C# interfaces) to large, inflexible systems.
- Always prefer data-driven architecture with single-source-of-truth setup to prevent duplicated configuration.

## Review guidelines

- Check for null-safety and correct disposal patterns.
- Prefer allocation-free patterns in hot paths.
- Follow established styles and namving conventions from existing files.
- Flag any logging of PII as P0.
- In Unity: avoid heavy work on main thread; watch GC allocs.

## Referenced repositories

### Board SDK
See top-level header of same name below.

### SerializeReference Extensions

Documentation: https://github.com/mackysoft/Unity-SerializeReferenceExtensions

Utilized to expose C# interface type selection for serialize reference fields to the Unity inspector.

### Extenject (Zenject)

Documentation: https://github.com/Mathijs-Bakker/Extenject

Utilized for dependency injection.

### Rahmen

First Party (private) Repository: https://github.com/mattstromandev/Rahmen

Provides low-level systems extending Unity capabilities, such as a logging system that prevents string allocation for logs unless the logs will actually be made, an event system, and a data system.

Note: we own this, so we can make adjustments if necessary.

# Board SDK

The Board SDK is referenced in, but not included with (for legal reasons), this repository. The Board SDK is read-only.

Documentation: https://docs.dev.board.fun

## Required Using Statements

```csharp
using Board.Core;      // BoardApplication, BoardPlayer, BoardPauseScreenContext
using Board.Input;     // BoardInput, BoardContact, BoardContactType, BoardContactPhase
using Board.Session;   // BoardSession, BoardSessionPlayer
using Board.Save;      // BoardSaveGameManager, BoardSaveGameMetadata
```

Do NOT use `using Board;` — each namespace must be imported individually.

## Project Setup

Run **Board > Configure Unity Project...** to automatically configure all required settings. The wizard handles platform switching, API levels, scripting backend, and Input System setup.

Settings assets (BoardGeneralSettings, BoardInputSettings) are auto-created on SDK import.

Download Piece Set Models via **Edit > Project Settings > Board > Input Settings** → "Load Available Models".

More info: https://docs.dev.board.fun/getting-started/setup-reference

## Platform Requirements

- Unity 2022.3 LTS or later (Unity 6 supported)
- Android 13 (API 33), ARM64, IL2CPP
- Unity Input System 1.7.0+
- Board OS 1.3.8+

**Unity 6 only**: Set Application Entry Point to "Activity" (not Game Activity) in Player Settings > Android > Other Settings.

More info: https://docs.dev.board.fun/getting-started/setup-reference

## Quick Reference

### Touch Input

More info: https://docs.dev.board.fun/guides/touch-input

```csharp
// Get all active contacts (pieces and fingers)
BoardContact[] contacts = BoardInput.GetActiveContacts();

// Filter by type
BoardContact[] pieces = BoardInput.GetActiveContacts(BoardContactType.Glyph);
BoardContact[] fingers = BoardInput.GetActiveContacts(BoardContactType.Finger);
```

**BoardContact properties:**
- `contactId` - Unique identifier for this contact
- `glyphId` - Which Piece in the set (0 to N-1), or -1 for fingers
- `screenPosition` - Position in screen coordinates
- `orientation` - Rotation in radians
- `phase` - None, Began, Moved, Ended, Canceled, Stationary
- `isTouched` - Whether a finger is touching this Piece
- `type` - Glyph (Piece) or Finger

```csharp
// Check if a specific contact is still active
bool isActive = false;
foreach (var c in BoardInput.GetActiveContacts()) {
    if (c.contactId == savedContactId) { isActive = true; break; }
}
```

### Players & Sessions

More info: https://docs.dev.board.fun/guides/player-management

```csharp
// Get current players (BoardSessionPlayer extends BoardPlayer with sessionId)
BoardSessionPlayer[] players = BoardSession.players;

// Get the active profile (device owner)
BoardPlayer activeProfile = BoardSession.activeProfile;

// Present player selector to add a player (returns true if added, false if dismissed)
bool added = await BoardSession.PresentAddPlayerSelector();

// Present selector to replace a specific player
bool replaced = await BoardSession.PresentReplacePlayerSelector(existingPlayer);

// Reset session to initial state (only active profile)
BoardSession.ResetPlayers();

// Listen for player changes
BoardSession.playersChanged += OnPlayersChanged;
BoardSession.activeProfileChanged += OnActiveProfileChanged;
```

### Save Games

More info: https://docs.dev.board.fun/guides/save-games

```csharp
// Create a save
var metadataChange = new BoardSaveGameMetadataChange {
    description = "Level 5 Complete",
    playedTime = 2700,  // seconds (ulong)
    gameVersion = Application.version,
    coverImage = screenshotTexture  // optional (Texture2D, converted to 432x243 PNG)
};
BoardSaveGameMetadata saved = await BoardSaveGameManager.CreateSaveGame(saveData, metadataChange);
string saveId = saved.id;

// Load a save (automatically activates the save's players in BoardSession.players)
byte[] data = await BoardSaveGameManager.LoadSaveGame(saveId);

// Update an existing save
BoardSaveGameMetadata updated = await BoardSaveGameManager.UpdateSaveGame(saveId, newData, metadataChange);

// List saves for current app
BoardSaveGameMetadata[] saves = await BoardSaveGameManager.GetSaveGamesMetadata();

// Load cover image
Texture2D cover = await BoardSaveGameManager.LoadSaveGameCoverImage(saveId);
```

### Pause Menu

More info: https://docs.dev.board.fun/guides/pause-menu

```csharp
// Configure pause screen (call once at startup)
BoardApplication.SetPauseScreenContext(
    applicationName: "My Game",
    showSaveOptionUponExit: true  // shows save option when exiting
);

// Or use the struct-based overload
BoardApplication.SetPauseScreenContext(new BoardPauseScreenContext {
    applicationName = "My Game",
    showSaveOptionUponExit = true,
    customButtons = null,
    audioTracks = null
});

// Update specific fields without replacing everything
BoardApplication.UpdatePauseScreenContext(showSaveOptionUponExit: false);

// Listen for pause actions
BoardApplication.pauseScreenActionReceived += (action, audioTracks) => {
    switch (action) {
        case BoardPauseAction.Resume:
            // User resumed
            break;
        case BoardPauseAction.ExitGameSaved:
            // Save game, then call BoardApplication.Exit()
            break;
        case BoardPauseAction.ExitGameUnsaved:
            BoardApplication.Exit();
            break;
    }
};

// Listen for custom button presses
BoardApplication.customPauseScreenButtonPressed += (buttonId, audioTracks) => {
    // Handle custom button
};

// Show/hide profile switcher overlay
BoardApplication.ShowProfileSwitcher();
BoardApplication.HideProfileSwitcher();

// Exit the game (returns to Library)
BoardApplication.Exit();
```

## Common Patterns

### Tracking Pieces Across Frames

```csharp
private Dictionary<int, GameObject> trackedPieces = new();

void Update() {
    var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);
    var activeIds = new HashSet<int>();

    foreach (var contact in contacts) {
        activeIds.Add(contact.contactId);

        if (contact.phase == BoardContactPhase.Began) {
            // New piece placed
            var piece = Instantiate(piecePrefabs[contact.glyphId]);
            trackedPieces[contact.contactId] = piece;
        }

        if (trackedPieces.TryGetValue(contact.contactId, out var obj)) {
            // Update position/rotation
            obj.transform.position = ScreenToWorld(contact.screenPosition);
            obj.transform.rotation = Quaternion.Euler(0, 0, -contact.orientation * Mathf.Rad2Deg);
        }
    }

    // Clean up lifted pieces
    foreach (var id in trackedPieces.Keys.ToList()) {
        if (!activeIds.Contains(id)) {
            Destroy(trackedPieces[id]);
            trackedPieces.Remove(id);
        }
    }
}
```

### Detecting Piece Touch

```csharp
foreach (var contact in BoardInput.GetActiveContacts(BoardContactType.Glyph)) {
    if (contact.isTouched) {
        // Finger is touching this piece
    }
}
```

## GlyphID vs. ContactID

**Critical distinction—track by `contactId`, NOT `glyphId`.**

- **`glyphId`** - Non-unique identifier for the Piece type (0 to N-1 in your Piece Set). Multiple Pieces of the same type share the same `glyphId`.
- **`contactId`** - Unique identifier for each contact (Piece or finger). No two contacts ever share a `contactId`. Use this to track individual Pieces across frames.

## Board Input Settings

More info: https://docs.dev.board.fun/guides/touch-input

Configure via **Edit > Project Settings > Board > Input Settings**:

| Setting | Default | Description |
|---------|---------|-------------|
| Translation Smoothing | 0.5 | 0-1, higher = smoother but more lag |
| Rotation Smoothing | 0.5 | 0-1, higher = smoother but more lag |
| Persistence | 4 | Frames to keep contact without confirmation |
| Piece Set Model | — | The .tflite model file in StreamingAssets |

Settings properties are **readonly at runtime**. To use different values, create multiple BoardInputSettings assets in the Editor and switch between them:

```csharp
// Switch to a different settings asset at runtime (cancels all contacts!)
BoardInput.settings = alternateSettings;

// Read current settings
float smoothing = BoardInput.settings.translationSmoothing;
string model = BoardInput.settings.pieceSetModelFilename;
```

## Build & Deploy

More info: https://docs.dev.board.fun/getting-started/deploy

Build an APK via **File > Build Settings > Build**. Deploy to Board hardware using `bdb` (Board Developer Bridge):

```bash
# Check connection
bdb status

# Install APK
bdb install path/to/game.apk

# Launch app
bdb launch com.yourcompany.yourgame

# Stream logs
bdb logs com.yourcompany.yourgame

# Stop app
bdb stop com.yourcompany.yourgame

# List installed apps
bdb list

# Remove app
bdb remove com.yourcompany.yourgame
```

`bdb` requires Board OS 1.3.8+. Connect Board via USB-C (data cable, not charge-only).

## Important Notes

- Piece Set Models (`.tflite` files) define which Pieces Board can detect—only one set active at a time
- Changing `BoardInput.settings` at runtime cancels all active contacts 
- Switching Piece Set models causes a brief delay (no input during load)
- Always call `BoardApplication.Exit()` when exiting—don't just quit
- Session always requires at least one Profile player

## Additional Resources

- API Reference: https://docs.dev.board.fun/api/
- Simulator Guide: https://docs.dev.board.fun/guides/simulator
- Changelog: https://docs.dev.board.fun/more/changelog
