using System;

namespace SmartStore.Core.Domain.Export
{
	public enum ExportFileType
	{
		Xml = 0,
		Xls,
		Csv,
		Txt,
		Pdf
	}


	public enum ExportEntityType
	{
		Product = 0,
		Category,
		Manufacturer,
		Customer,
		Order
	}


	[Serializable]
	public class ExportPartition
	{
		public ExportPartition()
		{
			Limit = 100;
			BatchSize = 1000;
		}

		/// <summary>
		/// The number of records to be skipped
		/// </summary>
		public int Offset { get; set; }

		/// <summary>
		/// How many records to be loaded per database round-trip
		/// </summary>
		public int Limit { get; set; }

		/// <summary>
		/// The maximum number of records of one processed batch
		/// </summary>
		public int BatchSize { get; set; }

		/// <summary>
		/// Whether to start a separate run-through for each store
		/// </summary>
		public bool PerStore { get; set; }
	}
}
