# omochat

omochat is a simple Nostr client for viewing and posting to Bitchat.

## Requirements

- Windows 11 22H2 (x64)
- .NET 8.0 runtime
  - The runtime is required. If it is not installed, follow the on-screen instructions shown on first run.

## Overview

`omochat.exe` lets you peek into Bitchat timelines and post messages. To post, enter your Nostr private key (starts with `nsec1...`) in the Settings screen. You can generate a new Nostr private key by clicking the **Create new key** button in Settings.

## Keyboard Shortcuts

- ESC: Open Settings
- F1 / F12: Toggle post bar
- F2: Toggle Time column
- F3: Toggle Name column
- F4: Toggle User-hash column (hidden by default)
- F5: Toggle Geohash column
- F9 / Z / double-click empty area: Toggle content word wrap
- F10 / Right-click empty area: Open Mute settings
- F11: Minimize main window

- Shift + W: Move to top event
- W / Arrow Up: Move selection up
- S / Arrow Down: Move selection down
- Shift + S: Move to bottom event
- A / Arrow Left: Open web view (also via right-click on event)
- D: Translate (opens Google Translate page in web view)
- C: Close web view
- X: Jump to selected row's Geohash

- F: Mention (inserts selected user's name into the post bar)
- H: Send "hug"
- T: Send "slap"

- R: Reply (this is a Nostr reply, not a Bitchat-specific reply)
- Ctrl + Shift + Z: Activate main form

## SSP (Shiori/SSP) Integration

You can stream timeline events to SSP (Shiori/SSP). Recommended tools for speech output: GhostSpeaker + Bouyomi-chan.

- SSP resources:
  - https://ssp.shillest.net/
  - https://keshiki.nobody.jp/

- GhostSpeaker:
  - https://github.com/apxxxxxxe/GhostSpeaker
- Bouyomi-chan:
  - https://chi.usamimi.info/Program/Application/BouyomiChan/

omochat supports sending Nostr event notifications (Nostr/0.5) for the SSP ghost "nostalk", including avatar images and Bitchat info:
- https://github.com/nikolat/nostalk

## Changelog

- 2025-09-15 — v0.1.4
  - Added feature: press `X` to jump to selected row's Geohash
  - Save Geohash column visibility state

- 2025-09-02 — v0.1.3
  - Added feature: `D` opens Google Translate for the post in web view
  - Prepared support for SSP OnNostr events (Nostr/0.5)

- 2025-09-01 — v0.1.2
  - In World view, posts outside the current Geohash are shown in gray
  - Prepared Nostr/0.5 support for SSP OnNostr events
    - Note: Reference0 remains Nostr/0.4; Reference8,9,10 now include g, n, t tags (empty if undefined)
  - Updated NuGet packages

- 2025-08-30 — v0.1.1
  - Added name mute
  - Added content mute
    - Both support partial-match and multiple entries separated by newlines

- 2025-08-30 — v0.1.0
  - Added mute feature
  - Default display order changed to ascending (enable "Descending order" in Settings for descending)
  - Added "Scroll to new post" setting
  - Added "World view" setting
  - Updated shortcut keys

- 2025-08-30 — v0.0.2
  - Title bar shows teleport tag on/off
  - Added User-hash display column
  - Added mention, hug, slap features
  - Added several client colors
    - Note: If `clients.json` exists it will not be overwritten. Delete it to add new clients.

- 2025-08-29 — v0.0.1
  - Initial release

## NuGet Packages

- CredentialManagement  
  https://www.nuget.org/packages/CredentialManagement

## Nostr Client Library

- NNostr  
  https://github.com/Kukks/NNostr  
  - A modified portion of `NNostr.Client` v0.0.49 is used.

## DirectSSTP Sending Library

- DirectSSTPTester  
  https://github.com/nikolat/DirectSSTPTester  
  - Uses `SSTPLib` v4.0.0 from the project.

## Notes

- This README is a translation and reformatting of the original `readme.txt`.
- For bugs, issues or contribution details, check the repository and open an issue or pull request.