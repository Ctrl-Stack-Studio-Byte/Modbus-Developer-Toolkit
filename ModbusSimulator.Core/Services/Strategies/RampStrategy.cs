using ModbusSimulator.Models;

namespace ModbusSimulator.Services.Strategies {
  /// <summary>
  /// Strategy for generating linear Ramp (Sawtooth) signals.
  /// </summary>
  public class RampStrategy : ISignalStrategy {
    public short Calculate(RegisterChannel rc, double gStep, DataGenerator dg) =>
        // Linear progression from Min to Max based on step size
        dg.getRampValue(rc.Min, rc.Max, rc.StepSize, gStep);
  }
}
