using System.Diagnostics;

namespace Aural;

/// <summary>
/// Represents a playback session returned by Player.Play().
/// Use this token to control audio playback (pause, resume, stop, volume, seek).
/// </summary>
public class PlaybackToken
{
    private readonly AudioPlayer _audioPlayer;

    internal PlaybackToken(AudioPlayer audioPlayer)
    {
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
    }

    /// <summary>
    /// Pauses the current playback. Call Play() to resume.
    /// </summary>
    public void Pause()
    {
        try
        {
            _audioPlayer.Pause();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Error pausing playback: {ex.Message}");
        }
    }

    /// <summary>
    /// Resumes playback from pause or plays from current position.
    /// </summary>
    public void Play()
    {
        try
        {
            _audioPlayer.Play();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Error playing: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops playback and releases resources. The token becomes unusable after this call.
    /// </summary>
    public void End()
    {
        try
        {
            _audioPlayer.Stop();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Error stopping playback: {ex.Message}");
        }
    }

    /// <summary>
    /// Adjusts the volume for this playback session.
    /// </summary>
    /// <param name="volume">Volume level from 0.0 (silent) to 1.0 (full volume). Values outside this range will be clamped.</param>
    public void SetVolume(float volume)
    {
        try
        {
            volume = Math.Clamp(volume, 0.0f, 1.0f);
            _audioPlayer.SetVolume(volume);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Error setting volume: {ex.Message}");
        }
    }

    /// <summary>
    /// Seeks to a specific position in the audio file.
    /// </summary>
    /// <param name="seconds">The position to seek to (in seconds). Must be within valid playback duration.</param>
    public void Seek(float seconds)
    {
        try
        {
            if (seconds < 0)
            {
                Debug.WriteLine("[Aural] Seek position cannot be negative; clamping to 0.");
                seconds = 0;
            }

            _audioPlayer.Seek(seconds);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Error seeking to {seconds}s: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a value indicating whether audio is currently playing.
    /// </summary>
    public bool IsPlaying => _audioPlayer.IsPlaying;

    /// <summary>
    /// Gets the current playback position in seconds.
    /// </summary>
    public float CurrentPosition => _audioPlayer.CurrentPosition;

    /// <summary>
    /// Gets the total duration of the audio file in seconds.
    /// </summary>
    public float Duration => _audioPlayer.Duration;
}
