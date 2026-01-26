using ModbusSimulator.Models;

namespace ModbusSimulator.Services.Strategies {
  /// <summary>
  /// Defines the standard structure for all signal generation algorithms.
  /// </summary>
  public interface ISignalStrategy {
    /// <summary>
    /// Calculates the register value based on the specific strategy logic.
    /// </summary>
    /// <param name="rh">The channel configuration data.</param>
    /// <param name="gStep">The current global time step.</param>
    /// <param name="dg">The data generator utility for math functions.</param>
    /// <returns>A short integer value for the Modbus register.</returns>
    short Calculate(RegisterChannel rh, double gStep, DataGenerator dg);
  }
}
