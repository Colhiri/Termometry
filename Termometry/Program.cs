using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;
using System.Windows;


namespace Termometry
{
    public class Program
    {
        public static void Main(string[] args)
        {

            List<double> ControlDepths = new List<double>() { 0, 1, 2, 2.5, 5, 10 };
            List<double> ControlTemperatures = new List<double>() { 9.6, 0, 5, 0, -20, -5.2 };

            IInterpolation InterpData = Interpolate.CubicSplineMonotone(ControlDepths, ControlTemperatures);

            var x = new DenseVector(20);
            var y = new DenseVector(x.Count);

            for (int i = 0; i < x.Count; i++)
            {
                x[i] = (ControlDepths[^1] * i) / (x.Count - 1);
                y[i] = InterpData.Interpolate(x[i]);
            }

            ScottPlot.Plot myPlot = new();
            myPlot.Add.Scatter(x.ToArray(), y.ToArray());
            myPlot.SavePng("quickstart.png", 400, 300);
            
            
        }
    }

    public enum TypeAnomaly
    {
        Frozen,
        Thawed,
        Normaly
    }



    public class ReadRowTermometry
    {

    }

    // Скважина
    public class TermSample
    {
        // Организационные параметры скважины
        public TermSampleOrganisation OrganisationParameters;
        // Расчетные параметры скважины
        public TermSampleGraphic CalcParameters;

        // Максимальный отскок температуры
        private double MaxTemp;
        // Температура стабилизации
        private double TempStabilization;
        // Глубина стабилизации
        private double DepthStabilization;

        // Температуры в Январе (начало круга)
        private double TemperatureJanuary;
        // Температуры в Июне (середина круга)
        private double TemperatureJune;

        // Углы (от 0 до 180)
        private List<double> Angles;
        // Максимальный отскок температуры в году
        private List<double> MaxTemperatures;

