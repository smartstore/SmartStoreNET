using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore
{

	public static class MiscExtensions
	{
		public static bool IsDatabaseAvailable(this SqlConnectionStringBuilder sb) 
		{
			bool res = false;
			try 
			{
				using (SqlConnection conn = new SqlConnection(sb.ToString())) 
				{
					conn.Open();
					res = (conn.State == System.Data.ConnectionState.Open);
					conn.Close();
				}
			}
			catch (Exception) 
			{
			}
			return res;
		}

		public static T GetValue<T>(this OleDbDataReader reader, string name, T defaultValue = default(T)) 
		{
			try 
			{
				object value;
				if (reader != null && name.HasValue() && (value = reader[name]) != null && value != DBNull.Value) 
				{
					return (T)Convert.ChangeType(value, typeof(T));
				}
			}
			catch (Exception exc) 
			{
				exc.Dump();
			}
			return defaultValue;
		}

	}
}
