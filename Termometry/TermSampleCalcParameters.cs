using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;

namespace Termometry
{
    public class TermSampleCalcParameters
    {
        // Конфиг
        private ConfigParameters Config;

        // Максимальная температура (необходим расчет относительно даты)
        private double MaxTemperature;
        // Глубина максимальной температуры (необходим расчет относительно даты)
        private double DepthMaxTemp;
        // Глубина скважины
        private double MaxDepthBoreHole;
        // Температура стабилизации
        private double TemperatureStabilization;
        // Глубина стабилизации температуры
        private double DepthStabilization;
        // Температура на поверхности
        private double AirTemperature;
        // Тип аномалии (Обычный график, есть талая часть, есть мерзлая часть)
        private TypeAnomaly Anomaly;
        // Если есть аномалия, задать старт аномалии
        private double StartDepthAnomaly;
        // Если есть аномалия, задать конец аномалии
        private double EndDepthAnomaly;
        // Если есть аномалия, задать температуру аномалии
        private double TemperatureAnomaly;
        // Температуры в Январе (начало круга)
        private double TemperatureJanuary;
        // Температуры в Июне (середина круга)
        private double TemperatureJune;

        // Лист опорных температур
        public List<double> ControlTemperatures;
        // Лист опорных глубин
        public List<double> ControlDepths;
        
        // Лист всех глубин графика
        public List<double> Depths;
        // Лист всех температур графика
        public List<double> Temperatures;

        private TermSampleOrganisation OrganisationParameters;

        private TermCircle Circle;

        public TermSampleCalcParameters(TermSampleOrganisation OrganisationParameters, ConfigParameters Config, 
                          double TemperatureStabilization, double MaxDepthBoreHole,
                          double DepthStabilization, double AirTemperature,
                          double TemperatureJanuary, double TemperatureJune,
                          TypeAnomaly TypeAnomaly, double StartDepthAnomaly = 0f,
                          double EndDepthAnomaly = 0f, double TemperatureAnomaly = 0f)
        {
            this.OrganisationParameters = OrganisationParameters;
            this.Config = Config;

            this.MaxDepthBoreHole = MaxDepthBoreHole;
            this.TemperatureStabilization = TemperatureStabilization;
            this.DepthStabilization = DepthStabilization;
            this.AirTemperature = AirTemperature;
            this.Anomaly = TypeAnomaly;
            this.StartDepthAnomaly = StartDepthAnomaly;
            this.EndDepthAnomaly = EndDepthAnomaly;
            this.TemperatureAnomaly = TemperatureAnomaly;

            this.TemperatureJanuary = TemperatureJanuary;
            this.TemperatureJune = TemperatureJune;

            // Получить график глубин
            this.Depths = CalcDepths();

            // Ищем максимальную температуру
            this.Circle = new TermCircle(OrganisationParameters.CountDays, TemperatureJanuary, TemperatureJune);
            this.MaxTemperature = Circle.MaxTerm(OrganisationParameters.DayOfYear);
            // Провермяем на аномалии и меняем максимальную температуру, если они есть
            int IndexMaxDepth = (Anomaly == TypeAnomaly.Anomaly) ? Depths.IndexOf(EndDepthAnomaly) + 1 : new Random().Next(1, 5);
            this.DepthMaxTemp = Depths[IndexMaxDepth];
            // this.MaxTemperature = (Anomaly == TypeAnomaly.Frozen || Anomaly == TypeAnomaly.Thawed) ? CalcMaxTemperature(Depths[IndexMaxDepth - 1], MaxTemperature) : MaxTemperature;

            // Получить опорные точки глубин и температур
            this.ControlTemperatures = CalcControlTemp();
            this.ControlDepths = CalcControlDepth();

            // Получить график температур
            this.Temperatures = CalcTemperatures();
        }

