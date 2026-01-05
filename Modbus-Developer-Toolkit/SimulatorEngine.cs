using FluentModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSimulator {
  internal class SimulatorEngine {

    private string _hostAddress;
    private ushort _hostPort;
    private ModbusTcpServer _MTS;
    private DataGenerator _generator;

    public SimulatorEngine(string address, ushort port) { 
      _hostAddress = address;
      _hostPort = port;
      _MTS = new ModbusTcpServer();
      _generator = new DataGenerator();
    }

    public void start() {
      _MTS.Start(new IPEndPoint(IPAddress.Parse(_hostAddress),_hostPort));
      Console.WriteLine("Modbus Server Started on Port 50200...");

      short[] registers = new short[10];

      // Background Task: Simulates the internal logic of a PLC/Sensor.
      // Periodically updates the Modbus holding registers with simulated data.
      Task.Run(() => {
        Console.WriteLine("--- Background Loop Started ---");
        while(true) {
          var buffer = _MTS.GetHoldingRegisters();

          // Modbus registers only support 16-bit integers (short).
          // To simulate float values (e.g., 20.5°C), we scale the base value by 10.
          // Base value 200 represents 20.0°C. 
          // The client/master must divide the received value by 10.0 to restore the decimal.
          buffer[0] = _generator.getNextValue(200, 30);
          buffer[1]++;
          Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Temperature: {buffer[0] / 10.0}°C | Count: {buffer[1]}.");
          Thread.Sleep(1000);
        }


      });
      

    }


    public void stop() { _MTS.Stop(); }



    /// <summary>
    /// Blocks the main thread to keep the application alive 
    /// while the background simulation loop continues to run.
    /// </summary>
    public void waitForExitCommand() {

      Console.WriteLine("Press 'q' to Exit:");
      while(Console.ReadLine() != "q") {
      }
    
    }



  }
}
