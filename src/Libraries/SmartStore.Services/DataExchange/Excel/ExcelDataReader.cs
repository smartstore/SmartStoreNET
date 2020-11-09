using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace SmartStore.Services.DataExchange.Excel
{
    public class ExcelDataReader : DisposableObject, IDataReader
    {
        private ExcelPackage _package;
        private ExcelWorksheet _sheet;
        private int _totalRows;
        private int _totalColumns;

        private bool _initialized;
        private string[] _columns;
        private Dictionary<string, int> _columnIndexes;
        private int _currentRowIndex;
        private bool _eof;

        private readonly object _lock = new object();

        public ExcelDataReader(Stream source, bool hasHeaders)
        {
            Guard.NotNull(source, nameof(source));

            _package = new ExcelPackage(source);

            // get the first worksheet in the workbook
            _sheet = _package.Workbook.Worksheets.FirstOrDefault();
            if (_sheet == null)
            {
                throw Error.InvalidOperation("The excel package does not contain any worksheet.");
            }

            if (_sheet.Dimension == null)
            {
                throw Error.InvalidOperation("The excel worksheet does not contain any data.");
            }

            HasHeaders = hasHeaders;
            DefaultHeaderName = "Column";

            _totalColumns = _sheet.Dimension.End.Column;
            _totalRows = _sheet.Dimension.End.Row - (hasHeaders ? 1 : 0);

            _currentRowIndex = -1;
        }

        #region Configuration

        public bool HasHeaders
        {
            get;
            private set;
        }

        public string DefaultHeaderName
        {
            get;
            set;
        }

        #endregion

        #region public members

        public bool MoveToStart()
        {
            return MoveTo(0);
        }

        public bool MoveToEnd()
        {
            return MoveTo(_totalRows - 1);
        }

        public bool MoveTo(int row)
        {
            EnsureInitialize();
            ValidateDataReader(validateInitialized: false);

            if (row < 0 || row >= _totalRows)
                return false;

            _currentRowIndex = row;
            _eof = false;

            return true;
        }

        public bool EndOfStream => _eof;

        public int TotalRows
        {
            get
            {
                EnsureInitialize();
                return _totalRows;
            }
        }

        public IReadOnlyCollection<string> GetColumnHeaders()
        {
            EnsureInitialize();
            return _columns.AsReadOnly();
        }

        public int CurrentRowIndex => _currentRowIndex;

        public string GetFormatted(int i)
        {
            ValidateDataReader();
            return _sheet.Cells[ExcelRowIndex(_currentRowIndex), i + 1].Text;
        }

        public int GetColumnIndex(string name)
        {
            Guard.NotEmpty(name, nameof(name));

            EnsureInitialize();

            int index;

            if (_columnIndexes != null && _columnIndexes.TryGetValue(name, out index))
                return index;
            else
                return -1;
        }

        #endregion

        #region IDataReader

        bool IDataReader.NextResult()
        {
            ValidateDataReader(validateInitialized: false);
            return false;
        }

        public bool Read()
        {
            ValidateDataReader(validateInitialized: false);
            return ReadNextRow(false);
        }

        int IDataReader.Depth
        {
            get
            {
                ValidateDataReader(validateInitialized: false);
                return 0;
            }
        }

        bool IDataReader.IsClosed => _eof;

        int IDataReader.RecordsAffected => -1;

        void IDataReader.Close()
        {
            Dispose();
        }


        public int FieldCount
        {
            get
            {
                EnsureInitialize();
                return _totalColumns;
            }
        }

        public object this[string name]
        {
            get
            {
                int index = GetColumnIndex(name);

                if (index < 0)
                    Error.Argument("name", "'{0}' column header not found.".FormatInvariant(name));

                return this[index];
            }
        }

        public object this[int i]
        {
            get
            {
                ValidateDataReader();
                // Excel indexes start from 1
                return _sheet.GetValue(ExcelRowIndex(_currentRowIndex), i + 1);
            }
        }

        public bool GetBoolean(int i)
        {
            object value = this[i];

            int result;
            if (Int32.TryParse(value.ToString(), out result))
                return (result != 0);
            else
                return Boolean.Parse(value.ToString());
        }

        public byte GetByte(int i)
        {
            return this[i].Convert<byte>(CultureInfo.CurrentCulture);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            ValidateDataReader();
            return CopyFieldToArray(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return this[i].Convert<char>(CultureInfo.CurrentCulture);
        }

        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
        {
            ValidateDataReader();
            return CopyFieldToArray(i, fieldOffset, buffer, bufferoffset, length);
        }

        IDataReader IDataRecord.GetData(int i)
        {
            ValidateDataReader();

            if (i == 0)
                return this;
            else
                return null;
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            return this[i].GetType().FullName;
        }

        public DateTime GetDateTime(int i)
        {
            return this[i].Convert<DateTime>(CultureInfo.CurrentCulture);
        }

        public decimal GetDecimal(int i)
        {
            return this[i].Convert<decimal>(CultureInfo.CurrentCulture);
        }

        public double GetDouble(int i)
        {
            return this[i].Convert<double>(CultureInfo.CurrentCulture);
        }

        public Type GetFieldType(int i)
        {
            return this[i].GetType();
        }

        public float GetFloat(int i)
        {
            return this[i].Convert<float>(CultureInfo.CurrentCulture);
        }

        public Guid GetGuid(int i)
        {
            return this[i].Convert<Guid>(CultureInfo.CurrentCulture);
        }

        public short GetInt16(int i)
        {
            return this[i].Convert<short>(CultureInfo.CurrentCulture);
        }

        public int GetInt32(int i)
        {
            return this[i].Convert<int>(CultureInfo.CurrentCulture);
        }

        public long GetInt64(int i)
        {
            return this[i].Convert<long>(CultureInfo.CurrentCulture);
        }

        public string GetName(int i)
        {
            EnsureInitialize();
            ValidateDataReader(validateInitialized: false);

            if (i < 0 || i >= _columns.Length)
            {
                throw new ArgumentOutOfRangeException("i", i,
                    "Column index must be included within [0, {0}], but specified column index was: '{1}'.".FormatInvariant(_columns.Length, i));
            }

            return _columns[i];
        }

        public int GetOrdinal(string name)
        {
            return GetColumnIndex(name);
        }

        DataTable IDataReader.GetSchemaTable()
        {
            EnsureInitialize();
            ValidateDataReader(validateInitialized: false);

            var schema = new DataTable("SchemaTable")
            {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = _columns.Length
            };

            schema.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.DataType, typeof(object)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsKey, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsLong, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.NumericScale, typeof(short)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ProviderType, typeof(int)).ReadOnly = true;

            schema.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)).ReadOnly = true;

            // null marks columns that will change for each row
            object[] schemaRow =
            {
                true,					// 00- AllowDBNull
				null,					// 01- BaseColumnName
				string.Empty,			// 02- BaseSchemaName
				string.Empty,			// 03- BaseTableName
				null,					// 04- ColumnName
				null,					// 05- ColumnOrdinal
				int.MaxValue,			// 06- ColumnSize
				typeof(string),			// 07- DataType
				false,					// 08- IsAliased
				false,					// 09- IsExpression
				false,					// 10- IsKey
				false,					// 11- IsLong
				false,					// 12- IsUnique
				DBNull.Value,			// 13- NumericPrecision
				DBNull.Value,			// 14- NumericScale
				(int) DbType.String,	// 15- ProviderType
				string.Empty,			// 16- BaseCatalogName
				string.Empty,			// 17- BaseServerName
				false,					// 18- IsAutoIncrement
				false,					// 19- IsHidden
				true,					// 20- IsReadOnly
				false					// 21- IsRowVersion
			};

            int r = ExcelRowIndex(0);

            for (int i = 0; i < _columns.Length; i++)
            {
                schemaRow[1] = _columns[i]; // Base column name
                schemaRow[4] = _columns[i]; // Column name
                schemaRow[5] = i;           // Column ordinal

                // get data type from 1st row only
                var firstValue = _sheet.Cells[r, i + 1].Value;
                schemaRow[7] = firstValue != null ? firstValue.GetType() : typeof(string);

                schema.Rows.Add(schemaRow);
            }

            return schema;
        }

        public string GetString(int i)
        {
            return this[i].Convert<string>(CultureInfo.CurrentCulture);
        }

        public object GetValue(int i)
        {
            ValidateDataReader();
            return ((IDataRecord)this).IsDBNull(i) ? DBNull.Value : this[i];
        }

        int IDataRecord.GetValues(object[] values)
        {
            var record = (IDataRecord)this;

            for (int i = 0; i < _totalColumns; i++)
            {
                values[i] = record.GetValue(i);
            }

            return _totalColumns;
        }

        bool IDataRecord.IsDBNull(int i)
        {
            return this[i] == null;
        }

        #endregion

        #region Helper

        /// <summary>
        /// Reads the next record.
        /// </summary>
        /// <param name="onlyReadHeaders">
        /// Indicates if the reader will proceed to the next record after having read headers.
        /// <see langword="true"/> if it stops after having read headers; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns><see langword="true"/> if a record has been successfully reads; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        protected virtual bool ReadNextRow(bool onlyReadHeaders)
        {
            if (_eof)
                return false;

            CheckDisposed();

            if (!_initialized)
            {
                _columns = new string[_totalColumns];
                _columnIndexes = new Dictionary<string, int>(_totalColumns, StringComparer.OrdinalIgnoreCase);

                for (int i = 1; i <= _totalColumns; i++)
                {
                    // Excel indexes start from 1
                    string name = null;
                    if (HasHeaders)
                    {
                        name = _sheet.GetValue<string>(1, i).NullEmpty();
                    }

                    name = name ?? (DefaultHeaderName ?? "Column") + i;

                    _columns[i - 1] = name;
                    _columnIndexes[name] = i - 1;
                }

                if (_columns.Select(x => x.ToLower()).Distinct().ToArray().Length != _columns.Length)
                {
                    _columns = null;
                    _columnIndexes = null;
                    throw Error.InvalidOperation("The first row cannot contain duplicate column names.");
                }

                _initialized = true;

                if (!onlyReadHeaders)
                {
                    return ReadNextRow(false);
                }
            }
            else
            {
                _currentRowIndex++;
                if (_currentRowIndex >= _totalRows)
                {
                    _eof = true;
                    return false;
                }
            }

            return true;
        }

        private int ExcelRowIndex(int i)
        {
            // Excel indexes start from 1
            return i + (HasHeaders ? 2 : 1);
        }

        /// <summary>
        /// Ensures that the reader is initialized.
        /// </summary>
        private void EnsureInitialize()
        {
            if (!_initialized)
            {
                ReadNextRow(true);
            }

            Debug.Assert(_columns != null);
            Debug.Assert(_columns.Length > 0 || (_columns.Length == 0 && _columnIndexes == null));
        }

        private void ValidateDataReader(bool validateInitialized = true, bool validateNotClosed = true)
        {
            if (validateInitialized && (!_initialized || _currentRowIndex < 0))
                throw new InvalidOperationException("No current record. Call Read() to initialize the reader.");

            if (validateNotClosed && IsDisposed)
                throw new InvalidOperationException("This operation is invalid when the reader is closed.");
        }

        private long CopyFieldToArray(int column, long columnOffset, Array destinationArray, int destinationOffset, int length)
        {
            EnsureInitialize();

            if (column < 0 || column >= _totalColumns)
            {
                throw new ArgumentOutOfRangeException("column", column,
                    "Column index must be included within [0, {0}], but specified column index was: '{1}'.".FormatInvariant(_totalColumns, column));
            }

            if (columnOffset < 0 || columnOffset >= int.MaxValue)
                throw new ArgumentOutOfRangeException("fieldOffset");

            // Array.Copy(...) will do the remaining argument checks

            if (length == 0)
                return 0;

            string value = this[column].ToString();

            if (value == null)
                value = string.Empty;

            if (destinationArray.GetType() == typeof(char[]))
                Array.Copy(value.ToCharArray((int)columnOffset, length), 0, destinationArray, destinationOffset, length);
            else
            {
                char[] chars = value.ToCharArray((int)columnOffset, length);
                byte[] source = new byte[chars.Length];
                ;

                for (int i = 0; i < chars.Length; i++)
                    source[i] = Convert.ToByte(chars[i]);

                Array.Copy(source, 0, destinationArray, destinationOffset, length);
            }

            return length;
        }

        #endregion

        #region IDisposable Support

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _sheet = null;
                    if (_package != null)
                    {
                        lock (_lock)
                        {
                            if (_package != null)
                            {
                                _package.Dispose();
                                _package = null;
                                _eof = true;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        #endregion
    }
}
