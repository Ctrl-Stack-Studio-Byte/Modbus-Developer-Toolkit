
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModbusSimulator.Models {
  public class AppConfig {
    // --- Network Settings ---

    /// <summary>
    /// Local IP address the Modbus server will bind to. Use "127.0.0.1" for local testing.
    /// </summary>
    public string HostAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// TCP port for the Modbus server. Standard Modbus is 502, but 50200 is used for non-admin testing.
    /// </summary>
    public ushort HostPort { get; set; } = 50200;

    // --- Simulation Core Settings ---

    /// <summary>
    /// Execution frequency of the background simulation loop in milliseconds.
    /// </summary>
    public int SamplingIntervalMs { get; set; } = 1000;

    /// <summary>
    /// The range of random fluctuation (jitter) added to the simulated signals.
    /// </summary>
    //public short NoiseRange { get; set; } = 3;

    // --- Data Persistence (Logging) ---

    /// <summary>
    /// Enables or disables CSV data logging to the local disk.
    /// </summary>
    public bool IsLoggingEnabled { get; set; } = true;

    /// <summary>
    /// Destination filename for telemetry data. Example: "Simulation_Results.csv".
    /// </summary>
    public string LogFileName { get; set; } = "Log.csv";

    /// <summary>
    /// A list of configured channels to be simulated.
    /// </summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<RegisterChannel> Channels { get; set; } = new List<RegisterChannel>() {
      new RegisterChannel { Name = "Sine_Sample", Address = 0, SignalType = "Sine", BaseValue = 250, Amplitude = 50, Period = 60, NoiseRange = 0 },
      new RegisterChannel { Name = "Ramp_Sample", Address = 1, SignalType = "Ramp", Min = 0, Max = 1000, StepSize = 100 }
    };
  }
}
