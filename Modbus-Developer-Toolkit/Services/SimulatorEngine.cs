using FluentModbus;
using ModbusSimulator.Models;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSimulator.Services {
  /// <summary>
  /// Core engine responsible for managing the Modbus server lifecycle and background simulation data flow.
  /// </summary>
  internal class SimulatorEngine {
    private readonly AppConfig _appConfig;
    private readonly ModbusTcpServer _MTS;
    private readonly DataGenerator _generator;

    /// <summary>
    /// Flag to control the background simulation thread safely across different threads.
    /// </summary>
    // Use 'volatile' to ensure the most up-to-date value is read across threads.
    private volatile bool _isRunning;
    private double _globalStep;

    /// <summary>
    /// Initializes a new instance of the engine with injected configuration settings.
    /// </summary>
    /// <param name="config">The application configuration object.</param>
    public SimulatorEngine(AppConfig config) {
      _appConfig = config;
      _MTS = new ModbusTcpServer();
      _generator = new DataGenerator();
      _globalStep = 0;
    }

    /// <summary>
    /// Starts the Modbus TCP server and begins the asynchronous data simulation loop.
    /// </summary>
    public void start() {
      // Set the flag to true before starting the background task.
      _isRunning = true;

      // Establish the network endpoint using configured IP and Port.
      _MTS.Start(new IPEndPoint(IPAddress.Parse(_appConfig.HostAddress), _appConfig.HostPort));
      Console.WriteLine($"Modbus Server Started on {_appConfig.HostAddress}:{_appConfig.HostPort}...");

      // Spawn a background task to decouple simulation logic from the UI/Main thread.
      Task.Run(() => {
        Console.WriteLine("--- Background Loop Started ---");
        // The loop will exit gracefully when _isRunning is set to false.
        while(_isRunning) {
          try {
            // Access the shared Modbus register buffer.
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
            buffer[0] = _generator.getNoisySineValue(250,50,60, _globalStep, _appConfig.NoiseRange);
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
            // Persistent Logging
            // Packages real-time data and the sync clock for CSV storage.
            double[] values = { buffer[0], buffer[1], buffer[2], _globalStep };
            _generator.saveToCSV(values, _appConfig.LogFileName, _appConfig.IsLoggingEnabled);

            // Increment the global clock once per loop iteration
            _globalStep++;
          } catch(Exception ex) {
            Console.WriteLine($"Loop Error: {ex.Message}");
          }

          // Dynamic Loop Timing: Controlled by the configuration file.
          Thread.Sleep(_appConfig.SamplingIntervalMs);
        }
        // This line executes after the while loop finishes.
        Console.WriteLine("--- Background Loop Gracefully Stopped. ---");

      });
      

    }

    /// <summary>
    /// Signals the engine to stop and performs resource cleanup.
    /// </summary>
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
      while(Console.ReadLine()?.ToLower() != "q") {
        // Keep waiting until 'q' is entered.
      }

    }



  }
}
