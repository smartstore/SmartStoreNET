using System;
using System.Collections.Generic;
using System.Xml.Serialization;

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
		public bool Succeeded
		{
			get { return LastError.IsEmpty(); }
		}

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
			/// Name of file
			/// </summary>
			public string FileName { get; set; }

			/// <summary>
			/// Short optional text that describes the content of the file
			/// </summary>
			public string Label { get; set; }

			/// <summary>
			/// Whether the file contains entity data
			/// </summary>
			public bool IsDataFile { get; set; }
		}
	}
}
