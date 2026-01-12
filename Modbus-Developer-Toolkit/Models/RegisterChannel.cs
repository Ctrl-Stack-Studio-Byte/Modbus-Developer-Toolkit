
namespace ModbusSimulator.Models {
  /// <summary>
  /// Represents a single Modbus register configuration and its simulation behavior.
  /// </summary>
  public class RegisterChannel {
    /// <summary>
    /// Human-readable name for the channel (e.g., "Main_Tank_Temperature").
    /// </summary>
    public string Name { get; set; } = "DefaultChannelZERO";

    /// <summary>
    /// The Modbus holding register address (e.g., 0, 1, 2).
    /// </summary>
    public ushort Address { get; set; } = 256;

    /// <summary>
    /// Defines the waveform type (e.g., "Sine", "NoisySine", "Ramp").
    /// </summary>
    public string SignalType { get; set; } = "Sine";

    /// <summary>
    /// The center point of the signal (e.g., 250 for 25.0°C).
    /// </summary>
    public double BaseValue { get; set; } = 0;

    /// <summary>
    /// The maximum deviation from the base value.
    /// </summary>
    public double Amplitude { get; set; } = 5;

    /// <summary>
    /// The time (in iterations/steps) it takes to complete one full cycle.
    /// </summary>
    public double Period { get; set; } = 60;

    /// <summary>
    /// Minimum value for Ramp/Random signals.
    /// </summary>
    public double Min { get; set; } = 0;

    /// <summary>
    /// Maximum value for Ramp/Random signals.
    /// </summary>
    public double Max { get; set; } = 1000;

    /// <summary>
    /// How much the value increases per step (for Ramp).
    /// </summary>
    public double StepSize { get; set; } = 100;

    /// <summary>
    /// The latest calculated value for this channel (scaled value).
    /// </summary>
    public short CurrentValue { get; set; } = 0;
  }
}
