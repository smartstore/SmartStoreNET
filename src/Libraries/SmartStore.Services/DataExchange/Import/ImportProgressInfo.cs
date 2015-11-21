using System;

namespace SmartStore.Services.DataExchange.Import
{
	
	public class ImportProgressInfo
	{

		public int TotalRecords
		{
			get;
			set;
		}

		public int TotalProcessed 
		{
			get;
			set;
		}
		 
		public double ProcessedPercent
		{
			get
			{
				if (TotalRecords == 0)
					return 0;

				return ((double)TotalProcessed / (double)TotalRecords) * 100;
			}
		}

		public int NewRecords
		{
			get;
			set;
		}

		public int ModifiedRecords
		{
			get;
			set;
		}

		public TimeSpan ElapsedTime
		{
			get;
			set;
		}

		public int TotalWarnings
		{
			get;
			set;
		}

		public int TotalErrors
		{
			get;
			set;
		}
	}

}
