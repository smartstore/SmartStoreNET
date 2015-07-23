using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SmartStore.Collections
{
	/// <summary>
	/// Structure that combines early and lazy data loading
	/// </summary>
	public class LazyMultimap<T> : Multimap<int, T>
	{
		private Func<int[], Multimap<int, T>> _load;
		private List<int> _loaded;		// to avoid database rountrips with empty results

		public LazyMultimap(Func<int[], Multimap<int, T>> load, IEnumerable<int> initial = null)
		{
			_load = load;
			_loaded = new List<int>();

			Load(initial);
		}

		public virtual ICollection<T> Ensure(int key)
		{
			if (!_loaded.Contains(key))
			{
				if (typeof(T).Name == "ProductVariantAttribute")
					"lazy load {0}".FormatInvariant(key).Dump();

				Load(new int[] { key });
			}

			// better not override indexer cause of stack overflow risk
			var result = base[key];

			Debug.Assert(_loaded.Contains(key), "Possible missing multimap result for key {0} and type {1}.".FormatInvariant(key, typeof(T).Name), "");

			return result;
		}

		protected virtual void Load(IEnumerable<int> keys)
		{
			if (keys != null)
			{
				var loadKeys = keys.Distinct().Except(_loaded).ToArray();

				if (loadKeys.Any())
				{
					var items = _load(loadKeys);

					//if (typeof(T).Name == "ProductVariantAttribute")
					//	"call load {0}".FormatInvariant(string.Join(";", loadKeys)).Dump();

					_loaded.AddRange(loadKeys);

					foreach (var range in items)
					{
						base.AddRange(range.Key, range.Value);
					}
				}
			}
		}
	}
}
