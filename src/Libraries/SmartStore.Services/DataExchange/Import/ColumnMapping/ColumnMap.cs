using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Services.DataExchange.Import
{
	public class ColumnMap
	{
		// maps source column to property
		private readonly Dictionary<string, ColumnMappingValue> _map = new Dictionary<string, ColumnMappingValue>(StringComparer.OrdinalIgnoreCase);

		private static bool IsIndexed(string name)
		{
			return (name.EmptyNull().EndsWith("]") && name.EmptyNull().Contains("["));
		}

		private static string CreateSourceName(string name, string index)
		{
			if (index.HasValue())
			{
				name += String.Concat("[", index, "]");
			}

			return name;
		}

		public IReadOnlyDictionary<string, ColumnMappingValue> Mappings
		{
			get { return _map; }
		}

		public static bool ParseSourceColumn(string sourceColumn, out string columnWithoutIndex, out string index)
		{
			columnWithoutIndex = sourceColumn;
			index = null;

			var result = true;

			if (sourceColumn.HasValue() && IsIndexed(sourceColumn))
			{
				var x1 = sourceColumn.IndexOf('[');
				var x2 = sourceColumn.IndexOf(']', x1);

				if (x1 != -1 && x2 != -1 && x2 > x1)
				{
					columnWithoutIndex = sourceColumn.Substring(0, x1);
					index = sourceColumn.Substring(x1 + 1, x2 - x1 - 1);
				}
				else
				{
					result = false;
				}
			}

			return result;
		}

		//public IEnumerable<KeyValuePair<string, ColumnMappingValue>> GetInvalidMappings()
		//{
		//	var mappings = Mappings.Where(x => 
		//		x.Value.Property.HasValue() &&
		//		Mappings.Count(y => y.Value.Property.IsCaseInsensitiveEqual(x.Value.Property)) > 1
		//	);

		//	return mappings;
		//}

		public bool AddMapping(string sourceColumn, string entityProperty, string defaultValue = null)
		{
			return AddMapping(sourceColumn, null, entityProperty, defaultValue);
        }

		public bool AddMapping(string sourceColumn, string index, string entityProperty, string defaultValue = null)
		{
			Guard.ArgumentNotEmpty(() => sourceColumn);

			var isAlreadyMapped = (entityProperty.HasValue() && _map.Any(x => x.Value.Property.IsCaseInsensitiveEqual(entityProperty)));

			if (isAlreadyMapped)
				return false;

			_map[CreateSourceName(sourceColumn, index)] = new ColumnMappingValue
			{
				Property = entityProperty,
				Default = defaultValue
			};

			return true;
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
				// a) source column and property are equal or b) property should be ignored
				return result;
			}

			var crossPair = _map.FirstOrDefault(x => x.Value.Property != null && x.Value.Property == sourceColumn);

			if (crossPair.Key.HasValue())
			{
				// source column and property are unequal
				return new ColumnMappingValue
				{
					Property = crossPair.Key,
					Default = (crossPair.Value != null ? crossPair.Value.Default : null)
				};
			}

			// there is no mapping at all
			return new ColumnMappingValue { Property = sourceColumn };
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
				return result.Property;
			}

			var crossPair = _map.FirstOrDefault(x => x.Value.Property == sourceColumn);

			if (crossPair.Key.HasValue())
			{
				return crossPair.Key;
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
	}


	public class ColumnMappingValue
	{
		/// <summary>
		/// The property name of the target entity
		/// </summary>
		public string Property { get; set; }

		/// <summary>
		/// An optional default value
		/// </summary>
		public string Default { get; set; }
	}
}
