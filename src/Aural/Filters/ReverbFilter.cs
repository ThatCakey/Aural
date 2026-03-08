namespace Aural.Filters;

/// <summary>
/// Schroeder reverberator. Creates room ambience using 4 comb filters + 2 allpass filters.
/// Cost: ~3-5ms per buffer when enabled.
/// </summary>
public class ReverbFilter : AudioFilter
{
    /// <summary>Room size (0.0-1.0). Larger = longer reverb tail. Default: 0.5.</summary>
    public float RoomSize { get; set; } = 0.5f;

    /// <summary>Damp factor (0.0-1.0). Higher = dampen high frequencies. Default: 0.5.</summary>
    public float DampFactor { get; set; } = 0.5f;

    /// <summary>Wet signal mix (0.0-1.0). Reverb level. Default: 0.3.</summary>
    public float Wet { get; set; } = 0.3f;

    /// <summary>Dry signal mix (0.0-1.0). Original signal level. Default: 0.7.</summary>
    public float Dry { get; set; } = 0.7f;

    // Internal state for Schroeder reverberator (fixed buffer sizes for performance)
    private const int CombSize1 = 1116;
    private const int CombSize2 = 1188;
    private const int CombSize3 = 1277;
    private const int CombSize4 = 1356;
    private const int AllpassSize1 = 556;
    private const int AllpassSize2 = 441;

    internal float[] _combBuffer1 = new float[CombSize1];
    internal float[] _combBuffer2 = new float[CombSize2];
    internal float[] _combBuffer3 = new float[CombSize3];
    internal float[] _combBuffer4 = new float[CombSize4];
    internal float[] _allpassBuffer1 = new float[AllpassSize1];
    internal float[] _allpassBuffer2 = new float[AllpassSize2];

    internal int _combIndex1 = 0, _combIndex2 = 0, _combIndex3 = 0, _combIndex4 = 0;
    internal int _allpassIndex1 = 0, _allpassIndex2 = 0;

    internal float _combFilter1 = 0f, _combFilter2 = 0f, _combFilter3 = 0f, _combFilter4 = 0f;

    /// <summary>Create a deep copy of this filter.</summary>
    public override AudioFilter Clone() => new ReverbFilter
    {
        Enabled = this.Enabled,
        RoomSize = this.RoomSize,
        DampFactor = this.DampFactor,
        Wet = this.Wet,
        Dry = this.Dry
    };
}
