using SmartStore.Core.Configuration;

namespace SmartStore.ConfigurableExportTest.Settings
{
    public class ConfigurableExportSettings : ISettings
    {
        public string MyFirstSetting { get; set; }


        public int PictureId { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }




    }
}