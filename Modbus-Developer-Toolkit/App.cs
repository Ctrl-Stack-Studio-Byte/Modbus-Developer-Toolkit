using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator {
  internal class App {
    static void Main(string[] args) {
      SimulatorEngine SE = new SimulatorEngine("127.0.0.1",50200);
      SE.start();

      SE.waitForExitCommand();

      SE.stop();

    }
  }
}
