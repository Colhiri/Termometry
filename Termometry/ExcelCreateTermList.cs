using OfficeOpenXml;

namespace Termometry
{
    public class ExcelCreateTermList
    {
        private readonly ExcelPackage pack;
        private readonly TermSampleOrganisation org;
        private readonly List<double> depths;
        private readonly List<double> temps;

        private string CurrentNameSheet;
        private ExcelWorksheet CurrentWorksheet => pack.Workbook.Worksheets[CurrentNameSheet];

        public ExcelCreateTermList(ExcelPackage pack, TermSampleOrganisation org, List<double> depths, List<double> temps) 
        {
            this.pack = pack;
            this.org = org;
            this.depths = depths;
            this.temps = temps;

            CurrentNameSheet = org.NameBoreHole;
            if (CurrentNameSheet.Contains('/') || CurrentNameSheet.Contains('\\'))
            {
                CurrentNameSheet.Replace('/', '_');
                CurrentNameSheet.Replace('\\', '_');
            }

            CreateTerm();

            // var test = new TermSample(Data, Config);
            // ScottPlot.Plot myPlot = new();
            // myPlot.Add.Scatter(CalcTestNormal.Depths.ToArray(), CalcTestNormal.Temperatures.ToArray());
            // myPlot.SavePng("Normal.png", 400, 300);
        }

        private void CreateTerm()
        {
            CopySheet();
            WriteOrganisationParameters();
            WriteCalcParameters();
            pack.Save();
        }

        private void CopySheet()
        {
            try
            {
                pack.Workbook.Worksheets.Copy("skv", CurrentNameSheet);
            }
            catch
            {
                pack.Workbook.Worksheets.Delete(CurrentNameSheet);
                pack.Workbook.Worksheets.Copy("skv", CurrentNameSheet);
            }
        }

        private void WriteOrganisationParameters()
        {
            // Запись объекта
            CurrentWorksheet.Cells["A1"].Value = $"Объект: {org.ObjectName}";
            // Запись скважины
            CurrentWorksheet.Cells["A4"].Value = $"Инженерно-геологическая скважина № {org.NameBoreHole}";
            // Запись температуры воздуха
            CurrentWorksheet.Cells["D6"].Value = $"{org.AirTemperature}";
            // Запись Дата бурения (окончания)
            CurrentWorksheet.Cells["D8"].Value = $"{org.DateBoreHole.ToString("D")}";
            // Запись Термокосы
            CurrentWorksheet.Cells["D9"].Value = $"{org.TermoCosa}";
            // Запись Даты термометрии
            CurrentWorksheet.Cells["D11"].Value = $"{org.DateTermometry}";
            // Запись Даты термометрии
            CurrentWorksheet.Cells["B12"].Value = $"{org.NameBoreHole}";
            CurrentWorksheet.Cells[$"B12:B{12 + depths.Count - 1}"].Merge = true;
            CurrentWorksheet.Cells[$"B12:B{12 + depths.Count - 1}"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
        }

        private void WriteCalcParameters()
        {
            // Запись глубин
            var cellsDep = CurrentWorksheet.Cells[$"C12"];
            cellsDep.LoadFromCollection(depths);
            // CurrentWorksheet.Cells[$"С12:С{12 + depths.Count}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            // CurrentWorksheet.Cells[$"С12:С{12 + depths.Count}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            // CurrentWorksheet.Cells[$"С12:С{12 + depths.Count}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            // CurrentWorksheet.Cells[$"С12:С{12 + depths.Count}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;  
            // Запись температур
            var cells = CurrentWorksheet.Cells[$"D12"];
            cells.LoadFromCollection(temps);
            // CurrentWorksheet.Cells[$"D12:D{12 + depths.Count}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            // CurrentWorksheet.Cells[$"D12:D{12 + depths.Count}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            // CurrentWorksheet.Cells[$"D12:D{12 + depths.Count}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            // CurrentWorksheet.Cells[$"D12:D{12 + depths.Count}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

    }
}
