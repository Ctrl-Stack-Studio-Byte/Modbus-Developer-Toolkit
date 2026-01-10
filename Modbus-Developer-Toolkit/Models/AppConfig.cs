
namespace ModbusSimulator {
  public class AppConfig {
    /// <summary>
    /// The name of the output CSV file. Default is "Log.csv".
    /// </summary>
    public string LogFileName { get; set; } = "Log.csv";

    /// <summary>
    /// The delay between each data collection in milliseconds.
    /// </summary>
    public int SamplingIntervalMs { get; set; } = 1000;
  }
}
