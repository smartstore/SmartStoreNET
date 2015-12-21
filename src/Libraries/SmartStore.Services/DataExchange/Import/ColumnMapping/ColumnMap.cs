using System;
using System.Collections.Generic;

namespace SmartStore.Services.DataExchange.Import
{
	public class ColumnMap
	{
		private readonly Dictionary<string, ColumnMappingValue> _map = new Dictionary<string, ColumnMappingValue>(StringComparer.OrdinalIgnoreCase);

		public IReadOnlyDictionary<string, ColumnMappingValue> Mappings
		{
			get { return _map; }
		}

		public void AddMapping(string sourceColumn, string entityProperty, string defaultValue = null)
		{
			AddMapping(sourceColumn, null, entityProperty, defaultValue);
        }

		public void AddMapping(string sourceColumn, string index, string entityProperty, string defaultValue = null)
		{
			Guard.ArgumentNotEmpty(() => sourceColumn);
			Guard.ArgumentNotEmpty(() => entityProperty);

			_map[CreateSourceName(sourceColumn, index)] = new ColumnMappingValue
			{
				EntityProperty = entityProperty,
				DefaultValue = defaultValue
			};
		}

		/// <summary>
		/// Gets a mapped column value
		/// </summary>
		/// <param name="sourceColumn">The name of the column to get a mapped value for.</param>
		/// <returns>The mapped column value OR - if the name is unmapped - a value with the passed <paramref name="sourceColumn"/></returns>
		public ColumnMappingValue GetMapping(string sourceColumn)
		{
			ColumnMappingValue result;

			if (_map.TryGetValue(sourceColumn, out result))
			{
				return result;
			}

			return new ColumnMappingValue { EntityProperty = sourceColumn };
		}

		/// <summary>
		/// Gets a mapped column value
		/// </summary>
		/// <param name="sourceColumn">The name of the column to get a mapped value for.</param>
		/// <param name="index">The column index, e.g. a language code (de, en etc.)</param>
		/// <returns>The mapped column value OR - if the name is unmapped - a value with the passed <paramref name="sourceColumn"/>[<paramref name="index"/>]</returns>
		public ColumnMappingValue GetMapping(string sourceColumn, string index)
		{
			return GetMapping(CreateSourceName(sourceColumn, index));
		}

		/// <summary>
		/// Gets a mapped property name
		/// </summary>
		/// <param name="sourceColumn">The name of the column to get a mapped property name for.</param>
		/// <returns>The mapped property name OR - if the name is unmapped - the passed <paramref name="sourceColumn"/>[<paramref name="index"/>]</returns>
		public string GetMappedProperty(string sourceColumn)
		{
			ColumnMappingValue result;

			if (_map.TryGetValue(sourceColumn, out result))
			{
				return result.EntityProperty;
			}

			return sourceColumn;
		}

		/// <summary>
		/// Gets a mapped property name
		/// </summary>
		/// <param name="sourceColumn">The name of the column to get a mapped property name for.</param>
		/// <param name="index">The column index, e.g. a language code (de, en etc.)</param>
		/// <returns>The mapped property name OR - if the name is unmapped - the passed <paramref name="sourceColumn"/>[<paramref name="index"/>]</returns>
		public string GetMappedProperty(string sourceColumn, string index)
		{
			return GetMappedProperty(CreateSourceName(sourceColumn, index));
		}

		internal static string CreateSourceName(string name, string index)
		{
			if (index.HasValue())
			{
				name += String.Concat("[", index, "]");
			}

			return name;
		}
	}


	public class ColumnMappingValue
	{
		/// <summary>
		/// The property name of the target entity
		/// </summary>
		public string EntityProperty { get; set; }

		/// <summary>
		/// An optional default value
		/// </summary>
		public string DefaultValue { get; set; }
	}
}
