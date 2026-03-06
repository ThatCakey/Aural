# Aural

**Tested and working audio playback library for .NET** — Play audio files with a single line of code using cross-platform OpenAL support.

## Features

- **Simple API**: Play any audio file with `Player.Play(path, volume, loop)`
- **Cross-platform**: Works on Windows, macOS, and Linux automatically
- **Format support**: WAV, OGG Vorbis
- **Playback control**: Pause, resume, stop, volume adjustment, seeking
- **Zero setup**: No initialization required—just start playing
- **Intuitive tokens**: Control-based API via PlaybackToken objects
- **OpenAL backend**: Built on OpenTK.Audio.OpenAL for reliable, cross-platform audio

## Installation

```bash
dotnet add package Aural
```

## Quick Start

```csharp
using Aural;

// Play an audio file
var token = Player.Play("path/to/song.ogg", 0.7f, loop: true);

// Let it play for 10 seconds
Thread.Sleep(10000);

// Pause playback
token.Pause();
Thread.Sleep(10000);

// Resume playback
token.Play();
Thread.Sleep(10000);

// Stop and release resources
token.End();

// Optional: cleanup all active playbacks on shutdown
Player.Dispose();
```

## API Reference

### `Player.Play(filePath, volume, loop)`

Plays an audio file and returns a PlaybackToken for control.

**Parameters:**
- `filePath` (string): Path to audio file (WAV, MP3, OGG)
- `volume` (float, default: 1.0): Volume level 0.0 (silent) to 1.0 (full)
- `loop` (bool, default: false): Whether to loop the audio

**Returns:** `PlaybackToken?` - Control token, or null if playback failed

### `PlaybackToken` Methods

- `Pause()` - Pause playback
- `Play()` - Resume playback
- `End()` - Stop and release resources
- `SetVolume(float)` - Adjust volume (0.0-1.0)
- `Seek(float)` - Seek to position in seconds
- `IsPlaying` (property) - heck if audio is playing
- `CurrentPosition` (property) - Get current playback position in seconds

### `Player.Dispose()`

Disposes all active playback sessions. Call on application shutdown.

## Supported Formats
C
- WAV (.wav)
- OGG (.ogg)

## Platform Support

- Windows
- macOS
- Linux

## License

MIT
