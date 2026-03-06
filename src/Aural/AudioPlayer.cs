using System.Diagnostics;
using OpenTK.Audio.OpenAL;
using NVorbis;

namespace Aural;

/// <summary>
/// Internal audio playback engine using Silk.NET.OpenAL for cross-platform support.
/// Handles audio file loading, decoding, and playback management.
/// </summary>
internal class AudioPlayer : IDisposable
{
    private static ALDevice _device;
    private static ALContext _context;
    private static bool _initialized;
    private static readonly object _initLock = new();

    private int _source;
    private int _buffer;
    private float _volume = 1.0f;
    private bool _isPlaying;
    private bool _disposed;    private bool _playbackFinished;  // Track if playback naturally ended vs was paused
    private readonly string _filePath;
    private readonly bool _shouldLoop;
    private byte[]? _audioData;
    private int _sampleRate;
    private int _channels;
    private int _totalSamples;

    public bool IsPlaying => _isPlaying;

    public bool PlaybackFinished => _playbackFinished;

    public float CurrentPosition
    {
        get
        {
            if (_sampleRate <= 0 || _source == 0)
                return 0f;

            try
            {
                AL.GetSource(_source, ALGetSourcei.SampleOffset, out int sampleOffset);
                return (float)(sampleOffset / (double)_sampleRate);
            }
            catch
            {
                return 0f;
            }
        }
    }

    public float Duration
    {
        get
        {
            if (_sampleRate <= 0)
                return 0f;
            return (float)(_totalSamples / (double)_sampleRate);
        }
    }

    public AudioPlayer(string filePath, float volume, bool shouldLoop)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _volume = Math.Clamp(volume, 0.0f, 1.0f);
        _shouldLoop = shouldLoop;

