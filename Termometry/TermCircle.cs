using System.Data;

namespace Termometry
{
    /// <summary>
    /// Создать круг максимальных температур для всех дней в году
    /// </summary>
    public class TermCircle
    {
        // Углы (от 0 до 180)
        private List<double> Angles;
        // Максимальный отскок температуры в году
        private List<double> MaxTemperatures;
        // КОличество дней в году
        private int CountDays;
        // Температура января
        private double TemperatureJanuary;
        // Температура июня
        private double TemperatureJune;
        // Получить максимальную температуру дня
        public double MaxTerm(int day) => MaxTemperatures[day];

        public TermCircle(int CountDays, double TemperatureJanuary, double TemperatureJune)
        {
            this.CountDays = CountDays;
            this.TemperatureJanuary = TemperatureJanuary;
            this.TemperatureJune = TemperatureJune;

            // Получаем углы
            Angles = new List<double>();
            FillAngles(ref Angles);
            // Получаем температуры
            MaxTemperatures = new List<double>();
            FillMaxTemperatures(ref MaxTemperatures);
        }

        /// <summary>
        /// Заполнение углов с заданным шагом до 180 градусов
        /// </summary>
        /// <param name="Angles"></param>
        private void FillAngles(ref List<double> Angles)
        {
            for (int i = 0; i < CountDays; i++)
            {
                Angles.Add((180f / (float)CountDays) * i);
            }
        }

        /// <summary>
        /// Заполнение максимальных отскоков температур в каждом месяце
        /// </summary>
        /// <param name="MaxTemperatures"></param>
        private void FillMaxTemperatures(ref List<double> MaxTemperatures)
        {
            double Y_0 = TemperatureJanuary;
            double R_Y = Math.Abs(TemperatureJanuary) + Math.Abs(TemperatureJune);

            double X_0 = 6f;
            double R_X = 6f;

            MaxTemperatures = Angles.Select(x => Y_0 + R_Y * Math.Sin(x * Math.PI / 180f)).ToList();
        }
    }
}
