using System.Data;

namespace Termometry
{
    /// <summary>
    /// Соединение с экселем и заполнение данными
    /// </summary>
    public class ExcelRowData
    {
        // Данные
        public DataRow Data;
        // Заголовки данных
        public List<string> Headers;

        // Имя скважины
        public string NameBoreHole;
        // Имя скважины
        public string ObjectName;
        // Термокоса
        public string TermoCosa;
        // Дата бурения
        public DateOnly DateBoreHole;
        // Дата замера термометрии
        public DateOnly DateTermometry;
        // Глубина скважины
        public double MaxDepthBoreHole;
        // Температура стабилизации
        public double TemperatureStabilization;
        // Глубина стабилизации температуры
        public double DepthStabilization;
        // Температура на поверхности
        public double AirTemperature;
        // Тип аномалии (Обычный график, есть талая часть, есть мерзлая часть)
        public TypeAnomaly Anomaly;
        // Если есть аномалия, задать старт аномалии
        public double StartDepthAnomaly;
        // Если есть аномалия, задать конец аномалии
        public double EndDepthAnomaly;
        // Если есть аномалия, задать температуру аномалии
        public double TemperatureAnomaly;
        // Температуры в Январе (начало круга)
        public double TemperatureJanuary;
        // Температуры в Июне (середина круга)
        public double TemperatureJune;

        public ExcelRowData(List<string> Headers, DataRow Data)
        {
            this.Headers = Headers;
            this.Data = Data;
            Initialization();
        }

        private void Initialization()
        {
            string HeaderElement;
            object Temp;

            // Инициализация имени скважины
            HeaderElement = "Скважина";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try 
            { 
                NameBoreHole = Temp.ToString();
                if (NameBoreHole == "") throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}");
            }
            catch { throw new Exception($"Не задано имя скважины в строке {Data[0]}"); }

            // Инициализация Объект
            HeaderElement = "Объект";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { ObjectName = Temp.ToString(); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            // Инициализация термокосы
            HeaderElement = "Термокоса";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { TermoCosa = Temp.ToString(); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            // Инициализация Дата бурения
            HeaderElement = "Дата бурения";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { DateBoreHole = DateOnly.Parse(Temp.ToString()); }
            catch
            {
                try
                {
                    DateBoreHole = DateOnly.FromDateTime(DateTime.Parse(Temp.ToString()));
                }
                catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }
            }

            // Инициализация Дата термометрии
            HeaderElement = "Дата термометрии";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { DateTermometry = DateOnly.Parse(Temp.ToString()); }
            catch
            {
                try
                {
                    DateTermometry = DateOnly.FromDateTime(DateTime.Parse(Temp.ToString()));
                }
                catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }
            }

            // Инициализация Глубина скважины
            HeaderElement = "Глубина скважины";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { MaxDepthBoreHole = Double.Parse(Temp.ToString()); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            // Инициализация Конечная температура
            HeaderElement = "Конечная температура";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { TemperatureStabilization = Double.Parse(Temp.ToString()); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            // Инициализация Глубина конечной температуры
            HeaderElement = "Глубина конечной температуры";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { DepthStabilization = Double.Parse(Temp.ToString()); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            // Инициализация Температура воздуха
            HeaderElement = "Температура воздуха";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { AirTemperature = Double.Parse(Temp.ToString()); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            // Инициализация Талые от/Мерзлые от
            HeaderElement = "Талые от/ Мерзлые от";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { StartDepthAnomaly = Double.Parse(Temp.ToString()); }
            catch { StartDepthAnomaly = 0; }

            // Инициализация Талые до/ Мерзлые до
            HeaderElement = "Талые до/ Мерзлые до";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { EndDepthAnomaly = Double.Parse(Temp.ToString()); }
            catch { EndDepthAnomaly = 0; }

            // Инициализация Отскок температуры при аномалии
            HeaderElement = "Отскок температуры при аномалии";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { TemperatureAnomaly = Double.Parse(Temp.ToString()); }
            catch { TemperatureAnomaly=0; }

            // Инициализация Температуры 6 месяца
            HeaderElement = "Температуры 6 месяца";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { TemperatureJune = Double.Parse(Temp.ToString()); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            // Инициализация Температуры 12 месяца
            HeaderElement = "Температуры 12 месяца";
            Temp = Data[Headers.IndexOf(HeaderElement)];
            try { TemperatureJanuary = Double.Parse(Temp.ToString()); }
            catch { throw new Exception($"Не задана {HeaderElement} в строке {Data[0]}"); }

            var AnomalyTemp = Data[Headers.IndexOf("Тип графика")];
            if (AnomalyTemp.ToString() == "Аномалия" && StartDepthAnomaly != 0 && EndDepthAnomaly != 0)
                Anomaly = TypeAnomaly.Anomaly;
            else
                Anomaly = TypeAnomaly.Normaly;
        }
    }
}
