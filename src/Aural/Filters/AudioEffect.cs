namespace Aural.Filters;

/// <summary>
/// Container for all audio effects. Pass to Player.Play() to apply real-time filters during playback.
/// The effect object is deep-cloned when playback starts; modifications after Play() have no effect.
/// All filters are disabled by default (zero performance cost).
/// </summary>
public class AudioEffect
{
    /// <summary>Low-pass filter (removes high frequencies). Default: disabled.</summary>
    public LowPassFilter LowPass { get; set; } = new LowPassFilter();

    /// <summary>High-pass filter (removes low frequencies). Default: disabled.</summary>
    public HighPassFilter HighPass { get; set; } = new HighPassFilter();

    /// <summary>Reverb filter (Schroeder reverberator). Default: disabled.</summary>
    public ReverbFilter Reverb { get; set; } = new ReverbFilter();

    /// <summary>Pitch shift filter (STFT phase vocoder). Default: disabled. WARNING: ~50-200ms latency.</summary>
    public PitchShiftFilter PitchShift { get; set; } = new PitchShiftFilter();

    /// <summary>Create a deep copy of all filters for immutable playback.</summary>
    public AudioEffect Clone() => new AudioEffect
    {
        LowPass = (LowPassFilter)this.LowPass.Clone(),
        HighPass = (HighPassFilter)this.HighPass.Clone(),
        Reverb = (ReverbFilter)this.Reverb.Clone(),
        PitchShift = (PitchShiftFilter)this.PitchShift.Clone()
    };
}