        public TermSample(TermSampleOrganisation OrganisationParameters, double TemperatureJanuary, double TemperatureJune)
        {
            this.TemperatureJanuary = TemperatureJanuary;
            this.TemperatureJune = TemperatureJune;
            this.OrganisationParameters = OrganisationParameters;

            Angles = new List<double>();
            FillAngles(ref Angles);

            MaxTemperatures = new List<double>();
            FillMaxTemperatures(ref MaxTemperatures);

            MaxTemp = MaxTemperatures[OrganisationParameters.DayOfYear];

            CalcParameters = new TermSampleGraphic(MaxTemperature: MaxTemp, TemperatureStabilization: TempS;
        }

        /// <summary>
        /// Заполнение углов с заданным шагом до 180 градусов
        /// </summary>
        /// <param name="Angles"></param>
        private void FillAngles(ref List<double> Angles)
        {
            for (int i = 0; i < OrganisationParameters.CountDays; i++) 
            {
                Angles.Add((180 / OrganisationParameters.CountDays) *  i);
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

            double X_0, R_X = 6;

            MaxTemperatures = Angles.Select(x => Y_0 + R_Y * Math.Sin(x * Math.PI / 180)).ToList();

            IInterpolation InterpData = Interpolate.CubicSplineMonotone(Angles, MaxTemperatures);

            var x = new DenseVector(OrganisationParameters.CountDays);
            var y = new DenseVector(x.Count);

            for (int i = 0; i < x.Count; i++)
            {
                x[i] = (180 * i) / (x.Count - 1);
                y[i] = InterpData.Interpolate(x[i]);
            }

            MaxTemperatures = x.ToList();
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
        public double MaxDepthBoreHoll;
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

        public TermSampleCalcParameters(double MaxTemperature, double TemperatureStabilization,
                          double DepthStabilization, double AirTemperature,
                          TypeAnomaly TypeAnomaly, double StartDepthAnomaly = 0f,
                          double EndDepthAnomaly = 0f, double TemperatureAnomaly = 0f)
        {
            this.TemperatureStabilization = TemperatureStabilization;
            this.DepthStabilization = DepthStabilization;
            this.AirTemperature = AirTemperature;
            this.Anomaly = TypeAnomaly;
            this.StartDepthAnomaly = StartDepthAnomaly;
            this.EndDepthAnomaly = EndDepthAnomaly;
            this.TemperatureAnomaly = TemperatureAnomaly;
        }
    }

    /// <summary>
    /// Расчетные параметры скважины
    /// </summary>
    public class TermSampleGraphic
    {
        // 
        private TermSampleCalcParameters CalcParameters;
        // Лист опорных температур
        private List<double> ControlTemperatures;
        // Лист опорных глубин
        private List<double> ControlDepths;

        // Лист всех глубин графика
        public List<double> Depths;
        // Лист всех температур графика
        public List<double> Temperatures;

        public TermSampleGraphic(TermSampleCalcParameters CalcParameters)
        {
            this.CalcParameters = CalcParameters;

            this.ControlTemperatures = CalcControlTemp();
            this.ControlDepths = CalcControlDepth();
            this.Depths = CalcDepths();
            this.Temperatures = CalcTemperatures();

            int IndexMaxDepth = (CalcParameters.Anomaly == TypeAnomaly.Frozen || CalcParameters.Anomaly == TypeAnomaly.Thawed) ? Depths.IndexOf(CalcParameters.EndDepthAnomaly) + 1 : new Random().Next(1, 5);
            CalcParameters.DepthMaxTemp = Depths[IndexMaxDepth];
            CalcParameters.MaxTemperature = (CalcParameters.Anomaly == TypeAnomaly.Frozen || CalcParameters.Anomaly == TypeAnomaly.Thawed) ? CalcMaxTemperature(Depths[IndexMaxDepth], CalcParameters.MaxTemperature) : CalcParameters.MaxTemperature;
        }

        private double CalcMaxTemperature(double NormallyDepth, double MaxTemp)
        {
            double[] x = new double[] { MaxTemp, TemperatureStabilization};
            double[] y = new double[] { DepthMaxTemp, DepthStabilization };

            var Regress = SimpleRegression.Fit(x, y);
            return Regress.A * NormallyDepth + Regress.B;
        }

        private List<double> CalcControlDepth()
        {
            List<double> CalcDepth = null;
            switch (Anomaly)
            {
                case TypeAnomaly.Normaly:
                    CalcDepth = new List<double>() { 0f, DepthMaxTemp, (DepthMaxTemp + DepthStabilization) / 2f, DepthStabilization };
                    break;
                case TypeAnomaly.Frozen | TypeAnomaly.Thawed:
                    CalcDepth = new List<double>() { 0f, StartDepthAnomaly, (StartDepthAnomaly + EndDepthAnomaly) / 2f, EndDepthAnomaly, DepthMaxTemp, (DepthMaxTemp + DepthStabilization) / 2f, DepthStabilization };
                    break;
                default:
                    throw new Exception("Не задан тип графика!");
            }
            return CalcDepth;
        }

        private List<double> CalcControlTemp()
        {
            List<double> CalcTemp = null;
            switch (Anomaly)
            {
                case TypeAnomaly.Normaly:
                    CalcTemp = new List<double>() { AirTemperature, MaxTemperature, (MaxTemperature + TemperatureStabilization) / 2f, TemperatureStabilization };
                    break;
                case TypeAnomaly.Frozen | TypeAnomaly.Thawed:
                    CalcTemp = new List<double>() { AirTemperature, 0f, TemperatureAnomaly, 0f, MaxTemperature, (MaxTemperature + TemperatureStabilization) / 2f, TemperatureStabilization };
                    break;
                default:
                    throw new Exception("Не задан тип графика!");
            }
            return CalcTemp;
        }

        private List<double> CalcDepths()
        {
            List<double> CalcDepthsList = new List<double>() { 0 };

            double CurrentDepth = 0;

            double Multiple = 0.5f;

            while (CurrentDepth <= MaxDepthBoreHoll)
            {
                if (CurrentDepth <= 5) Multiple = 0.5f;
                if (CurrentDepth > 5 && CurrentDepth <= 10) Multiple = 1.0f;
                if (CurrentDepth > 10) Multiple = 2.0f;

                if (CurrentDepth + 0.5f > MaxDepthBoreHoll) break;
                else
                {
                    CurrentDepth += Multiple;
                    CalcDepthsList.Add(CurrentDepth);
                }
            }
            return CalcDepthsList;
        }

        private List<double> CalcTemperatures()
        {
            IInterpolation InterpData = Interpolate.CubicSplineMonotone(ControlDepths, ControlTemperatures);

            var x = new DenseVector(20);
            var y = new DenseVector(x.Count);

            for (int i = 0; i < x.Count; i++)
            {
                x[i] = (ControlDepths[^1] * i) / (x.Count - 1);
                y[i] = InterpData.Interpolate(x[i]);
            }

            return x.ToList();
        }

    }
}
