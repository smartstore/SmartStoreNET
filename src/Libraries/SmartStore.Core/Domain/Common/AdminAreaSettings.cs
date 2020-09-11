
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class AdminAreaSettings : ISettings
    {
        public AdminAreaSettings()
        {
            GridPageSize = 25;
            DisplayProductPictures = true;
        }

        public int GridPageSize { get; set; }

        public bool DisplayProductPictures { get; set; }
    }
}