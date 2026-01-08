using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator {
  public class DataGenerator {


    private Random _random;
    // Use a field to track time steps internally
    //private double _step;
    public DataGenerator() { 
      _random = new Random();
      //_step = 0;
    }

    /// <summary>
    /// Adds random jitter to a base value.
    /// </summary>
    /// <param name="baseValue">The original value to be jittered.</param>
    /// <param name="jitterRange">The maximum range of random variation.</param>
    /// <returns>A value with added noise.</returns>
    public short getNextRandomValue(short basic, short jitterRange) {
      // Returns a value within [baseValue - jitterRange, baseValue + jitterRange)
      return (short)(basic + _random.Next(-(jitterRange), jitterRange));

    }

    /// <summary>
    /// Generates a sine wave value based on time steps.
    /// </summary>
    /// <param name="baseValue">The center point of the wave (e.g., 200 for 20.0°C)</param>
    /// <param name="amplitude">The max variation from the center (e.g., 50 for ±5.0°C)</param>
    /// <param name="periodSteps">How many steps for one full cycle (e.g., 60 for 60 seconds)</param>
    /// <returns>A scaled integer for Modbus register</returns>
    public short getSineWaveValue(double baseValue, double amplitude, double periodSteps, double currentStep) {
      // Math.Sin takes radians. 2 * PI is one full circle.
      double radians = ( 2 * Math.PI / periodSteps ) * currentStep;
      double value = baseValue + ( amplitude * Math.Sin(radians) );

      //_step++; // Increment step for the next call
      return (short)Math.Round(value);
    }

    /// <summary>
    /// Generates a sine wave value with integrated noise.
    /// </summary>
    public short getNoisySineValue(double baseValue, double amplitude, double periodSteps,double currentStep, short noiseRange) {
    
      return (this.getNextRandomValue(this.getSineWaveValue(baseValue, amplitude, periodSteps,currentStep), noiseRange));
    
      
    }

    /// <summary>
    /// Generates a sawtooth ramp (linear increase) value.
    /// </summary>
    /// <param name="min">The starting minimum value.</param>
    /// <param name="max">The peak value before resetting.</param>
    /// <param name="stepSize">The amount to increase per global step.</param>
    /// <param name="currentStep">The external synchronized clock (e.g., _globalStep).</param>
    /// <returns>A value within [min, max). Returns min if parameters are invalid.</returns>
    public short getRampValue(double min,double max, double stepSize, double currentStep) {
      // 1. Validation: Ensure range is positive to avoid DivisionByZeroException
      if(max <= min) {
        Console.WriteLine($"[Error] DataGenerator: Max ({max}) must be greater than Min ({min}).");
        return (short)min;
      } else {

        double range = max - min;

        // 2. Calculate "Total Distance" without worrying about long-term overflow (using double)
        double totalDistance = stepSize * currentStep;

        // 3. Apply Modulo to create the cyclic "Sawtooth" effect
        // result will be between 0 and (range - 1)
        double result = ( totalDistance % range ) + min;

        return (short)Math.Round(result);
      }
    }

    /// <summary>
    /// Appends an array of telemetry data to a local CSV file with a timestamp.
    /// </summary>
    /// <param name="values">An array of double values representing different sensor signals or data points.</param>
    /// <remarks>
    /// This method uses a StreamWriter in append mode. 
    /// It leverages string.Join for flexibility across different array lengths.
    /// </remarks>
    public void saveToCSV(double[] values) {
      string filePath = "Log.csv";

      using(StreamWriter sw = new StreamWriter(filePath,true)) {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        sw.WriteLine($"{timestamp},{string.Join(",",values)}");
      }
    
    }



  }
}
