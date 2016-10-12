using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStore.Data.Caching
{
	[DesignerCategory("")]
	internal sealed class CachingCommand : DbCommand
	{
		private readonly DbCommand _command;
		private readonly CommandTreeFacts _commandTreeFacts;
		private readonly CacheTransactionInterceptor _cacheTransactionInterceptor;
		private readonly DbCachingPolicy _policy;

		public CachingCommand(
			DbCommand command, 
			CommandTreeFacts commandTreeFacts,
			CacheTransactionInterceptor cacheTransactionInterceptor,
			DbCachingPolicy policy)
		{
			Guard.NotNull(command, nameof(command));
			Guard.NotNull(commandTreeFacts, nameof(commandTreeFacts));
			Guard.NotNull(cacheTransactionInterceptor, nameof(cacheTransactionInterceptor));

			_command = command;
			_commandTreeFacts = commandTreeFacts;
			_cacheTransactionInterceptor = cacheTransactionInterceptor;
			_policy = policy;
		}

		internal CommandTreeFacts CommandTreeFacts
		{
			get { return _commandTreeFacts; }
		}

		internal CacheTransactionInterceptor CacheTransactionInterceptor
		{
			get { return _cacheTransactionInterceptor; }
		}

		internal DbCachingPolicy CachingPolicy
		{
			get { return _policy; }
		}

		internal DbCommand WrappedCommand
		{
			get { return _command; }
		}

		private bool IsCacheable
		{
			get
			{
				return _commandTreeFacts.IsQuery &&
					   (IsQueryCached || !_commandTreeFacts.UsesNonDeterministicFunctions
					   && _policy.CanBeCached(_commandTreeFacts.AffectedEntitySets, CommandText,
						   Parameters.Cast<DbParameter>()
							   .Select(p => new KeyValuePair<string, object>(p.ParameterName, p.Value))));
			}
		}

		private bool IsQueryCached
		{
			get
			{
				return SingletonQueries.Current.IsQueryCached(_commandTreeFacts.MetadataWorkspace, CommandText);
			}
		}

		public override void Cancel()
		{
			_command.Cancel();
		}

		public override string CommandText
		{
			get
			{
				return _command.CommandText;
			}
			set
			{
				_command.CommandText = value;
			}
		}

		public override int CommandTimeout
		{
			get
			{
				return _command.CommandTimeout;
			}
			set
			{
				_command.CommandTimeout = value;
			}
		}

		public override CommandType CommandType
		{
			get
			{
				return _command.CommandType;
			}
			set
			{
				_command.CommandType = value;
			}
		}

		protected override DbParameter CreateDbParameter()
		{
			return _command.CreateParameter();
		}

		protected override DbConnection DbConnection
		{
			get
			{
				return _command.Connection;
			}
			set
			{
				_command.Connection = value;
			}
		}

		protected override DbParameterCollection DbParameterCollection
		{
			get { return _command.Parameters; }
		}

		protected override DbTransaction DbTransaction
		{
			get
			{
				return _command.Transaction;
			}
			set
			{
				_command.Transaction = value;
			}
		}

		public override bool DesignTimeVisible
		{
			get
			{
				return _command.DesignTimeVisible;
			}
			set
			{
				_command.DesignTimeVisible = value;
			}
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			if (!IsCacheable)
			{
				var result = _command.ExecuteReader(behavior);

				if (!_commandTreeFacts.IsQuery)
				{
					_cacheTransactionInterceptor.InvalidateSets(Transaction, _commandTreeFacts.AffectedEntitySets.Select(s => s.Name));
				}

				return result;
			}

			var key = CreateKey();

			object value;
			if (_cacheTransactionInterceptor.GetItem(Transaction, key, out value))
			{
				return new CachingDataReader((CachedRecords)value);
			}

			using (var reader = _command.ExecuteReader(behavior))
			{
				var queryResults = new List<object[]>();

				while (reader.Read())
				{
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					queryResults.Add(values);
				}

				return HandleCaching(reader, key, queryResults);
			}
		}

		protected async override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
		{
			if (!IsCacheable)
			{
				var result = await _command.ExecuteReaderAsync(behavior, cancellationToken);

				if (!_commandTreeFacts.IsQuery)
				{
					_cacheTransactionInterceptor.InvalidateSets(Transaction, _commandTreeFacts.AffectedEntitySets.Select(s => s.Name));
				}

				return result;
			}

			var key = CreateKey();

			object value;
			if (_cacheTransactionInterceptor.GetItem(Transaction, key, out value))
			{
				return new CachingDataReader((CachedRecords)value);
			}

			using (var reader = await _command.ExecuteReaderAsync(behavior, cancellationToken))
			{
				var queryResults = new List<object[]>();

				while (await reader.ReadAsync(cancellationToken))
				{
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					queryResults.Add(values);
				}

				return HandleCaching(reader, key, queryResults);
			}
		}

		private DbDataReader HandleCaching(DbDataReader reader, string key, List<object[]> queryResults)
		{
			var cachedResult = new CachedRecords
			{
				TableMetadata = GetTableMetadata(reader),
				Records = queryResults,
				RecordsAffected = reader.RecordsAffected
			};

			int minCacheableRows, maxCachableRows;
			_policy.GetCacheableRows(_commandTreeFacts.AffectedEntitySets, out minCacheableRows, out maxCachableRows);

			if (IsQueryCached || (queryResults.Count >= minCacheableRows && queryResults.Count <= maxCachableRows))
			{
				TimeSpan? duration = _policy.GetExpirationTimeout(_commandTreeFacts.AffectedEntitySets);

				_cacheTransactionInterceptor.PutItem(
					Transaction,
					key,
					cachedResult,
					_commandTreeFacts.AffectedEntitySets.Select(s => s.Name),
					duration);
			}

			return new CachingDataReader(cachedResult);
		}

		protected override void Dispose(bool disposing)
		{
			_command.GetType()
				.GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(_command, new object[] { disposing });
		}

		private static ColumnMetadata[] GetTableMetadata(DbDataReader reader)
		{
			var columnMetadata = new ColumnMetadata[reader.FieldCount];

			for (var i = 0; i < reader.FieldCount; i++)
			{
				columnMetadata[i] = new ColumnMetadata
				{
					Name = reader.GetName(i),
					DataTypeName = reader.GetDataTypeName(i),
					DataType = reader.GetFieldType(i)
				};
			}

			return columnMetadata;
		}

		public override int ExecuteNonQuery()
		{
			var recordsAffected = _command.ExecuteNonQuery();

			InvalidateSetsForNonQuery(recordsAffected);

			return recordsAffected;
		}

		public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			var recordsAffected = await _command.ExecuteNonQueryAsync(cancellationToken);

			InvalidateSetsForNonQuery(recordsAffected);

			return recordsAffected;
		}

		private void InvalidateSetsForNonQuery(int recordsAffected)
		{
			if (recordsAffected > 0 && _commandTreeFacts.AffectedEntitySets.Any())
			{
				_cacheTransactionInterceptor.InvalidateSets(Transaction, _commandTreeFacts.AffectedEntitySets.Select(s => s.Name));
			}
		}

		public override object ExecuteScalar()
		{
			if (!IsCacheable)
			{
				return _command.ExecuteScalar();
			}

			var key = CreateKey();

			object value;

			if (_cacheTransactionInterceptor.GetItem(Transaction, key, out value))
			{
				return value;
			}

			value = _command.ExecuteScalar();

			// TODO: somehow determine expiration
			TimeSpan? duration = null;

			_cacheTransactionInterceptor.PutItem(
				Transaction,
				key,
				value,
				_commandTreeFacts.AffectedEntitySets.Select(s => s.Name),
				duration);

			return value;
		}

		public async override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
		{
			if (!IsCacheable)
			{
				return await _command.ExecuteScalarAsync(cancellationToken);
			}

			var key = CreateKey();

			object value;

			if (_cacheTransactionInterceptor.GetItem(Transaction, key, out value))
			{
				return value;
			}

			value = await _command.ExecuteScalarAsync(cancellationToken);

			// TODO: somehow determine expiration
			TimeSpan? duration = null;

			_cacheTransactionInterceptor.PutItem(
				Transaction,
				key,
				value,
				_commandTreeFacts.AffectedEntitySets.Select(s => s.Name),
				duration);

			return value;
		}

		public override void Prepare()
		{
			_command.Prepare();
		}

		public override UpdateRowSource UpdatedRowSource
		{
			get
			{
				return _command.UpdatedRowSource;
			}
			set
			{
				_command.UpdatedRowSource = value;
			}
		}

		private string CreateKey()
		{
			return
				string.Format(
				"{0}_{1}_{2}",
				Connection.Database,
				CommandText,
				string.Join(
					"_",
					Parameters.Cast<DbParameter>()
					.Select(p => string.Format("{0}={1}", p.ParameterName, p.Value))));
		}
	}
}
