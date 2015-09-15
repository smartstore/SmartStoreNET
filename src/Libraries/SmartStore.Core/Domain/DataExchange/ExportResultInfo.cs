using System;
using System.Collections.Generic;

namespace SmartStore.Core.Domain.DataExchange
{
	[Serializable]
	public class ExportResultInfo
	{
		/// <summary>
		/// Files created by last export
		/// </summary>
		public List<ExportResultFileInfo> Files { get; set; }
	}


	[Serializable]
	public class ExportResultFileInfo
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