        InitializeOpenAL();
        Initialize();
    }

    private static void InitializeOpenAL()
    {
        lock (_initLock)
        {
            if (_initialized)
                return;

            try
            {
                // Open default audio device
                _device = ALC.OpenDevice(null);
                if (_device == null || _device.Handle == 0)
                    throw new InvalidOperationException("Failed to open OpenAL audio device");

                // Create context
                _context = ALC.CreateContext(_device, (int[])null);
                if (_context == null || _context.Handle == 0)
                    throw new InvalidOperationException("Failed to create OpenAL context");

                // Make context current
                ALC.MakeContextCurrent(_context);

                _initialized = true;
            }
            catch (Exception ex)
            {
                string msg = $"[Aural] OpenAL initialization failed: {ex.Message}";
                Console.Error.WriteLine(msg);
                throw;
            }
        }
    }

    private void Initialize()
    {
        try
        {
            // Validate file exists
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Audio file not found: {_filePath}");

            // Load and decode audio file
            LoadAudioFile(_filePath);

            if (_audioData is null || _audioData.Length == 0)
                throw new InvalidOperationException("Failed to load audio data");

            // Create OpenAL buffer
            _buffer = AL.GenBuffer();
            
            if (_buffer == 0)
                throw new InvalidOperationException("Failed to create OpenAL buffer");

            // Determine format and fill buffer
            ALFormat format = _channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
            AL.BufferData(_buffer, format, _audioData, _sampleRate);

            // Create OpenAL source
            _source = AL.GenSource();
            
            if (_source == 0)
                throw new InvalidOperationException("Failed to create OpenAL source");

            AL.Source(_source, ALSourcei.Buffer, _buffer);
            AL.Source(_source, ALSourceb.Looping, _shouldLoop);
            SetVolume(_volume);

            // Start playback
            AL.SourcePlay(_source);
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            string msg = $"[Aural] Failed to initialize audio player: {ex.Message}";
            Console.Error.WriteLine(msg);
            Cleanup();
            throw;
        }
    }

    private void LoadAudioFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        try
        {
            switch (extension)
            {
                case ".wav":
                    LoadWav(filePath);
                    break;
                case ".ogg":
                    LoadOgg(filePath);
                    break;
                default:
                    throw new NotSupportedException($"Format '{extension}' not supported. Use WAV or OGG.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Aural] Failed to load '{filePath}': {ex.Message}");
            throw;
        }
    }

    private void LoadWav(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Read RIFF header
        var riffHeader = reader.ReadBytes(4);
        if (System.Text.Encoding.ASCII.GetString(riffHeader) != "RIFF")
            throw new InvalidOperationException("Invalid WAV: missing RIFF header");

        int fileSize = reader.ReadInt32();
        var waveHeader = reader.ReadBytes(4);
        if (System.Text.Encoding.ASCII.GetString(waveHeader) != "WAVE")
            throw new InvalidOperationException("Invalid WAV: missing WAVE header");

        // Find fmt and data chunks
        int channels = 0, sampleRate = 0, bitsPerSample = 0;
        long dataPos = 0;
        int dataSize = 0;

        while (stream.Position < stream.Length)
        {
            var chunkId = reader.ReadBytes(4);
            int chunkSize = reader.ReadInt32();
            long chunkEnd = stream.Position + chunkSize;

            string chunkIdStr = System.Text.Encoding.ASCII.GetString(chunkId);

            if (chunkIdStr == "fmt ")
            {
                reader.ReadInt16(); // Audio format (1 = PCM)
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // Byte rate
                reader.ReadInt16(); // Block align
                bitsPerSample = reader.ReadInt16();
            }
            else if (chunkIdStr == "data")
            {
                dataPos = stream.Position;
                dataSize = chunkSize;
                break;
            }

            stream.Position = chunkEnd;
        }

        if (channels == 0 || sampleRate == 0 || dataSize == 0)
            throw new InvalidOperationException("Invalid WAV: missing chunks");

        // Read audio data
        stream.Position = dataPos;
        _audioData = reader.ReadBytes(dataSize);
        _channels = channels;
        _sampleRate = sampleRate;
        _totalSamples = dataSize / (channels * bitsPerSample / 8);
    }

    private void LoadOgg(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var vorbis = new VorbisReader(stream);

            _sampleRate = vorbis.SampleRate;
            _channels = vorbis.Channels;

            // Read all samples
            var samples = new List<float>();
            var buffer = new float[4096];

            int samplesRead;
            while ((samplesRead = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
            {
                samples.AddRange(buffer.Take(samplesRead));
            }

            _totalSamples = samples.Count / _channels;

            // Convert float to PCM16
            _audioData = new byte[samples.Count * 2];
            for (int i = 0; i < samples.Count; i++)
            {
                float sample = Math.Clamp(samples[i], -1f, 1f);
                short pcmSample = (short)(sample * short.MaxValue);
                BitConverter.GetBytes(pcmSample).CopyTo(_audioData, i * 2);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to decode OGG: {ex.Message}", ex);
        }
    }

    public void Pause()
    {
        if (_source == 0)
            return;

        AL.SourcePause(_source);
        _isPlaying = false;
    }

    public void Play()
    {
        if (_source == 0)
        {
            return;
        }

        try
        {
            // Ensure context is current before playing
            ALC.MakeContextCurrent(_context);
            AL.SourcePlay(_source);
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Aural ERROR] Play failed: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (_source == 0)
            return;

        AL.SourceStop(_source);
        _isPlaying = false;
        _playbackFinished = true;  // Mark as finished so cleanup can proceed
        Dispose();
    }

    public void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0.0f, 1.0f);

        if (_source == 0)
            return;

        AL.Source(_source, ALSourcef.Gain, _volume);
    }

    public void Seek(float seconds)
    {
        if (_source == 0 || _sampleRate <= 0)
            return;

        try
        {
            int sampleOffset = (int)(seconds * _sampleRate);
            sampleOffset = Math.Max(0, Math.Min(sampleOffset, _totalSamples - 1));
            AL.Source(_source, ALSourcei.SampleOffset, sampleOffset);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Seek failed: {ex.Message}");
        }
    }

    private void Cleanup()
    {
        try
        {
            if (_source != 0)
            {
                AL.SourceStop(_source);
                AL.DeleteSource(_source);
            }

            if (_buffer != 0)
            {
                AL.DeleteBuffer(_buffer);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Aural] Cleanup error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Cleanup();
        _audioData = null;
        _disposed = true;
    }
}
