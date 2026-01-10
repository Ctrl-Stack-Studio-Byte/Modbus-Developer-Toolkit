using Newtonsoft.Json;
using System;
using System.IO;


namespace ModbusSimulator.Services {
  public static class ConfigHandler {
    // 1. This is the "Default" path. 
    // It's "hard-coded" as a fallback.
    private const string DefaultFileName = "config.json";

    /// <summary>
    /// Save config to the default path.
    /// </summary>
    public static void Save(AppConfig config) {
      // Call the other method with the default path
      Save(config, DefaultFileName);
    }

    /// <summary>
    /// Save config to a specific path (Flexible).
    /// </summary>
    public static void Save(AppConfig config, string path) {
      // Convert to JSON and Save to 'path'
      string json = JsonConvert.SerializeObject(config, Formatting.Indented);
      try {
        File.WriteAllText(path, json);
      } catch(Exception ex) {
        Console.WriteLine($"Error saving data to Config:{ex.Message}");
      }
      
    }
    /// <summary>
    /// Read config from the default path.
    /// </summary>
    public static AppConfig Load() {
      AppConfig config = new AppConfig();
      try {
        // Check if the file exists before reading
        if(!File.Exists(DefaultFileName)) {
          Console.WriteLine("Config file not found. Creating a default one.");
          //AppConfig defaultConfig = new AppConfig();
          Save(config); // Save the default settings to disk
          return config;
        }
        // Read text from file
        string json = File.ReadAllText(DefaultFileName);

        // Convert JSON string back to object
        config = JsonConvert.DeserializeObject<AppConfig>(json);

      } catch(Exception ex) {
        Console.WriteLine($"Error reading data from Config:{ex.Message}");
      }
      return config;
    }


  }
}
