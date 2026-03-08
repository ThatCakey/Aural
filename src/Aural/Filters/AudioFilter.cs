namespace Aural.Filters;

/// <summary>
/// Base class for all audio filters. All filters are disabled by default (zero performance cost).
/// </summary>
public abstract class AudioFilter
{
    /// <summary>
    /// Whether this filter is active. Default: false (no computation).
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Create a deep copy of this filter for immutable playback.
    /// </summary>
    public abstract AudioFilter Clone();
}
