using System;
using System.Globalization;
using System.Linq.Expressions;
using SmartStore.ComponentModel;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange.Import
{
	public class ImportRow<T> where T : BaseEntity
	{
		private const string ExplicitNull = "[NULL]";
		private const string ExplicitIgnore = "[IGNORE]";

		private bool _initialized = false;
		private T _entity;
		private string _entityDisplayName;
		private readonly int _position;
		private bool _isNew;
		private bool _isDirty;
		private ImportRowInfo _rowInfo;

		private readonly ImportDataSegmenter _segmenter;
		private readonly IDataRow _row;

		public ImportRow(ImportDataSegmenter parent, IDataRow row, int position)
		{
			_segmenter = parent;
			_row = row;
			_position = position;
		}

		public void Initialize(T entity, string entityDisplayName)
		{
			_entity = entity;
			_entityDisplayName = entityDisplayName;
			_isNew = _entity.Id == 0;

			_initialized = true;
		}

		private void CheckInitialized()
		{
			if (_initialized)
			{
				throw Error.InvalidOperation("A row must be initialized before interacting with the entity or the data store");
			}
		}

		private TProp GetDefaultValue<TProp>(ColumnMappingItem mapping, TProp defaultValue, ImportResult result = null)
		{
			if (mapping != null && mapping.Default.HasValue())
			{
				try
				{
					return mapping.Default.Convert<TProp>(_segmenter.Culture);
				}
				catch (Exception exception)
				{
					if (result != null)
					{
						var msg = "Failed to convert default value '{0}'. Please specify a convertable default value. Column: {1}";
						result.AddWarning(msg.FormatInvariant(mapping.Default, exception.Message), this.GetRowInfo(), mapping.SoureName);
					}
				}
			}

			return defaultValue;
		}

		public bool IsTransient
		{
			get { return _entity.Id == 0; }
		}

		public bool IsNew
		{
			get { return _isNew; }
		}

		public bool IsDirty
		{
			get { return _isDirty; }
		}

		public ImportDataSegmenter Segmenter
		{
			get { return _segmenter; }
		}

		public T Entity
		{
			get { return _entity; }
		}

		public IDataRow DataRow
		{
			get { return _row; }
		}

		public string EntityDisplayName
		{
			get { return _entityDisplayName; }
		}

		public bool NameChanged
		{
			get;
			set;
		}

		public int Position
		{
			get { return _position; }
		}

		/// <summary>
		/// Determines whether a specific column exists in the underlying data table 
		/// and contains a non-null, convertible value.
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <param name="withAnyIndex">
		///		If <c>true</c> and a column with the passed <paramref name="columnName"/> does not exist,
		///		this method seeks for any indexed column with the same name.
		/// </param>
		/// <returns><c>true</c> if the column exists and contains a value, <c>false</c> otherwise</returns>
		/// <remarks>
		///		This method takes mapped column names into account.
		/// </remarks>
		public bool HasDataValue(string columnName, bool withAnyIndex = false)
		{
			var result = HasDataValue(columnName, null);

			if (!result && withAnyIndex)
			{
				// Column does not have a value, but withAnyIndex is true:
				// Test for values in any indexed column.
				var indexes = _segmenter.GetColumnIndexes(columnName);
				foreach (var idx in indexes)
				{
					result = HasDataValue(columnName, idx);
					if (result)
						break;
				}
			}

			return result;
		}

		/// <summary>
		/// Determines whether the column <c>name[index]</c> exists in the underlying data table 
		/// and contains a non-null, convertible value.
		/// </summary>
		/// <param name="columnName">The name of the column</param>
		/// <param name="index">The index of the column</param>
		/// <returns><c>true</c> if the column exists and contains a value, <c>false</c> otherwise</returns>
		/// <remarks>
		///		This method takes mapped column names into account.
		/// </remarks>
		public bool HasDataValue(string columnName, string index)
		{
			var mapping = _segmenter.ColumnMap.GetMapping(columnName, index);

			object value;
			return (_row.TryGetValue(mapping.MappedName, out value) && value != null && value != DBNull.Value);
		}

		public TProp GetDataValue<TProp>(string columnName, bool force = false)
		{
			TProp value;
			TryGetDataValue<TProp>(columnName, null, out value, force);
			return value;
		}

		public TProp GetDataValue<TProp>(string columnName, string index, bool force = false)
		{
			TProp value;
			TryGetDataValue<TProp>(columnName, index, out value, force);
			return value;
		}

		public bool TryGetDataValue<TProp>(string columnName, out TProp value, bool force = false)
		{
			return TryGetDataValue(columnName, null, out value, force);
		}

		public bool TryGetDataValue<TProp>(string columnName, string index, out TProp value, bool force = false)
		{
			var mapping = _segmenter.ColumnMap.GetMapping(columnName, index);

			if (!force && mapping.IgnoreProperty)
			{
				value = default(TProp);
				return false;
			}

			object rawValue;
			if (_row.TryGetValue(mapping.MappedName, out rawValue) && rawValue != null && rawValue != DBNull.Value && !rawValue.ToString().IsCaseInsensitiveEqual(ExplicitIgnore))
			{
				value = rawValue.ToString().IsCaseInsensitiveEqual(ExplicitNull) 
					? default(TProp) 
					: rawValue.Convert<TProp>(_segmenter.Culture);
				return true;
			}

			if (IsNew)
			{
				// only transient/new entities should fallback to possible defaults.
				value = GetDefaultValue(mapping, default(TProp));
				return true;
			}

			value = default(TProp);
			return false;
		}

		public bool SetProperty<TProp>(
			ImportResult result,
			Expression<Func<T, TProp>> prop,
			TProp defaultValue = default(TProp),
			Func<object, CultureInfo, TProp> converter = null)
		{
			return SetProperty(
				result,
				null, // columnName
				prop, 
				defaultValue, 
				converter);
		}

		public bool SetProperty<TProp>(
			ImportResult result,
			string columnName,
			Expression<Func<T, TProp>> prop,
			TProp defaultValue = default(TProp),
			Func<object, CultureInfo, TProp> converter = null)
		{
			// TBD: (MC) do not check or validate for perf reason?
			//CheckInitialized();

			var isPropertySet = false;
			var pi = prop.ExtractPropertyInfo();
			var propName = pi.Name;
			var target = _entity;

			columnName = columnName ?? propName;

			try
			{
				object value;
				var mapping = _segmenter.ColumnMap.GetMapping(columnName);

				if (mapping.IgnoreProperty)
				{
					// explicitly ignore this property
				}
				else if (_row.TryGetValue(mapping.MappedName, out value) && value != null && value != DBNull.Value && !value.ToString().IsCaseInsensitiveEqual(ExplicitIgnore))
				{
					// source contains field value. Set it.
					TProp converted;
					if (converter != null)
					{
						converted = converter(value, _segmenter.Culture);
					}
					else if (value.ToString().IsCaseInsensitiveEqual(ExplicitNull))
					{
						// prop is "explicitly" set to null. Don't fallback to any default!
						converted = default(TProp);
					}
					else
					{
						converted = value.Convert<TProp>(_segmenter.Culture);
					}

					var fastProp = FastProperty.GetProperty(target.GetUnproxiedType(), propName, PropertyCachingStrategy.EagerCached);
					fastProp.SetValue(target, converted);
					isPropertySet = true;
				}
				else
				{
					// source field value does not exist or is null/empty
					if (IsNew)
					{
						// if entity is new and source field value is null, determine default value in this particular order: 
						//		2.) Default value in field mapping table
						//		3.) passed default value argument
						defaultValue = GetDefaultValue(mapping, defaultValue, result);

						// source does not contain field data or is empty...
						if (defaultValue != null)
						{
							// ...but the entity is new. In this case set the default value if given.
							var fastProp = FastProperty.GetProperty(target.GetUnproxiedType(), propName, PropertyCachingStrategy.EagerCached);
							fastProp.SetValue(target, defaultValue);
							isPropertySet = true;
						}
					}
				}
			}
			catch (Exception exception)
			{
				result.AddWarning("Conversion failed: " + exception.Message, this.GetRowInfo(), propName);
			}

			if (isPropertySet && !_isDirty)
			{
				_isDirty = true;
			}

			return isPropertySet;
		}

		public ImportRowInfo GetRowInfo()
		{
			if (_rowInfo == null)
			{
				_rowInfo = new ImportRowInfo(this.Position, this.EntityDisplayName);
			}

			return _rowInfo;
		}

		public override string ToString()
		{
			var str = "Pos: {0} - Name: {1}, IsNew: {2}, IsTransient: {3}".FormatCurrent(
				Position,
				EntityDisplayName.EmptyNull(),
				_initialized ? IsNew.ToString() : "-",
				_initialized ? IsTransient.ToString() : "-");
			return str;
		}
	}
}
