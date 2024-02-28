using OfficeOpenXml;
using OfficeOpenXml.Export.ToDataTable;
using System.Data;

namespace Termometry
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var Config = new ConfigParameters(NoisePercents: 4, pathToData: @"term.xlsm", doImages: true);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var options = ToDataTableOptions.Create();
                options.EmptyRowStrategy = EmptyRowsStrategy.Ignore;
                options.AlwaysAllowNull = true;

                DataTable datatable;
                List<string> Headers = new List<string>();
                using (var package = new ExcelPackage(path: Config.PathToData))
                {
                    var sheet = package.Workbook.Worksheets["List"];
                    datatable = sheet.Cells[$"A1:P2000"].ToDataTable(options);
                    foreach (DataColumn c in datatable.Columns)
                    {
                        Headers.Add(c.ColumnName);
                    }
                }

                // Если файл есть удаляем его и создае новый через копирования основного
                if (File.Exists(@"Done.xlsm")) File.Delete(@"Done.xlsm");
                File.Copy(@"term.xlsm", @"Done.xlsm");

                using (var package = new ExcelPackage(@"Done.xlsm"))
                {
                    foreach (DataRow r in datatable.Rows)
                    {
                        var Data = new ExcelRowData(Headers, r);
                        var Term = new TermSample(Data, Config);
                        var exc = new ExcelCreateTermList(package, Term.OrganisationParameters, Term.GetDepths(), Term.GetTemps());
                        Console.WriteLine($"{Term.OrganisationParameters.NameBoreHole} -- is complete.");
                    }
                }

                Console.WriteLine("Done");
                Console.ReadLine();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    // Скважина
    public class TermSample
    {
        // Организационные параметры скважины
        public TermSampleOrganisation OrganisationParameters;
        // Расчетные параметры скважины
        private TermSampleCalcParameters CalcParameters;

        // Получить глубины
        public List<double> GetDepths() => CalcParameters.Depths;
        // Получить температуры
        public List<double> GetTemps() => CalcParameters.Temperatures;

        public TermSample(ExcelRowData data, ConfigParameters config)
        {
            OrganisationParameters = new TermSampleOrganisation(NameBoreHole: data.NameBoreHole, ObjectName: data.ObjectName,
                TermoCosa: data.TermoCosa, 
                DateBoreHole: data.DateBoreHole, DateTermometry: data.DateTermometry, AirTemperature: data.AirTemperature);
            CalcParameters = new TermSampleCalcParameters(OrganisationParameters: OrganisationParameters, Config: config, 
                TemperatureStabilization: data.TemperatureStabilization, MaxDepthBoreHole: data.MaxDepthBoreHole, 
                DepthStabilization: data.DepthStabilization, AirTemperature: data.AirTemperature, 
                TemperatureJanuary: data.TemperatureJanuary, TemperatureJune: data.TemperatureJune, 
                TypeAnomaly: data.Anomaly, StartDepthAnomaly: data.StartDepthAnomaly, 
                EndDepthAnomaly: data.EndDepthAnomaly, TemperatureAnomaly: data.TemperatureAnomaly);
        }
    }
}
