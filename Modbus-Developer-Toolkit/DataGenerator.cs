using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator {
  public class DataGenerator {


    private Random _random;
    // Use a field to track time steps internally
    private double _step;
    public DataGenerator() { 
      _random = new Random();
      _step = 0;
    }


    public short getNextValue(short basic, short jitterRange) {

      return (short)(basic + _random.Next(-(jitterRange), jitterRange));

    }

    /// <summary>
    /// Generates a sine wave value based on time steps.
    /// </summary>
    /// <param name="baseValue">The center point of the wave (e.g., 200 for 20.0°C)</param>
    /// <param name="amplitude">The max variation from the center (e.g., 50 for ±5.0°C)</param>
    /// <param name="periodSteps">How many steps for one full cycle (e.g., 60 for 60 seconds)</param>
    /// <returns>A scaled integer for Modbus register</returns>
    public short getSineWaveValue(double baseValue, double amplitude, double periodSteps) {
      // Math.Sin takes radians. 2 * PI is one full circle.
      double radians = ( 2 * Math.PI / periodSteps ) * _step;
      double value = baseValue + ( amplitude * Math.Sin(radians) );

      _step++; // Increment step for the next call
      return (short)Math.Round(value);
    }

  }
}
