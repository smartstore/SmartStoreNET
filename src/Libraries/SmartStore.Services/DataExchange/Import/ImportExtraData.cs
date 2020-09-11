using System;

namespace SmartStore.Services.DataExchange.Import
{
    [Serializable]
    public class ImportExtraData
    {
        /// <summary>
        /// Number of images per object to be imported
        /// </summary>
        public int? NumberOfPictures { get; set; }
    }
}
