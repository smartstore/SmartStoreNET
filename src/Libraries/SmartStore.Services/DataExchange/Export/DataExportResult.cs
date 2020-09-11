using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange.Export
{
    [Serializable]
    public class DataExportResult
    {
        public DataExportResult()
        {
            Files = new List<ExportFileInfo>();
        }

        /// <summary>
        /// Whether the export succeeded
        /// </summary>
        public bool Succeeded => LastError.IsEmpty();

        /// <summary>
        /// Last error
        /// </summary>
        [XmlIgnore]
        public string LastError { get; set; }

        /// <summary>
        /// Files created by last export
        /// </summary>
        public List<ExportFileInfo> Files { get; set; }

        /// <summary>
        /// The path of the folder with the export files
        /// </summary>
        [XmlIgnore]
        public string FileFolder { get; set; }

        [Serializable]
        public class ExportFileInfo
        {
            /// <summary>
            /// Store identifier, can be 0.
            /// </summary>
            public int StoreId { get; set; }

            /// <summary>
            /// Name of file.
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Short optional text that describes the content of the file.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// The related entity type.
            /// </summary>
            public RelatedEntityType? RelatedType { get; set; }
        }
    }


    public class DataExportPreviewResult
    {
        public DataExportPreviewResult()
        {
            Data = new List<dynamic>();
        }

        /// <summary>
        /// Preview data.
        /// </summary>
        public List<dynamic> Data { get; set; }

        /// <summary>
        /// Number of total records.
        /// </summary>
        public int TotalRecords { get; set; }
    }
}
