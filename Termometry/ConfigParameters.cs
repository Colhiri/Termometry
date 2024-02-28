namespace Termometry
{
    public class ConfigParameters
    {
        // Шум для рандомности данных
        public double NoisePercents;
        // Путь к сводной
        public string PathToData;
        // Нужно ли делать изображения?
        public bool DoImages;

        public ConfigParameters(double NoisePercents, string pathToData, bool doImages)
        {
            this.NoisePercents = NoisePercents;
            PathToData = pathToData;
            DoImages = doImages;
        }
    }
}
