using FluentModbus;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSimulator {
  internal class SimulatorEngine {

    private string _hostAddress;
    private ushort _hostPort;
    private ModbusTcpServer _MTS;
    private DataGenerator _generator;
    // Use 'volatile' to ensure the most up-to-date value is read across threads.
    private volatile bool _isRunning;

    private short _noiseRange;
    private double _globalStep;
    public SimulatorEngine(string address, ushort port) { 
      _hostAddress = address;
      _hostPort = port;
      _MTS = new ModbusTcpServer();
      _generator = new DataGenerator();
      _noiseRange = 3;
      _globalStep = 0;
    }

    public void start() {
      // Set the flag to true before starting the background task.
      _isRunning = true;
      _MTS.Start(new IPEndPoint(IPAddress.Parse(_hostAddress),_hostPort));
      Console.WriteLine("Modbus Server Started on Port 50200...");

      short[] registers = new short[10];

      // Background Task: Simulates the internal logic of a PLC/Sensor.
      // Periodically updates the Modbus holding registers with simulated data.
      Task.Run(() => {
        Console.WriteLine("--- Background Loop Started ---");
        // The loop will exit gracefully when _isRunning is set to false.
        while(_isRunning) {
          try {
          
            var buffer = _MTS.GetHoldingRegisters();

            // Modbus registers only support 16-bit integers (short).
            // To simulate float values (e.g., 20.5°C), we scale the base value by 10.
            // Base value 200 represents 20.0°C. 
            // The client/master must divide the received value by 10.0 to restore the decimal.
            //buffer[0] = _generator.getNextValue(200, 30);

            // Inside the while loop of SimulatorEngine:
            // Base: 25.0°C (250), Amplitude: ±5.0°C (50), Period: 60 seconds
            // Both channels now use the exact same _globalStep
            // Channel 0: Noisy Sine Wave (Temperature)
            buffer[0] = _generator.getNoisySineValue(250,50,60, _globalStep,_noiseRange);
            // Channel 1: Ideal Sine Wave (Reference)
            buffer[1] = _generator.getSineWaveValue(250,50,60, _globalStep);
            // Channel 1: Linear Ramp (e.g., Water Level 0 to 1000, increment by 5 per second)
            buffer[2] = _generator.getRampValue(0, 1000, 100, _globalStep);

            // Output for monitoring
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd-HH:mm:ss}] | " +
              $"IdealTemperature: {buffer[1] / 10.0}°C | " +
              $"NoisyTemperature: {buffer[0] / 10.0}°C | " +
              $"Level: {buffer[2] / 10.0}%" +
              $"Count: {_globalStep}.");

            /// <summary>
            /// Prepares and logs telemetry data by packaging buffer values and the global step.
            /// </summary>
            /// <remarks>
            /// This snippet captures the current state of real-time buffers and synchronizes 
            /// them with the simulation timer (_globalStep) for persistent storage.
            /// </remarks>
            // 1. Initialize a data package with a fixed size to hold buffer values and the step counter
            double[] values = new double[4];
            // 2. Map real-time buffer values to the telemetry array
            values[0] = buffer[0];
            values[1] = buffer[1];
            values[2] = buffer[2];
            // 3. Attach the global simulation step for time-series synchronization
            values[3] = _globalStep;
            // 4. Delegate the persistent storage task to the data generator's logging utility
            _generator.saveToCSV(values);

            // Increment the global clock once per loop iteration
            _globalStep++;
          } catch(Exception ex) {
            Console.WriteLine($"Loop Error: {ex.Message}");
          }


          Thread.Sleep(1000);
        }
        // This line executes after the while loop finishes.
        Console.WriteLine("--- Background Loop Gracefully Stopped. ---");

      });
      

    }


    public void stop() {
      // Signal the background loop to stop.
      _isRunning = false;
      // Brief delay to allow the loop to finish its current iteration.
      Thread.Sleep(200);
      _MTS.Stop();
      Console.WriteLine("--Modbus Server Stopped.--");
    }



    /// <summary>
    /// Blocks the main thread to keep the application alive 
    /// while the background simulation loop continues to run.
    /// </summary>
    public void waitForExitCommand() {

      Console.WriteLine("Press 'q' to Exit:");
      while(Console.ReadLine() != "q") {
        // Keep waiting until 'q' is entered.
      }

    }



  }
}
