using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.DataExchange
{
	public class ColumnMap
	{
		private readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public IReadOnlyDictionary<string, string> Mappings
		{
			get { return _map; }
		}

		public void AddMapping(string sourceName, string mappedName)
		{
			AddMapping(sourceName, null, mappedName);
        }

		public void AddMapping(string sourceName, string index, string mappedName)
		{
			Guard.ArgumentNotEmpty(() => sourceName);
			Guard.ArgumentNotEmpty(() => mappedName);

			if (index.HasValue())
			{
				sourceName += String.Concat("[", index, "]");
			}

			_map[CreateName(sourceName, index)] = mappedName;
		}

		/// <summary>
		/// Gets a mapped column name
		/// </summary>
		/// <param name="sourceName">The name of the column to get a mapped name for.</param>
		/// <returns>The mapped column name OR - if the name is unmapped - the passed <paramref name="sourceName"/></returns>
		public string GetMappedName(string sourceName)
		{
			string result;
			if (_map.TryGetValue(sourceName, out result))
			{
				return result;
			}

			return sourceName;
		}

		/// <summary>
		/// Gets a mapped column name
		/// </summary>
		/// <param name="sourceName">The name of the column to get a mapped name for.</param>
		/// <param name="index">The column index, e.g. a language code (de, en etc.)</param>
		/// <returns>The mapped column name OR - if the name is unmapped - the passed <paramref name="sourceName"/>[<paramref name="index"/>]</returns>
		public string GetMappedName(string sourceName, string index)
		{
			sourceName = CreateName(sourceName, index);

			string result;
			if (_map.TryGetValue(sourceName, out result))
			{
				return result;
			}

			return sourceName;
		}

		internal static string CreateName(string name, string index)
		{
			if (index.HasValue())
			{
				name += String.Concat("[", index, "]");
			}

			return name;
		}
	}
}
