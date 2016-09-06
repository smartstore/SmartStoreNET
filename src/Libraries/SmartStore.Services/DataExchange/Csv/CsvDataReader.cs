//  Wrapper for LumenWorks CsvReader (fork by phatcher)
//  ----------------------------------------------------
//	LumenWorks.Framework.IO.CsvReader
//	Copyright (c) 2006 Sébastien Lorion
//  https://github.com/phatcher/CsvReader/
//
//	MIT license (http://en.wikipedia.org/wiki/MIT_License)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LumenWorks.Framework.IO.Csv;

namespace SmartStore.Services.DataExchange.Csv
{
	/// <summary>
	/// Represents a reader that provides fast, non-cached, forward-only access to CSV data.  
	/// </summary>
	public class CsvDataReader : DisposableObject, IDataReader, IEnumerable<string[]>
	{
		private readonly CsvReader _csv;
		private readonly IDataReader _reader;

		/// <summary>
		/// Initializes a new instance of the CsvDataReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		public CsvDataReader(TextReader reader)
			: this(reader, new CsvConfiguration())
		{
		}

		/// <summary>
		/// Initializes a new instance of the CsvDataReader class.
		/// </summary>
		/// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
		public CsvDataReader(TextReader reader, CsvConfiguration configuration)
		{
			Guard.NotNull(reader, nameof(reader));
			Guard.NotNull(configuration, nameof(configuration));

			this.Configuration = configuration;

			_csv = new CsvReader(
				reader, 
				configuration.HasHeaders,
				configuration.Delimiter,
				configuration.Quote,
				configuration.Escape,
				configuration.Comment,
				configuration.TrimValues ? ValueTrimmingOptions.All : ValueTrimmingOptions.None,
				configuration.NullValue);

			_csv.SupportsMultiline = configuration.SupportsMultiline;
			_csv.SkipEmptyLines = configuration.SkipEmptyLines;
			_csv.DefaultHeaderName = configuration.DefaultHeaderName;
			_csv.DefaultParseErrorAction = (LumenWorks.Framework.IO.Csv.ParseErrorAction)((int)configuration.DefaultParseErrorAction);
			_csv.MissingFieldAction = (LumenWorks.Framework.IO.Csv.MissingFieldAction)((int)configuration.MissingFieldAction);

			_reader = _csv;
		}

		public CsvConfiguration Configuration
		{
			get;
			private set;
		}

		#region Public wrapper members

		/// <summary>
		/// Gets the field headers.
		/// </summary>
		/// <returns>The field headers or an empty array if headers are not supported.</returns>
		/// <exception cref="T:System.ComponentModel.ObjectDisposedException">
		///	The instance has been disposed of.
		/// </exception>
		public string[] GetFieldHeaders()
		{
			return _csv.GetFieldHeaders();
		}

		public bool EndOfStream
		{
			get
			{
				return _csv.EndOfStream;
			}
		}

		/// <summary>
		/// Gets the current row index in the CSV file (0-based).
		/// </summary>
		/// <value>The current row index in the CSV file.</value>
		public long CurrentRowIndex
		{
			get
			{
				return _csv.CurrentRecordIndex;
			}
		}

		public void CopyCurrentRowTo(string[] array)
		{
			_csv.CopyCurrentRecordTo(array);
		}

		/// <summary>
		/// Gets the current row's raw CSV data.
		/// </summary>
		/// <returns>The current raw CSV data.</returns>
		public string GetRawData()
		{
			return _csv.GetCurrentRawData();
		}

		/// <summary>
		/// Gets the field with the specified name. <see cref="M:hasHeaders"/> must be <see langword="true"/>.
		/// </summary>
		/// <value>
		/// The field with the specified name.
		/// </value>
		/// <exception cref="T:ArgumentNullException">
		///		<paramref name="field"/> is <see langword="null"/> or an empty string.
		/// </exception>
		/// <exception cref="T:InvalidOperationException">
		///	The CSV does not have headers (<see cref="M:HasHeaders"/> property is <see langword="false"/>).
		/// </exception>
		/// <exception cref="T:ArgumentException">
		///		<paramref name="field"/> not found.
		/// </exception>
		/// <exception cref="T:MalformedCsvException">
		///		The CSV appears to be corrupt at the current position.
		/// </exception>
		/// <exception cref="T:System.ComponentModel.ObjectDisposedException">
		///	The instance has been disposed of.
		/// </exception>
		public string this[string field]
		{
			get
			{
				return _csv[field];
			}
		}

