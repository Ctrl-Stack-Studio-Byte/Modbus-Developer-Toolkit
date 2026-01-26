using FluentModbus;
using ModbusSimulator.Models;
using ModbusSimulator.Services.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSimulator.Services {
  /// <summary>
  /// Core engine responsible for managing the Modbus server lifecycle and background simulation data flow.
  /// </summary>
  public class SimulatorEngine {
    private readonly AppConfig _appConfig;
    private readonly ModbusTcpServer _MTS;
    private readonly DataGenerator _generator;
    // A thread-safe dictionary to map signal type names to their respective strategies
    private readonly Dictionary<string, ISignalStrategy> _strategies;
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
      // Initialize the strategy library
      _strategies = new Dictionary<string, ISignalStrategy> {
            { "Sine", new SineStrategy() },
            { "Ramp", new RampStrategy() }
        };
      //Console.WriteLine($"[Debug] Total channels loaded: {_appConfig.Channels.Count}");
    }

    /// <summary>
    /// Starts the Modbus TCP server and begins the asynchronous data simulation loop.
    /// </summary>
    public void start() {
      // Set the flag to true before starting the background task.
      _isRunning = true;

      // Establish the network endpoint using configured IP and Port.
      _MTS.Start(new IPEndPoint(IPAddress.Parse(_appConfig.HostAddress), _appConfig.HostPort));
      //Console.WriteLine($"Modbus Server Started on {_appConfig.HostAddress}:{_appConfig.HostPort}...");

      // Display Configuration once
      this.PrintSystemHeader();

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
            //buffer[0] = _generator.getNoisySineValue(250,50,60, _globalStep, _appConfig.NoiseRange);
            // Channel 1: Ideal Sine Wave (Reference)
            //buffer[1] = _generator.getSineWaveValue(250,50,60, _globalStep);
            // Channel 1: Linear Ramp (e.g., Water Level 0 to 1000, increment by 5 per second)
            //buffer[2] = _generator.getRampValue(0, 1000, 100, _globalStep);

            /*
            // Data-Driven Update: Iterate through all configured channels
            // This loop handles N channels without needing any code changes
            foreach(var channel in _appConfig.Channels) {
              short value = 0;

              // Strategy Pattern: Select the algorithm based on the SignalType defined in JSON
              if(channel.SignalType == "Sine") {
                // Calculate sine wave value with noise using the global synchronized clock
                value = _generator.getNoisySineValue(
                    channel.BaseValue,
                    channel.Amplitude,
                    channel.Period,
                    _globalStep,
                    channel.NoiseRange // Injecting the channel-specific noise range
                );
              } else if(channel.SignalType == "Ramp") {
                // Calculate sawtooth ramp value based on Min, Max, and StepSize
                value = _generator.getRampValue(
                    channel.Min,
                    channel.Max,
                    channel.StepSize,
                    _globalStep
                );
              }

              // Write the calculated value to the specific Modbus address defined in config.
              // This decouples the logic from hard-coded array indices.
              buffer[channel.Address] = value;
              // Also write the calculated value to the config.
              channel.CurrentValue = value;
            }
            */

            // Refresh all channel data and synchronize with the Modbus register map.
            this.UpdateSignals(buffer);

            // 3. Monitoring: Output synchronization status to the console
            if(_appConfig.IsLoggingEnabled) {
              //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Updated {_appConfig.Channels.Count} channels at Step: {_globalStep}");

              // Use string.Join to create a clean single-line output for all channels
              // Log real-time telemetry data with minimal overhead
              // CurrentValue / 10.0 assumes the raw data is stored as an integer (deciscale)
              var displayInfo = string.Join(" | ", _appConfig.Channels.Select(c => $"{c.Name}: {c.CurrentValue / 10.0}"));
              Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {displayInfo} | Step: {_globalStep}");


            }

            /*
            // Output for monitoring
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd-HH:mm:ss}] | " +
              $"IdealTemperature: {buffer[1] / 10.0}°C | " +
              $"NoisyTemperature: {buffer[0] / 10.0}°C | " +
              $"Level: {buffer[2] / 10.0}%" +
              $"Count: {_globalStep}.");
            */

            /// <summary>
            /// Prepares and logs telemetry data by packaging buffer values and the global step.
            /// </summary>
            /// <remarks>
            /// This snippet captures the current state of real-time buffers and synchronizes 
            /// them with the simulation timer (_globalStep) for persistent storage.
            /// </remarks>
            // Persistent Logging
            // Packages real-time data and the sync clock for CSV storage.
            //double[] values = { buffer[0], buffer[1], buffer[2], _globalStep };
            //_generator.saveToCSV(values, _appConfig.LogFileName, _appConfig.IsLoggingEnabled);

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

    /// <summary>
    /// Prints a one-time summary of the channel configurations and their physical boundaries.
    /// This helps operators verify the system setup before data streaming begins.
    /// </summary>
    private void PrintSystemHeader() {
      Console.ForegroundColor = ConsoleColor.Cyan;

      //System Status
      Console.WriteLine("\n" + new string('=', 60));
      Console.WriteLine($"[STATUS] Server: {_appConfig.HostAddress}:{_appConfig.HostPort} | Channels: {_appConfig.Channels.Count}");
      Console.WriteLine($"[TIME  ] {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
      Console.WriteLine(new string('=', 60));

      Console.WriteLine("\n" + new string('=', 60));
      Console.WriteLine($"SYSTEM STARTUP - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
      Console.WriteLine(new string('=', 60));

      foreach(var ch in _appConfig.Channels) {
        // Aligning columns for better readability in the console
        // DynamicRangeDisplay shows the pre-configured physical boundaries
        Console.WriteLine($"{ch.Name,-15} | Addr: {ch.Address,-5} | {ch.DynamicRangeDisplay}");
      }

      Console.WriteLine(new string('=', 60) + "\n");
      Console.ResetColor();
    }

    private void UpdateSignals(Span<short> buffer) {
      // The high-level loop: just tells the engine to process each channel
      foreach(var channel in _appConfig.Channels) {
        ProcessSingleChannel(channel,buffer);
      }
    }
    /// <summary>
    /// Handles the logic for a single register channel.
    /// Separating this makes the code easier to test and maintain.
    /// </summary>
    private void ProcessSingleChannel(RegisterChannel rc, Span<short> buffer) {
      // Attempt to retrieve the calculation strategy
      if(_strategies.TryGetValue(rc.SignalType, out var strategy)) {
        // 1. Calculate the new value
        rc.CurrentValue = strategy.Calculate(rc, _globalStep, _generator);

        // 2. Update the Modbus data buffer
        buffer[rc.Address] = rc.CurrentValue;
      } else {
        // Fallback or logging if signal type is unknown
        Console.WriteLine($"Signal type '{rc.SignalType}' is not supported.");
      }
    }


  }
}
