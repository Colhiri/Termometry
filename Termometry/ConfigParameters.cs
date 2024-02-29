﻿using System.Text.Json;

namespace Termometry
{
    public class ConfigParameters
    {
        // Шум для рандомности данных
        public double NoisePercents { get; set; }
        // Путь к сводной
        public string PathToData { get; set; }
        public bool Percents { get; set; }
        public double RandomTempMin;
        public double RandomTempMax;

        private string pathToConfig = $"Config.json";

        public ConfigParameters()
        {
            
            if (!File.Exists(pathToConfig))
            {
                Console.WriteLine("Файл конфигурации не найден. Создаю новый.");
                using (FileStream fs = new FileStream(pathToConfig, FileMode.OpenOrCreate))
                {
                    ConfigParameters conf = new ConfigParameters(5, @"term.xlsm", false, -0.5, 0.5);
                    JsonSerializer.Serialize<ConfigParameters>(fs, conf);
                    Console.WriteLine("Config create.");
                }
            }
        }

        private ConfigParameters(double NoisePercents, string PathToData, bool Percents, 
            double RandomTempMin, double RandomTempMax)
        {
            this.NoisePercents = NoisePercents;
            this.PathToData = PathToData;
        }

        public ConfigParameters GetConfig()
        {
            if (File.Exists(pathToConfig))
            {
                using (FileStream fs = new FileStream(pathToConfig, FileMode.OpenOrCreate))
                {
                    ConfigParameters? conf = JsonSerializer.Deserialize<ConfigParameters>(fs);
                    Console.WriteLine($"Config load.");
                    this.NoisePercents = conf.NoisePercents;
                    this.PathToData = conf.PathToData;
                }
            }

            return this;
        }
    }
}
