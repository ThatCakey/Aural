# Aural

**Audio playback library for .NET** — Play audio files with a single line of code using cross-platform OpenAL support.

[Aural is on NuGet](https://www.nuget.org/packages/Aural)

## Features

- **Simple API**: Play any audio file with `Player.Play(path, volume, loop)`
- **Cross-platform**: Works on Windows, macOS, and Linux automatically
- **Format support**: WAV, OGG Vorbis, MP3
- **Playback control**: Pause, resume, stop, volume adjustment, seeking
- **Zero setup**: No initialization required—just start playing
- **Intuitive tokens**: Control-based API via PlaybackToken objects
- **Real-time filters**: Low-pass, high-pass, reverb, and pitch shift (opt-in for zero overhead)
- **OpenAL backend**: Built on OpenTK.Audio.OpenAL for reliable, cross-platform audio

## Installation

```bash
dotnet add package Aural
```

## Quick Start

```csharp
using Aural;

// Play an audio file
var token = Player.Play("path/to/song.mp3", 0.7f, loop: true);

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

## Audio Effects

Apply real-time filters to audio playback. All filters are disabled by default (zero performance cost). Enable only the filters you need.

### Supported Filters

- **Low-Pass**: Removes high frequencies (~0.5ms overhead)
  - Parameters: `Frequency` (20-20000 Hz), `Resonance` (0.5-5.0 Q)
  - Use case: Warm tone, remove noise

- **High-Pass**: Removes low frequencies (~0.5ms overhead)
  - Parameters: `Frequency` (20-2000 Hz), `Resonance` (0.5-5.0 Q)
  - Use case: Remove rumble, sub-bass

- **Reverb**: Schroeder reverberator for room ambience (~3-5ms overhead)
  - Parameters: `RoomSize` (0.0-1.0), `DampFactor` (0.0-1.0), `Wet` (0.0-1.0), `Dry` (0.0-1.0)
  - Use case: Space, ambience

- **Pitch Shift**: STFT phase vocoder for transposition (~10-15ms overhead, **50-200ms latency**)
  - Parameters: `SemiTones` (-12 to +12), `WindowSize` (512, 1024, 2048)
  - Use case: Transpose music, karaoke, tuning adjustment

### Filter Example

```csharp
using Aural;
using Aural.Filters;

// Create audio effect with filters
var effect = new AudioEffect
{
    LowPass = new LowPassFilter
    {
        Enabled = true,
        Frequency = 8000f,    // Remove frequencies above 8kHz
        Resonance = 1.0f
    },
    Reverb = new ReverbFilter
    {
        Enabled = true,
        RoomSize = 0.7f,       // Larger room
        DampFactor = 0.5f,     // Dampen high frequencies
        Wet = 0.3f,            // 30% reverb mix
        Dry = 0.7f             // 70% original signal
    }
};

// Apply effects during playback
var token = Player.Play("song.mp3", volume: 0.8f, effect: effect);

// Query which filters are active
var activeEffects = token.GetActiveFilters();
Console.WriteLine($"Low-Pass: {activeEffects.LowPass.Enabled}");
Console.WriteLine($"Reverb: {activeEffects.Reverb.Enabled}");

Thread.Sleep(5000);
token.End();
```

### Important Notes

- **Immutability**: The effect object is **deep-cloned** when playback starts. Modifications after `Play()` have no effect.
- **Opt-In**: Filters are disabled by default. Enable only what you need.
- **Pitch Shift Latency**: Expect 50-200ms latency with pitch shift enabled. Use smaller `WindowSize` for lower latency, larger for better quality.
- **Cross-Platform**: All filters work identically on Windows, macOS, and Linux.

## API Reference

### `Player.Play(filePath, volume, loop, effect)`

Plays an audio file and returns a PlaybackToken for control.

**Parameters:**
- `filePath` (string): Path to audio file (WAV, MP3, OGG)
- `volume` (float, default: 1.0): Volume level 0.0 (silent) to 1.0 (full)
- `loop` (bool, default: false): Whether to loop the audio
- `effect` (AudioEffect?, default: null): Optional audio effects/filters to apply

**Returns:** `PlaybackToken?` - Control token, or null if playback failed

### `PlaybackToken` Methods

- `Pause()` - Pause playback
- `Play()` - Resume playback
- `End()` - Stop and release resources
- `SetVolume(float)` - Adjust volume (0.0-1.0)
- `Seek(float)` - Seek to position in seconds
- `GetActiveFilters()` - Get reconstructed AudioEffect showing which filters are active
- `IsPlaying` (property) - Check if audio is playing
- `CurrentPosition` (property) - Get current playback position in seconds
- `Duration` (property) - Get total audio duration in seconds

### `Player.Dispose()`

Disposes all active playback sessions. Call on application shutdown.

## Supported Formats

- WAV (.wav)
- OGG (.ogg)
- MPEG (.mp3)

## Platform Support

- Windows
- macOS
- Linux

## License

MIT
