
using ModbusSimulator.Models;
using ModbusSimulator.Services;

namespace ModbusSimulator.UI{
  internal class App {
    static void Main(string[] args) {

      // 1. Initialize configuration by loading from a persistent JSON file.
      // The ConfigHandler will automatically provide defaults if the file is missing.
      AppConfig config = ConfigHandler.Load();

      // 2. Instantiate the simulation engine with the loaded configuration.
      // This allows the engine to adapt to different network and simulation settings without recompilation.
      SimulatorEngine SE = new SimulatorEngine(config);

      // 3. Launch the Modbus server and background simulation loop.
      SE.start();

      // 4. Block the main thread and wait for user interaction to maintain service availability.
      SE.waitForExitCommand();

      // 5. Perform a graceful shutdown: stop the server and release occupied resources.
      SE.stop();

    }
  }
}
