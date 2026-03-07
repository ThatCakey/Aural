using System.Diagnostics;

namespace Aural;

/// <summary>
/// Main public API for audio playback. Use this static class to play audio files.
/// Example: var token = Player.Play("song.ogg", 0.7f, true);
/// </summary>
public static class Player
{
    private static readonly HashSet<AudioPlayer> ActivePlayers = new();
    private const int MaxSimultaneousPlaybacks = 255;

    /// <summary>
    /// Plays an audio file and returns a token for playback control.
    /// Supports WAV, OGG Vorbis, and MP3 formats. Works on Windows, Mac, and Linux automatically.
    /// </summary>
    /// <param name="filePath">Path to the audio file (WAV, OGG, or MP3).</param>
    /// <param name="volume">Volume level from 0.0 (silent) to 1.0 (full volume). Default is 1.0.</param>
    /// <param name="loop">Whether to loop the audio file. Default is false.</param>
    /// <returns>
    /// A PlaybackToken for controlling playback (pause, resume, stop, volume, seek),
    /// or null if playback could not be started.
    /// </returns>
    /// <example>
    /// <code>
    /// var token = Aural.Player.Play("music.ogg", 0.7f, true);
    /// if (token != null)
    /// {
    ///     Thread.Sleep(5000);
    ///     token.Pause();
    ///     Thread.Sleep(2000);
    ///     token.Play();
    ///     Thread.Sleep(5000);
    ///     token.End();
    /// }
    /// </code>
    /// </example>
    public static PlaybackToken? Play(string filePath, float volume = 1.0f, bool loop = false)
    {
        try
        {
            // Enforce simultaneous playback limit
            if (ActivePlayers.Count >= MaxSimultaneousPlaybacks)
            {
                string msg = $"[Aural] Cannot start playback: maximum simultaneous playbacks ({MaxSimultaneousPlaybacks}) reached.";
                Debug.WriteLine(msg);
                Console.Error.WriteLine(msg);
                return null;
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(filePath))
            {
                string msg = "[Aural] Play failed: file path cannot be null or empty.";
                Debug.WriteLine(msg);
                Console.Error.WriteLine(msg);
                return null;
            }

            // Resolve to absolute path for safety
            string absolutePath = Path.GetFullPath(filePath);

            // Create audio player
            var audioPlayer = new AudioPlayer(absolutePath, volume, loop);

            // Track active player
            ActivePlayers.Add(audioPlayer);

            // Create token with cleanup callback
            var token = new PlaybackToken(audioPlayer);

            // Register for cleanup when playback ends
            _ = Task.Run(async () =>
            {
                while (!audioPlayer.PlaybackFinished)
                {
                    await Task.Delay(100);
                }

                ActivePlayers.Remove(audioPlayer);
                audioPlayer.Dispose();
            });

            return token;
        }
        catch (Exception ex)
        {
            string msg = $"[Aural] Play failed: {ex.Message}";
            Debug.WriteLine(msg);
            Console.Error.WriteLine(msg);
            Console.Error.WriteLine($"[Aural] Exception type: {ex.GetType().Name}");
            Console.Error.WriteLine($"[Aural] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
                Console.Error.WriteLine($"[Aural] Inner exception: {ex.InnerException.Message}");
            
            // Also write to file for debugging
            try
            {
                File.AppendAllText("/tmp/aural_error.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR: {msg}\n");
                File.AppendAllText("/tmp/aural_error.log", $"  Type: {ex.GetType().Name}\n");
                File.AppendAllText("/tmp/aural_error.log", $"  Stack: {ex.StackTrace}\n\n");
            }
            catch { }
            
            Console.Out.Flush();
            Console.Error.Flush();
            return null;
        }
    }

    /// <summary>
    /// Disposes all active playback sessions and releases audio resources.
    /// Call this when shutting down your application.
    /// </summary>
    public static void Dispose()
    {
        try
        {
            var playersToDispose = ActivePlayers.ToList();
            foreach (var player in playersToDispose)
            {
                try
                {
                    player.Dispose();
                    ActivePlayers.Remove(player);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Aural] Error disposing player: {ex.Message}");
                }
            }

            ActivePlayers.Clear();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Error during global disposal: {ex.Message}");
        }
    }
}
