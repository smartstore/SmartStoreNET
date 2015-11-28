using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore
{

	public static class SqlDataReaderExtensions
	{
		public static object GetValue(this SqlDataReader reader, string columnName) 
		{
			try 
			{
				if (reader != null && columnName.HasValue()) 
				{
					int ordinal = reader.GetOrdinal(columnName);
					return reader.GetValue(ordinal);
				}
			}
			catch (Exception exc) 
			{
				exc.Dump();
			}
			return null;
		}
	}
}
