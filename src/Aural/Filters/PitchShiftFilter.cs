namespace Aural.Filters;

/// <summary>
/// Pitch shift using STFT phase vocoder. Transposes audio without changing duration.
/// Cost: ~10-15ms per buffer when enabled. 50-200ms latency. USER MUST OPT-IN (default disabled).
/// Formants shift with pitch (acceptable for most music playback).
/// Window sizes: 512 (low latency, lower quality), 1024 (balanced), 2048 (high quality, high latency).
/// </summary>
public class PitchShiftFilter : AudioFilter
{
    /// <summary>Pitch shift in semitones (-12 to +12). Default: 0 (no shift).</summary>
    public float SemiTones { get; set; } = 0f;

    /// <summary>FFT window size (512, 1024, or 2048). Larger = better quality, more latency. Default: 1024.</summary>
    public int WindowSize { get; set; } = 1024;

    // Internal state for phase vocoder
    internal float[] _window = new float[0];
    internal float[] _inputBuffer = new float[0];
    internal float[] _outputBuffer = new float[0];
    internal float[] _lastPhase = new float[0];
    internal float[] _sumPhase = new float[0];
    internal int _inputIndex = 0;
    internal int _outputIndex = 0;
    internal int _hopSize = 0;

    /// <summary>Create a deep copy of this filter.</summary>
    public override AudioFilter Clone() => new PitchShiftFilter
    {
        Enabled = this.Enabled,
        SemiTones = this.SemiTones,
        WindowSize = this.WindowSize
    };

    /// <summary>Initialize buffers and window function (called once per playback).</summary>
    internal void Initialize(int sampleRate)
    {
        _hopSize = WindowSize / 4;
        
        _inputBuffer = new float[WindowSize * 2];
        _outputBuffer = new float[WindowSize * 2];
        _lastPhase = new float[WindowSize / 2];
        _sumPhase = new float[WindowSize / 2];
        _inputIndex = 0;
        _outputIndex = 0;

        // Hann window
        _window = new float[WindowSize];
        for (int i = 0; i < WindowSize; i++)
        {
            _window[i] = 0.5f * (1f - MathF.Cos(2f * MathF.PI * i / (WindowSize - 1)));
        }

        Array.Clear(_lastPhase, 0, _lastPhase.Length);
        Array.Clear(_sumPhase, 0, _sumPhase.Length);
    }
}
