using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.IO;

namespace SmartStore.Data.Utilities
{
    public class SqlBlobStream : Stream
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private DbConnection _connection;
        private DbDataReader _reader;

        private long? _length;
        private long _dataIndex = 0;

        public SqlBlobStream(
            IDbConnectionFactory connectionFactory,
            string connectionString,
            string tableName,
            string blobColumnName,
            string pkColumnName,
            object pkColumnValue)
        {
            Guard.NotNull(connectionFactory, nameof(connectionFactory));
            Guard.NotEmpty(connectionString, nameof(connectionString));
            Guard.NotEmpty(tableName, nameof(tableName));
            Guard.NotEmpty(blobColumnName, nameof(blobColumnName));
            Guard.NotEmpty(pkColumnName, nameof(pkColumnName));
            Guard.NotNull(pkColumnValue, nameof(pkColumnValue));

            ConnectionString = connectionString;
            TableName = tableName;
            BlobColumnName = blobColumnName;
            PkColumnName = pkColumnName;
            PkColumnValue = pkColumnValue;

            _connectionFactory = connectionFactory;
        }

        private void EnsureOpen()
        {
            if (_reader != null)
                return;

            _connection = _connectionFactory.CreateConnection(ConnectionString);
            _connection.Open();

            using (var command = _connection.CreateCommand())
            {
                PrimaryKey = command.CreateParameter();
                PrimaryKey.ParameterName = "@" + PkColumnName;
                PrimaryKey.Value = PkColumnValue;

                command.Connection = _connection;
                command.CommandType = CommandType.Text;
                command.CommandText = $"SELECT [{BlobColumnName}] FROM [{TableName}] WHERE [{PrimaryKey.ParameterName.Substring(1)}] = {PrimaryKey.Value}";
                command.Parameters.Add(PrimaryKey);

                _reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection);
                if (!_reader.Read())
                {
                    throw new DataException($"Blob [{TableName}].[{BlobColumnName}] with id '{PrimaryKey.Value}' does not exist.");
                }

                if (_reader.IsDBNull(0))
                {
                    _length = 0;
                }
            }
        }

        public string ConnectionString { get; private set; }
        public string TableName { get; private set; }
        public string BlobColumnName { get; private set; }
        public string PkColumnName { get; private set; }
        public object PkColumnValue { get; private set; }
        public DbParameter PrimaryKey { get; private set; }

        #region Stream members

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin != SeekOrigin.Begin)
            {
                throw new NotSupportedException($"{nameof(SqlBlobStream)} only supports 'SeekOrigin.Begin'.");
            }

            if (offset == _dataIndex)
            {
                return 0;
            }

            if (offset < _dataIndex)
            {
                // This is a forward-only read. Close and start over again.
                CloseReader();
                return Seek(offset, origin);
            }

            if (offset > _dataIndex)
            {
                EnsureOpen();

                var buffer = new byte[1];
                var num = _reader.GetBytes(0, offset - 1, buffer, 0, 1);
                _dataIndex = offset;
                return num;
            }

            return 0;
        }

        public override bool CanSeek => true;
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override long Position
        {
            get => _dataIndex;
            set => Seek(value, SeekOrigin.Begin);
        }
        public override long Length
        {
            get
            {
                EnsureOpen();

                if (_length == null)
                {
                    _length = _reader.GetBytes(0, 0, null, 0, 0);
                }

                return _length.Value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureOpen();

            var read = _reader.GetBytes(0, _dataIndex + offset, buffer, 0, count);
            _dataIndex += read;

            return (int)read;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseReader();
            }
        }

        public void CloseReader()
        {
            if (_reader != null)
            {
                if (!_reader.IsClosed)
                    _reader.Close();

                _reader = null;
            }

            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

                _connection.Dispose();
                _connection = null;
            }

            _dataIndex = 0;
            PrimaryKey = null;
        }

        #endregion
    }
}
