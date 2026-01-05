using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator {
  public class DataGenerator {


    private Random _random;

    public DataGenerator() { 
      _random = new Random();
    
    }





    public short getNextValue(short basic, short jitterRange) {

      

      return (short)(basic + _random.Next(-(jitterRange), jitterRange));
      
      


    }



  }
}
