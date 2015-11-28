using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Text;
using System.Web.Routing;
using SmartStore.Core;

namespace SmartStore
{   
    public static class MiscExtensions
    {
		public static void Dump(this Exception exc) 
		{
			try 
			{
				exc.StackTrace.Dump();
				exc.Message.Dump();
			}
			catch { }
		}

		public static string ToAllMessages(this Exception exception)
		{
			var sb = new StringBuilder();

			while (exception != null)
			{
				if (!sb.ToString().EmptyNull().Contains(exception.Message))
				{
					sb.Grow(exception.Message, " * ");
				}
				exception = exception.InnerException;
			}
			return sb.ToString();
		}

		public static string ToElapsedMinutes(this Stopwatch watch) 
        {
			return "{0:0.0}".FormatWith(TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalMinutes);
		}

		public static string ToElapsedSeconds(this Stopwatch watch) 
        {
			return "{0:0.0}".FormatWith(TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds);
		}

		public static bool HasColumn(this DataView dv, string columnName) 
        {
			dv.RowFilter = "ColumnName='" + columnName + "'";
			return dv.Count > 0;
		}

		public static string GetDataType(this DataTable dt, string columnName) 
        {
			dt.DefaultView.RowFilter = "ColumnName='" + columnName + "'";
			return dt.Rows[0]["DataType"].ToString();
		}

		public static int CountExecute(this OleDbConnection conn, string sqlCount) 
        {
			using (OleDbCommand cmd = new OleDbCommand(sqlCount, conn)) 
            {
				return (int)cmd.ExecuteScalar();
			}
		}

		public static object SafeConvert(this TypeConverter converter, string value) 
        {
			try 
            {
				if (converter != null && value.HasValue() && converter.CanConvertFrom(typeof(string))) 
                {
					return converter.ConvertFromString(value);
				}
			}
			catch (Exception exc) 
            {
				exc.Dump();
			}
			return null;
		}

		public static bool IsEqual(this TypeConverter converter, string value, object compareWith) 
        {
			object convertedObject = converter.SafeConvert(value);

			if (convertedObject != null && compareWith != null)
				return convertedObject.Equals(compareWith);

			return false;
		}

        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }

		/// <summary>Converts bytes into a hex string.</summary>
		public static string ToHexString(this byte[] bytes, int length = 0)
		{
			if (bytes == null || bytes.Length <= 0)
				return "";

			var sb = new StringBuilder();

			foreach (byte b in bytes)
			{
				sb.Append(b.ToString("x2"));

				if (length > 0 && sb.Length >= length)
					break;
			}
			return sb.ToString();
		}

		public static T GetMergedDataValue<T>(this IMergedData mergedData, string key, T defaultValue)
		{
			try
			{
				if (mergedData.MergedDataValues != null && !mergedData.MergedDataIgnore)
				{
					object value;

					if (mergedData.MergedDataValues.TryGetValue(key, out value))
						return (T)value;
				}
			}
			catch (Exception) { }

			return defaultValue;
		}

		public static bool IsRouteEqual(this RouteData routeData, string controller, string action)
		{
			if (routeData == null)
				return false;

			return routeData.GetRequiredString("controller").IsCaseInsensitiveEqual(controller) && routeData.GetRequiredString("action").IsCaseInsensitiveEqual(action);
		}

		/// <summary>
		/// Append grow if string builder is empty. Append delimiter and grow otherwise.
		/// </summary>
		/// <param name="sb">Target string builder</param>
		/// <param name="grow">Value to append</param>
		/// <param name="delimiter">Delimiter to use</param>
		public static void Grow(this StringBuilder sb, string grow, string delimiter)
		{
			Guard.ArgumentNotNull(delimiter, "delimiter");

			if (!string.IsNullOrWhiteSpace(grow))
			{
				if (sb.Length <= 0)
					sb.Append(grow);
				else
					sb.AppendFormat("{0}{1}", delimiter, grow);
			}
		}

		public static string SafeGet(this string[] arr, int index)
		{
			return (arr != null && index < arr.Length ? arr[index] : "");
		}
    }
}
