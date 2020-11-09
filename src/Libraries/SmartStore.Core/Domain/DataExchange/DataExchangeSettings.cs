using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.DataExchange
{
    public class DataExchangeSettings : ISettings
    {
        public DataExchangeSettings()
        {
            MaxFileNameLength = 50;
            ImageDownloadTimeout = 10;
        }

        /// <summary>
        /// The maximum length of file names (in characters) of files created by the export framework
        /// </summary>
        public int MaxFileNameLength { get; set; }

        /// <summary>
        /// Relative path to a folder with images to be imported
        /// </summary>
        public string ImageImportFolder { get; set; }

        /// <summary>
        /// The timeout for image download per entity in minutes
        /// </summary>
        public int ImageDownloadTimeout { get; set; }
    }
}
