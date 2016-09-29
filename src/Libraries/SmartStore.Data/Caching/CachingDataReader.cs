using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace SmartStore.Data.Caching
{
	internal class CachingDataReader : DbDataReader
	{
		private enum State
		{
			BOF,
			Reading,
			EOF,
			Closed,
			Disposed
		}

		private State _state;
		private readonly ColumnMetadata[] _tableMetadata;
		private readonly int _recordsAffected;

		// TODO: multiple resultsets?
		private readonly IEnumerator<object[]> _resultRowsEnumerator;

		internal CachingDataReader(CachedRecords cachedResult)
		{
			Guard.NotNull(cachedResult, nameof(cachedResult));

			_tableMetadata = cachedResult.TableMetadata;
			_recordsAffected = cachedResult.RecordsAffected;
			_resultRowsEnumerator = cachedResult.Records.GetEnumerator();
			_state = State.BOF;
		}

		public override void Close()
		{
			EnsureNotDisposed();

			_state = State.Closed;
		}

		public override int Depth
		{
			get { throw new NotImplementedException(); }
		}

		public override int FieldCount
		{
			get { return _tableMetadata.Length; }
		}

		public override bool GetBoolean(int ordinal)
		{
			return GetValue(ordinal).Convert<bool>();
		}

		public override byte GetByte(int ordinal)
		{
			return GetValue(ordinal).Convert<byte>();
		}

		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override char GetChar(int ordinal)
		{
			throw new NotImplementedException();
		}

		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override string GetDataTypeName(int ordinal)
		{
			return _tableMetadata[ordinal].DataTypeName;
		}

		public override DateTime GetDateTime(int ordinal)
		{
			return GetValue(ordinal).Convert<DateTime>();
		}

		public override decimal GetDecimal(int ordinal)
		{
			return GetValue(ordinal).Convert<decimal>();
		}

		public override double GetDouble(int ordinal)
		{
			return (double)GetValue(ordinal);
		}

		public override IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public override Type GetFieldType(int ordinal)
		{
			return _tableMetadata[ordinal].DataType;
		}

		public override float GetFloat(int ordinal)
		{
			return GetValue(ordinal).Convert<float>();
		}

		public override Guid GetGuid(int ordinal)
		{
			return GetValue(ordinal).Convert<Guid>();
		}

		public override short GetInt16(int ordinal)
		{
			return GetValue(ordinal).Convert<short>();
		}

		public override int GetInt32(int ordinal)
		{
			return GetValue(ordinal).Convert<int>();
		}

		public override long GetInt64(int ordinal)
		{
			return GetValue(ordinal).Convert<long>();
		}

		public override string GetName(int ordinal)
		{
			return _tableMetadata[ordinal].Name;
		}

		public override int GetOrdinal(string name)
		{
			throw new NotImplementedException();
		}

		public override DataTable GetSchemaTable()
		{
			throw new NotSupportedException();
		}

		public override string GetString(int ordinal)
		{
			return GetValue(ordinal).Convert<string>();
		}

		public override object GetValue(int ordinal)
		{
			EnsureReading();

			return _resultRowsEnumerator.Current[ordinal];
		}

		public override int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public override bool HasRows
		{
			get { throw new NotImplementedException(); }
		}

		public override bool IsClosed
		{
			get
			{
				EnsureNotDisposed();

				return _state == State.Closed;
			}
		}

		public override bool IsDBNull(int ordinal)
		{
			return GetValue(ordinal) == DBNull.Value || GetValue(ordinal) == null;
		}

		public override bool NextResult()
		{
			// TODO: Multiple resultsets
			return false;
		}

		public override bool Read()
		{
			EnsureNotClosed();

			var result = _resultRowsEnumerator.MoveNext();

			_state = result ? State.Reading : State.EOF;

			return result;
		}

		public override int RecordsAffected
		{
			get { return _recordsAffected; }
		}

		public override object this[string name]
		{
			get { throw new NotImplementedException(); }
		}

		public override object this[int ordinal]
		{
			get { throw new NotImplementedException(); }
		}

		private void EnsureReading()
		{
			EnsureNotClosed();
			EnsureNotBOF();
			EnsureNotEOF();
		}

		private void EnsureNotClosed()
		{
			if (IsClosed)
			{
				throw new InvalidOperationException("Reader has already been closed.");
			}
		}

		private void EnsureNotDisposed()
		{
			if (_state == State.Disposed)
			{
				throw new InvalidOperationException("Object has already been disposed.");
			}
		}

		private void EnsureNotBOF()
		{
			if (_state == State.BOF)
			{
				throw new InvalidOperationException("The operation is invalid before reading any data.");
			}
		}

		private void EnsureNotEOF()
		{
			if (_state == State.EOF)
			{
				throw new InvalidOperationException("The operation is invalid after reading all data.");
			}
		}

		protected override void Dispose(bool disposing)
		{
			// base.Dispose() will call Close()
			base.Dispose(disposing);

			_resultRowsEnumerator.Dispose();

			_state = State.Disposed;
		}
	}
}
