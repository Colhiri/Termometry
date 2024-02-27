using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;

namespace Termometry
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var OrganTest = new TermSampleOrganisation("1", "1488", new DateOnly(2020, 1, 24), new DateOnly(2020, 1, 24));
            var CalcTest = new TermSampleCalcParameters(OrganTest, -5.2f, 15f, 10f, 5f, -41f, 15f, TypeAnomaly.Normaly);

            var test = new TermSample(OrganTest, CalcTest);
            ScottPlot.Plot myPlot = new();
            myPlot.Add.Scatter(CalcTest.Depths.ToArray(), CalcTest.Temperatures.ToArray());
            myPlot.SavePng("quickstart.png", 400, 300);
        }
    }

    public enum TypeAnomaly
    {
        Frozen,
        Thawed,
        Normaly
    }

    // Скважина
    public class TermSample
    {
        // Организационные параметры скважины
        public TermSampleOrganisation OrganisationParameters;
        // Расчетные параметры скважины
        public TermSampleCalcParameters CalcParameters;

        public TermSample(TermSampleOrganisation OrganisationParameters, TermSampleCalcParameters CalcParameters)
        {
            this.OrganisationParameters = OrganisationParameters;
            this.CalcParameters = CalcParameters;
        }
    }

    /// <summary>
    /// Организационные параметры скважины
    /// </summary>
    public class TermSampleOrganisation
    {
        // Имя скважины
        public string NameBoreHole;
        // Термокоса
        public string TermoCosa;

        public int Mouth;
        public int Day;
        public int Year;
        public int DayOfYear;
        public int CountDays;

        // Дата бурения
        private DateOnly DateBoreHole;
        // Дата замера термометрии
        private DateOnly DateTermometry;

        public TermSampleOrganisation(string NameBoreHole, string TermoCosa, DateOnly DateBoreHole, DateOnly DateTermometry)
        {
            this.NameBoreHole = NameBoreHole;
            this.TermoCosa = TermoCosa;
            this.DateBoreHole = DateBoreHole;
            this.DateTermometry = DateTermometry;

            Mouth = DateTermometry.Month;
            Day = DateTermometry.Day;
            Year = DateTermometry.Year;
            DayOfYear = DateTermometry.DayOfYear;
            CountDays = DateTime.IsLeapYear(Year) ? 366 : 365;
        }
    }

    public class TermSampleCalcParameters
    {
        // Максимальная температура (необходим расчет относительно даты)
        public double MaxTemperature;
        // Глубина максимальной температуры (необходим расчет относительно даты)
        public double DepthMaxTemp;
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

        public TermSampleCalcParameters(TermSampleOrganisation OrganisationParameters, double TemperatureStabilization,
                          double MaxDepthBoreHole,
                          double DepthStabilization, double AirTemperature,
                          double TemperatureJanuary, double TemperatureJune,
                          TypeAnomaly TypeAnomaly, double StartDepthAnomaly = 0f,
                          double EndDepthAnomaly = 0f, double TemperatureAnomaly = 0f)
        {
            this.OrganisationParameters = OrganisationParameters;

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
            int IndexMaxDepth = (Anomaly == TypeAnomaly.Frozen || Anomaly == TypeAnomaly.Thawed) ? Depths.IndexOf(EndDepthAnomaly) + 1 : new Random().Next(1, 5);
            this.DepthMaxTemp = Depths[IndexMaxDepth];
            this.MaxTemperature = (Anomaly == TypeAnomaly.Frozen || Anomaly == TypeAnomaly.Thawed) ? CalcMaxTemperature(Depths[IndexMaxDepth], MaxTemperature) : MaxTemperature;

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
            double[] x = new double[] { MaxTemp, TemperatureStabilization };
            double[] y = new double[] { DepthMaxTemp, DepthStabilization };

            var Regress = SimpleRegression.Fit(x, y);
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
                case TypeAnomaly.Frozen | TypeAnomaly.Thawed:
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
                case TypeAnomaly.Frozen | TypeAnomaly.Thawed:
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

            var x = new DenseVector(50);
            var y = new DenseVector(x.Count);

            for (int i = 0; i < x.Count; i++)
            {
                x[i] = ((double)ControlDepths[^1] * (double)i) / (double)(x.Count - 1);
                y[i] = InterpData.Interpolate(x[i]);
            }
            var testc = y.ToList();

            return testc;
        }
    }

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

            /*
            IInterpolation InterpData = Interpolate.CubicSplineMonotone(Angles, MaxTemperatures);

            var x = new DenseVector(CountDays);
            var y = new DenseVector(x.Count);

            for (int i = 0; i < x.Count; i++)
            {
                x[i] = (180f * (double)i) / (double)(x.Count - 1);
                y[i] = InterpData.Interpolate(x[i]);
            }
            MaxTemperatures = x.ToList();
            */
        }
    }
}
