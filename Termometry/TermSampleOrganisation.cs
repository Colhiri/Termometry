namespace Termometry
{
    /// <summary>
    /// Организационные параметры скважины
    /// </summary>
    public class TermSampleOrganisation
    {
        // Имя скважины
        public string NameBoreHole;
        // Термокоса
        public string TermoCosa;
        // Объект
        public string ObjectName;
        // Температура воздуха
        public double AirTemperature;


        public int Mouth;
        public int Day;
        public int Year;
        public int DayOfYear;
        public int CountDays;

        // Дата бурения
        public DateOnly DateBoreHole;
        // Дата замера термометрии
        public DateOnly DateTermometry;

        public TermSampleOrganisation(string NameBoreHole, string ObjectName, string TermoCosa, DateOnly DateBoreHole, DateOnly DateTermometry, double AirTemperature)
        {
            this.NameBoreHole = NameBoreHole;
            this.ObjectName = ObjectName;
            this.TermoCosa = TermoCosa;
            this.DateBoreHole = DateBoreHole;
            this.DateTermometry = DateTermometry;
            this.AirTemperature = AirTemperature;
            Mouth = DateTermometry.Month;
            Day = DateTermometry.Day;
            Year = DateTermometry.Year;
            DayOfYear = DateTermometry.DayOfYear;
            CountDays = DateTime.IsLeapYear(Year) ? 366 : 365;
        }
    }
}
