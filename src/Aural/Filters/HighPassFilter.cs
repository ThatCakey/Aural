namespace Aural.Filters;

/// <summary>
/// High-pass IIR Biquad filter. Removes low frequencies below the cutoff.
/// Cost: ~0.5ms per buffer when enabled.
/// </summary>
public class HighPassFilter : AudioFilter
{
    /// <summary>Cutoff frequency in Hz (20-2000). Default: 20 (essentially off).</summary>
    public float Frequency { get; set; } = 20f;

    /// <summary>Resonance/Q factor (0.5-5.0). Higher = steeper cutoff. Default: 1.0.</summary>
    public float Resonance { get; set; } = 1.0f;

    // Internal filter state (not exposed to user)
    internal float _z1 = 0f;
    internal float _z2 = 0f;
    internal float _b0 = 0f, _b1 = 0f, _b2 = 0f;
    internal float _a1 = 0f, _a2 = 0f;

    /// <summary>Update filter coefficients based on frequency and resonance.</summary>
    internal void UpdateCoefficients(int sampleRate)
    {
        float w0 = 2f * MathF.PI * Frequency / sampleRate;
        float sinw0 = MathF.Sin(w0);
        float cosw0 = MathF.Cos(w0);
        float alpha = sinw0 / (2f * Resonance);

        _b0 = (1f + cosw0) / 2f;
        _b1 = -(1f + cosw0);
        _b2 = (1f + cosw0) / 2f;
        
        float a0 = 1f + alpha;
        _a1 = -2f * cosw0 / a0;
        _a2 = (1f - alpha) / a0;

        _b0 /= a0;
        _b1 /= a0;
        _b2 /= a0;
    }

    /// <summary>Create a deep copy of this filter.</summary>
    public override AudioFilter Clone() => new HighPassFilter
    {
        Enabled = this.Enabled,
        Frequency = this.Frequency,
        Resonance = this.Resonance
    };
}
