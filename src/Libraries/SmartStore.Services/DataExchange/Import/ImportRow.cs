using System;
using System.Globalization;
using System.Linq.Expressions;
using SmartStore.ComponentModel;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange.Import
{
	public class ImportRow<T> where T : BaseEntity
	{
		private bool _initialized = false;
		private T _entity;
		private string _entityDisplayName;
		private readonly int _position;
		private bool _isNew;
		private ImportRowInfo _rowInfo;

		private readonly ImportDataSegmenter<T> _segmenter;
		private readonly IDataRow _row;

		private TProp GetDefaultValue<TProp>(ColumnMappingValue mapping, string columnName, TProp defaultValue, ImportResult result = null)
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
						result.AddWarning(msg.FormatInvariant(mapping.Default, exception.Message), this.GetRowInfo(), columnName);
					}
				}
			}

			return defaultValue;
		}

		private void CheckInitialized()
		{
			if (_initialized)
			{
				throw Error.InvalidOperation("A row must be initialized before interacting with the entity or the data store");
			}
		}

		public ImportRow(ImportDataSegmenter<T> parent, IDataRow row, int position)
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

		public bool IsTransient
		{
			get { return _entity.Id == 0; }
		}

		public bool IsNew
		{
			get { return _isNew; }
		}

		public ImportDataSegmenter<T> Segmenter
		{
			get { return _segmenter; }
		}

		public T Entity
		{
			get { return _entity; }
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

		public bool HasDataValue(string columnName)
		{
			return HasDataValue(columnName, null);
		}

		public bool HasDataValue(string columnName, string index)
		{
			object value;
			var mapping = _segmenter.ColumnMap.GetMapping(columnName, index);
			
			return (_row.TryGetValue(mapping.Property, out value) && value != null && value != DBNull.Value);
		}

		public TProp GetDataValue<TProp>(string columnName)
		{
			return GetDataValue<TProp>(columnName, null);
		}

		public TProp GetDataValue<TProp>(string columnName, string index)
		{
			object value;
			var mapping = _segmenter.ColumnMap.GetMapping(columnName, index);

			if (_row.TryGetValue(mapping.Property, out value) && value != null && value != DBNull.Value)
			{
				return value.Convert<TProp>(_segmenter.Culture);
			}

			if (IsTransient)
			{
				// only transient/new entities should fallback to possible defaults.
				return GetDefaultValue(mapping, columnName, default(TProp));
			}

			return default(TProp);
		}

		public bool SetProperty<TProp>(
			ImportResult result,
			T target,
			Expression<Func<T, TProp>> prop,
			TProp defaultValue = default(TProp),
			Func<object, CultureInfo, TProp> converter = null)
		{
			// TBD: (MC) do not check for perf reason?
			//CheckInitialized();

			var isPropertySet = false;
			var pi = prop.ExtractPropertyInfo();
			var propName = pi.Name;

			try
			{
				object value;
				var mapping = _segmenter.ColumnMap.GetMapping(propName);

				if (mapping.IgnoreProperty)
				{
					// explicitly ignore this property
				}
				else if (_row.TryGetValue(mapping.Property, out value) && (value != null && value != DBNull.Value))
				{
					// source contains field value. Set it.
					TProp converted;
					if (converter != null)
					{
						converted = converter(value, _segmenter.Culture);
					}
					else if (value.ToString().IsCaseInsensitiveEqual("[NULL]"))
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
					if (IsTransient)
					{
						// if entity is new and source field value is null, determine default value in this particular order: 
						//		2.) Default value in field mapping table
						//		3.) passed default value argument
						defaultValue = GetDefaultValue(mapping, propName, defaultValue, result);

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