		/// <summary>
		/// Gets the field at the specified index.
		/// </summary>
		/// <value>The field at the specified index.</value>
		/// <exception cref="T:ArgumentOutOfRangeException">
		///		<paramref name="index"/> must be included in [0, <see cref="M:FieldCount"/>[.
		/// </exception>
		/// <exception cref="T:InvalidOperationException">
		///		No record read yet. Call ReadLine() first.
		/// </exception>
		/// <exception cref="T:MalformedCsvException">
		///		The CSV appears to be corrupt at the current position.
		/// </exception>
		/// <exception cref="T:System.ComponentModel.ObjectDisposedException">
		///	The instance has been disposed of.
		/// </exception>
		public virtual string this[int index]
		{
			get
			{
				return _csv[index];
			}
		}

		#endregion

		#region IDataReader (implicit)

		public int FieldCount
		{
			get
			{
				return _reader.FieldCount;
			}
		}

		public bool Read()
		{
			return _csv.ReadNextRecord();
		}

		public int GetOrdinal(string name)
		{
			return _reader.GetOrdinal(name);
		}

		#endregion

		#region IDataReader (explicit)

		object IDataRecord.this[string name]
		{
			get
			{
				return _reader[name];
			}
		}

		object IDataRecord.this[int i]
		{
			get
			{
				return _reader[i];
			}
		}

		int IDataReader.Depth
		{
			get
			{
				return _reader.Depth;
			}
		}

		bool IDataReader.IsClosed
		{
			get
			{
				return _reader.IsClosed;
			}
		}

		int IDataReader.RecordsAffected
		{
			get
			{
				return _reader.RecordsAffected;
			}
		}

		void IDataReader.Close()
		{
			_reader.Close();
		}

		bool IDataRecord.GetBoolean(int i)
		{
			return _reader.GetBoolean(i);
		}

		byte IDataRecord.GetByte(int i)
		{
			return _reader.GetByte(i);
		}

		long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
		}

		char IDataRecord.GetChar(int i)
		{
			return _reader.GetChar(i);
		}

		long IDataRecord.GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
		{
			return _reader.GetChars(i, fieldOffset, buffer, bufferoffset, length);
		}

		IDataReader IDataRecord.GetData(int i)
		{
			return _reader.GetData(i);
		}

		string IDataRecord.GetDataTypeName(int i)
		{
			return _reader.GetDataTypeName(i);
		}

		DateTime IDataRecord.GetDateTime(int i)
		{
			return _reader.GetDateTime(i);
		}

		decimal IDataRecord.GetDecimal(int i)
		{
			return _reader.GetDecimal(i);
		}

		double IDataRecord.GetDouble(int i)
		{
			return _reader.GetDouble(i);
		}

		Type IDataRecord.GetFieldType(int i)
		{
			return _reader.GetFieldType(i);
		}

		float IDataRecord.GetFloat(int i)
		{
			return _reader.GetFloat(i);
		}

		Guid IDataRecord.GetGuid(int i)
		{
			return _reader.GetGuid(i);
		}

		short IDataRecord.GetInt16(int i)
		{
			return _reader.GetInt16(i);
		}

		int IDataRecord.GetInt32(int i)
		{
			return _reader.GetInt32(i);
		}

		long IDataRecord.GetInt64(int i)
		{
			return _reader.GetInt64(i);
		}

		string IDataRecord.GetName(int i)
		{
			return _reader.GetName(i);
		}

		DataTable IDataReader.GetSchemaTable()
		{
			return _reader.GetSchemaTable();
		}

		string IDataRecord.GetString(int i)
		{
			return _reader.GetString(i);
		}

		object IDataRecord.GetValue(int i)
		{
			return _reader.GetGuid(i);
		}

		int IDataRecord.GetValues(object[] values)
		{
			return _reader.GetValues(values);
		}

		bool IDataRecord.IsDBNull(int i)
		{
			return _reader.IsDBNull(i);
		}

		bool IDataReader.NextResult()
		{
			return _reader.NextResult();
		}

		#endregion

		#region IEnumerable<string[]>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _csv.GetEnumerator();
		}

		IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
		{
			return _csv.GetEnumerator();
		}

		#endregion

		#region Disposable

		protected override void OnDispose(bool disposing)
		{
			if (disposing)
			{
				_csv.Dispose();
			}
		}

		#endregion
	}
}