        /// <summary>
        /// Посчитать глубины графика
        /// </summary>
        /// <returns></returns>
        private List<double> CalcDepths()
        {
            List<double> CalcDepthsList = new List<double>() { };

            double CurrentDepth = 0;

            double Multiple = 0.5f;

            while (CurrentDepth <= MaxDepthBoreHole+2)
            {
                if (CurrentDepth + 0.5f > MaxDepthBoreHole)
                {
                    if (CurrentDepth != MaxDepthBoreHole) CalcDepthsList.Add(MaxDepthBoreHole);
                    break;
                }
                else
                {
                    CalcDepthsList.Add(CurrentDepth);
                }
                if (CurrentDepth < 5) Multiple = 0.5f;
                if (CurrentDepth >= 5 && CurrentDepth < 10) Multiple = 1.0f;
                if (CurrentDepth >= 10) Multiple = 2.0f;

                CurrentDepth += Multiple;
            }
            return CalcDepthsList;
        }

        /// <summary>
        /// Если есть аномалия, посчитать нижележащую максимальную температуру
        /// </summary>
        /// <param name="NormallyDepth"></param>
        /// <param name="MaxTemp"></param>
        /// <returns></returns>
        private double CalcMaxTemperature(double NormallyDepth, double MaxTemp)
        {
            throw new System.Exception("Not work!");
            // Не работает 
            // Не работает 
            // Не работает 
            // Не работает 
            // Не работает 
            double[] x = new double[] { MaxTemp, TemperatureStabilization };
            double[] y = new double[] { DepthMaxTemp, DepthStabilization };

            var Regress = SimpleRegression.Fit(y, x);
            return Regress.A * NormallyDepth + Regress.B;
        }

        /// <summary>
        /// Контрольные точки глубины
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<double> CalcControlDepth()
        {
            List<double> CalcDepth = null;
            switch (Anomaly)
            {
                case TypeAnomaly.Normaly:
                    CalcDepth = new List<double>() { 0f, DepthMaxTemp, (DepthMaxTemp + DepthStabilization) / 2f, DepthStabilization, MaxDepthBoreHole };
                    break;
                case TypeAnomaly.Anomaly:
                    CalcDepth = new List<double>() { 0f, StartDepthAnomaly, (StartDepthAnomaly + EndDepthAnomaly) / 2f, EndDepthAnomaly, DepthMaxTemp, (DepthMaxTemp + DepthStabilization) / 2f, DepthStabilization, MaxDepthBoreHole };
                    break;
                default:
                    throw new Exception("Не задан тип графика!");
            }
            return CalcDepth;
        }

        /// <summary>
        /// Контрольные точки температуры
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<double> CalcControlTemp()
        {
            List<double> CalcTemp = null;
            switch (Anomaly)
            {
                case TypeAnomaly.Normaly:
                    CalcTemp = new List<double>() { AirTemperature, MaxTemperature, (MaxTemperature + TemperatureStabilization) / 2f, TemperatureStabilization, TemperatureStabilization };
                    break;
                case TypeAnomaly.Anomaly:
                    CalcTemp = new List<double>() { AirTemperature, 0f, TemperatureAnomaly, 0f, MaxTemperature, (MaxTemperature + TemperatureStabilization) / 2f, TemperatureStabilization, TemperatureStabilization };
                    break;
                default:
                    throw new Exception("Не задан тип графика!");
            }
            return CalcTemp;
        }

        /// <summary>
        /// Рассчитать температуры
        /// </summary>
        /// <returns></returns>
        private List<double> CalcTemperatures()
        {
            IInterpolation InterpData = Interpolate.CubicSplineMonotone(ControlDepths, ControlTemperatures);

            var x = new DenseVector(Depths.Count);
            var y = new DenseVector(x.Count);

            for (int i = 0; i < x.Count; i++)
            {
                x[i] = ((double)ControlDepths[^1] * (double)i) / (double)(x.Count - 1);
                y[i] = InterpData.Interpolate(x[i]);
                double randInt = new Random().Next(-(int)(Config.NoisePercents * 100f), (int)(Config.NoisePercents * 100f)) / 10000f;
                double random = (double)(1f + randInt);
                y[i] = y[i] * random;
            }
            return y.ToList();
        }
    }
}
