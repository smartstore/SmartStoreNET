using System;
using System.Collections.Generic;

namespace SmartStore.Services.DataExchange
{
	public class ColumnMap
	{
		private readonly Dictionary<string, ColumnMappingValue> _map = new Dictionary<string, ColumnMappingValue>(StringComparer.OrdinalIgnoreCase);

		public IReadOnlyDictionary<string, ColumnMappingValue> Mappings
		{
			get { return _map; }
		}

		public void AddMapping(string sourceName, string entityProperty, string defaultValue = null)
		{
			AddMapping(sourceName, null, entityProperty, defaultValue);
        }

		public void AddMapping(string sourceName, string index, string entityProperty, string defaultValue = null)
		{
			Guard.ArgumentNotEmpty(() => sourceName);
			Guard.ArgumentNotEmpty(() => entityProperty);

			_map[CreateSourceName(sourceName, index)] = new ColumnMappingValue
			{
				EntityProperty = entityProperty,
				DefaultValue = defaultValue
			};
		}

		/// <summary>
		/// Gets a mapped column value
		/// </summary>
		/// <param name="sourceName">The name of the column to get a mapped value for.</param>
		/// <returns>The mapped column name OR - if the name is unmapped - a value with the passed <paramref name="sourceName"/></returns>
		public ColumnMappingValue GetMapping(string sourceName)
		{
			ColumnMappingValue result;

			if (_map.TryGetValue(sourceName, out result))
			{
				return result;
			}

			return new ColumnMappingValue { EntityProperty = sourceName };
		}

		/// <summary>
		/// Gets a mapped column value
		/// </summary>
		/// <param name="sourceName">The name of the column to get a mapped value for.</param>
		/// <param name="index">The column index, e.g. a language code (de, en etc.)</param>
		/// <returns>The mapped column name OR - if the name is unmapped - a value with the passed <paramref name="sourceName"/>[<paramref name="index"/>]</returns>
		public ColumnMappingValue GetMapping(string sourceName, string index)
		{
			return GetMapping(CreateSourceName(sourceName, index));
		}

		public string GetMappedProperty(string sourceName)
		{
			ColumnMappingValue result;

			if (_map.TryGetValue(sourceName, out result))
			{
				return result.EntityProperty;
			}

			return sourceName;
		}

		public string GetMappedProperty(string sourceName, string index)
		{
			return GetMappedProperty(CreateSourceName(sourceName, index));
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
		public string EntityProperty { get; set; }

		public string DefaultValue { get; set; }
	}
}
