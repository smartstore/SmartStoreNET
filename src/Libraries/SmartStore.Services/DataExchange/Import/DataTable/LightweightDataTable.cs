using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
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
		private readonly IDictionary<string, int> _alternativeColumnIndexes;

		public LightweightDataTable(IList<IDataColumn> columns, IList<object[]> data)
		{
			Guard.NotNull(columns, nameof(columns));
			Guard.NotNull(data, nameof(data));

			if (columns.Select(x => x.Name.ToLower()).Distinct().ToArray().Length != columns.Count)
			{
				throw Error.Argument("columns", "The columns collection cannot contain duplicate column names.");
			}

			_columns = new ReadOnlyCollection<IDataColumn>(columns);

			TrimData(data);

			var rows = data.Select(x => new LightweightDataRow(this, x)).Cast<IDataRow>().ToList();
			_rows = new ReadOnlyCollection<IDataRow>(rows);

			_columnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			_alternativeColumnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			for (var i = 0; i < columns.Count; i++)
			{
				var name = columns[i].Name;
				var alternativeName = GetAlternativeColumnNameFor(name);

				_columnIndexes[name] = i;

				if (!alternativeName.IsCaseInsensitiveEqual(name))
					_alternativeColumnIndexes[alternativeName] = i;
			}
		}

		private static void TrimData(IList<object[]> data)
		{
			// When a user deletes content instead of whole rows from an excel sheet,
			// our data table contains completely empty rows at the end.
			// Here we get rid of them as they are absolutely useless.
			for (int i = data.Count - 1; i >= 0; i--)
			{
				var allColumnsEmpty = data[i].All(x => x == null || x == DBNull.Value);
				if (allColumnsEmpty)
				{
					data.RemoveAt(i);
					//i--;
				}
				else
				{
					// get out here on the first occurence of a NON-empty row
					break;
				}
			}
		}

		public bool HasColumn(string name)
		{
			if (name.HasValue())
			{
				return (_columnIndexes.ContainsKey(name) || _alternativeColumnIndexes.ContainsKey(name));
			}

			return false;
		}

		public int GetColumnIndex(string name)
		{
			int index;

			if (name.HasValue())
			{
				if (_columnIndexes.TryGetValue(name, out index))
					return index;

				if (_alternativeColumnIndexes.TryGetValue(name, out index))
					return index;
			}

			return -1;
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

		public static string GetAlternativeColumnNameFor(string name)
		{
			if (name.IsEmpty())
				return name;

			return name
				.Replace(" ", "")
				.Replace("-", "")
				.Replace("_", "");
		}

		public static IDataTable FromPostedFile(
			HttpPostedFileBase file,
			int skip = 0,
			int take = int.MaxValue)
		{
			Guard.NotNull(file, nameof(file));

			return FromFile(file.FileName, file.InputStream, file.ContentLength, new CsvConfiguration(), skip, take);
		}

		public static IDataTable FromPostedFile(
			HttpPostedFileBase file,
			CsvConfiguration configuration,
			int skip = 0,
			int take = int.MaxValue)
		{
			Guard.NotNull(file, nameof(file));

			return FromFile(file.FileName, file.InputStream, file.ContentLength, configuration, skip, take);
		}

		public static IDataTable FromFile(
			string fileName,
			Stream stream,
			long contentLength,
			CsvConfiguration configuration,
			int skip = 0,
			int take = int.MaxValue)
		{
			Guard.NotEmpty(fileName, nameof(fileName));
			Guard.NotNull(stream, nameof(stream));
			Guard.NotNull(configuration, nameof(configuration));

			if (contentLength == 0)
			{
				throw Error.Argument("fileName", "The posted file '{0}' does not contain any data.".FormatInvariant(fileName));
			}

			IDataReader dataReader = null;

			try
			{
				var fileExt = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

				switch (fileExt)
				{
					case ".xlsx":
						dataReader = new ExcelDataReader(stream, true); // TODO: let the user specify if excel file has headers
						break;
					default:
						dataReader = new CsvDataReader(new StreamReader(stream), configuration);
						break;
				}

				var table = LightweightDataTable.FromDataReader(dataReader, skip, take);

				if (table.Columns.Count == 0 || table.Rows.Count == 0)
				{
					throw Error.InvalidOperation("The posted file '{0}' does not contain any columns or data rows.".FormatInvariant(fileName));
				}

				return table;
			}
			catch (Exception ex)
			{
				throw ex;
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
			Guard.NotNull(reader, nameof(reader));

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

			take = Math.Min(take, int.MaxValue - skip);

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
			Guard.NotNull(values, nameof(values));

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
			Guard.NotEmpty(name, nameof(name));
			Guard.NotNull(type, nameof(type));

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
