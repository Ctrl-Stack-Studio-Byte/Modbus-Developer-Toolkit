using ModbusSimulator.Models;

namespace ModbusSimulator.Services.Strategies {
  /// <summary>
  /// Strategy for generating Sine wave signals with optional noise.
  /// </summary>
  public class SineStrategy : ISignalStrategy {
    public short Calculate(RegisterChannel rc, double gStep, DataGenerator dg) =>
        // Delegate the math calculation to the shared DataGenerator utility
        dg.getNoisySineValue(rc.BaseValue, rc.Amplitude, rc.Period, gStep, rc.NoiseRange);
  }
}
