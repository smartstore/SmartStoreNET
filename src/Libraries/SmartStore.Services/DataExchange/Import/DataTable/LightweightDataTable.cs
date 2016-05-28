using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Excel;

namespace SmartStore.Services.DataExchange.Import
{
	public class LightweightDataTable : IDataTable
	{
		private readonly IList<IDataColumn> _columns;
		private readonly IList<IDataRow> _rows;
		private readonly IDictionary<string, int> _columnIndexes;

		public LightweightDataTable(IList<IDataColumn> columns, IList<object[]> data)
		{
			Guard.ArgumentNotNull(() => columns);
			Guard.ArgumentNotNull(() => data);

			if (columns.Select(x => x.Name.ToLower()).Distinct().ToArray().Length != columns.Count)
			{
				throw Error.Argument("columns", "The columns collection cannot contain duplicate column names.");
			}

			_columns = new ReadOnlyCollection<IDataColumn>(columns);

			var rows = data.Select(x => new LightweightDataRow(this, x)).Cast<IDataRow>().ToList();
			_rows = new ReadOnlyCollection<IDataRow>(rows);

			_columnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (var i = 0; i < columns.Count; i++)
			{
				_columnIndexes[columns[i].Name] = i;
			}
        }

		public bool HasColumn(string name)
		{
			return _columnIndexes.ContainsKey(name);
		}

		public int GetColumnIndex(string name)
		{
			Guard.ArgumentNotEmpty(name, "name");

			int index;
			if (!_columnIndexes.TryGetValue(name, out index))
			{
				return -1;
			}

			return index;
		}

		public IList<IDataColumn> Columns
		{
			get
			{
				return _columns;
			}
		}

		public IList<IDataRow> Rows
		{
			get
			{
				return _rows;
			}
		}

		public static IDataTable FromPostedFile(
			HttpPostedFileBase file,
			int skip = 0,
			int take = int.MaxValue)
		{
			return FromPostedFile(file, new CsvConfiguration(), skip, take);
		}

		public static IDataTable FromPostedFile(
			HttpPostedFileBase file,
			CsvConfiguration configuration,
			int skip = 0,
			int take = int.MaxValue)
		{
			Guard.ArgumentNotNull(() => file);
			Guard.ArgumentNotNull(() => configuration);

			if (file.ContentLength == 0)
			{
				throw Error.Argument("file", "The posted file '{0}' does not contain any data.".FormatInvariant(file.FileName)); // TODO Loc
			}

			IDataReader dataReader = null;

			try
			{
				var fileExt = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

				switch (fileExt)
				{
					case ".xlsx":
						dataReader = new ExcelDataReader(file.InputStream, true); // TODO: let the user specify if excel file has headers
						break;
					default:
						dataReader = new CsvDataReader(new StreamReader(file.InputStream), configuration);
						break;
				}

				var table = LightweightDataTable.FromDataReader(dataReader);

				if (table.Columns.Count == 0 || table.Rows.Count == 0)
				{
					throw Error.InvalidOperation("file", "The posted file '{0}' does not contain any columns or data rows.".FormatInvariant(file.FileName)); // TODO Loc
				}

				return table;
			}
			catch
			{
				throw;
			}
			finally
			{
				if (dataReader != null)
				{
					if (!dataReader.IsClosed)
					{
						dataReader.Dispose();
					}
					dataReader = null;
				}
			}
		}

		public static IDataTable FromDataReader(
			IDataReader reader, 
			int skip = 0, 
			int take = int.MaxValue)
		{
			Guard.ArgumentNotNull(() => reader);

			if (reader.IsClosed)
				throw new ArgumentException("This operation is invalid when the reader is closed.", "reader");

			var columns = new List<IDataColumn>(reader.FieldCount);
			var data = new List<object[]>();

			var schema = reader.GetSchemaTable();

			var nameCol = schema.Columns[SchemaTableColumn.ColumnName];
			var typeCol = schema.Columns[SchemaTableColumn.DataType];

			foreach (DataRow schemaRow in schema.Rows)
			{
				var column = new LightweightDataColumn((string)schemaRow[nameCol], (Type)schemaRow[typeCol]);
				columns.Add(column);
			}

			var fieldCount = reader.FieldCount;

			int i = -1;
			while (reader.Read())
			{
				i++;

				if (skip > i)
					continue;

				if (i >= skip + take)
					break;

				var values = new object[fieldCount];
				reader.GetValues(values);
				data.Add(values);
			}

			var table = new LightweightDataTable(columns, data);

			return table;
		}
	}

	internal class LightweightDataRow : DynamicObject, IDataRow
	{
		private readonly IDataTable _table;
		private readonly object[] _values;

		public LightweightDataRow(IDataTable table, object[] values)
		{
			Guard.ArgumentNotNull(() => values);

			if (table.Columns.Count != values.Length)
			{
				throw new ArgumentOutOfRangeException(
					"values", 
					"The number of row values must match the number of columns. Expected: {0}, actual: {1}".FormatInvariant(table.Columns.Count, values.Length));
			}

			_table = table;
			_values = values;
		}

		public IDataTable Table
		{
			get { return _table; }
		}

		public object[] Values
		{
			get { return _values; }
		}

		public object this[string name]
		{
			get
			{
				var index = _table.GetColumnIndex(name);
				if (index < 0)
					throw new KeyNotFoundException();

				return _values[index];
			}
			set
			{
				var index = _table.GetColumnIndex(name);
				if (index < 0)
					throw new KeyNotFoundException();

				_values[index] = value;
			}
		}

		public object this[int index]
		{
			get
			{
				ValidateColumnIndex(index);
				return _values[index];
			}
			set
			{
				ValidateColumnIndex(index);
				_values[index] = value;
			}
		}

		private void ValidateColumnIndex(int index)
		{
			if (index < 0 || index >= _table.Columns.Count)
			{
				throw new ArgumentOutOfRangeException("index", index,
					"Column index must be included within [0, {0}], but specified column index was: '{1}'.".FormatInvariant(_table.Columns.Count, index));
			}
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _table.Columns.Select(x => x.Name);
		}


		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = null;

			if (this.TryGetValue(binder.Name, out result))
			{
				return true;
			}

			return false;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			return this.TrySetValue(binder.Name, value);
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			result = null;

			try
			{
				result = _values[(int)indexes[0]];
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			try
			{
				_values[(int)indexes[0]] = value;
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	internal class LightweightDataColumn : IDataColumn
	{
		public LightweightDataColumn(string name, Type type)
		{
			Guard.ArgumentNotEmpty(() => name);
			Guard.ArgumentNotNull(() => type);

			this.Name = name;
			this.Type = type;
		}

		public string Name
		{
			get;
			private set;
		}

		public Type Type
		{
			get;
			private set;
		}
	}
}
