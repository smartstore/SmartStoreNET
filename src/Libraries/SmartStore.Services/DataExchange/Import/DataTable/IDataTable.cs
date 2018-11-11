using System;
using System.Collections.Generic;

namespace SmartStore.Services.DataExchange.Import
{
	public interface IDataColumn
	{
		string Name { get; }
		Type Type { get; }
	}

	public interface IDataRow
	{
		object[] Values { get; }
		object this[int index] { get; set; }
		object this[string name] { get; set; }

		IDataTable Table { get; }
	}

	public interface IDataTable
	{
		bool HasColumn(string name);
		int GetColumnIndex(string name);
        IList<IDataColumn> Columns { get; }
		IList<IDataRow> Rows { get; }
	}

	public static class IDataRowExtensions
	{
		public static object GetValue(this IDataRow row, int index)
		{
			return row[index];
		}

		public static object GetValue(this IDataRow row, string name)
		{
			return row[name];
		}

		public static void SetValue(this IDataRow row, int index, object value)
		{
			row[index] = value;
		}

		public static void SetValue(this IDataRow row, string name, object value)
		{
			row[name] = value;
		}

		public static bool TryGetValue(this IDataRow row, string name, out object value)
		{
			value = null;

			var index = row.Table.GetColumnIndex(name);
			if (index < 0)
				return false;

			value = row[index];
			return true;
		}

		public static bool TrySetValue(this IDataRow row, string name, object value)
		{
			var index = row.Table.GetColumnIndex(name);
			if (index < 0)
				return false;

			row[index] = value;
			return true;
		}
	}
}
