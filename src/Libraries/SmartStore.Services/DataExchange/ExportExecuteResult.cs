using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SmartStore.Services.DataExchange
{
	[Serializable]
	public class ExportExecuteResult
	{
		public ExportExecuteResult()
		{
			Files = new List<ExportExecuteResult.ExportFileInfo>();
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

		/// <summary>
		/// Suggested download file name
		/// </summary>
		[XmlIgnore]
		public string DownloadFileName { get; set; }

		[Serializable]
		public class ExportFileInfo
		{
			/// <summary>
			/// Store identifier
			/// </summary>
			public int StoreId { get; set; }

			/// <summary>
			/// Name of file
			/// </summary>
			public string FileName { get; set; }
		}
	}
}
