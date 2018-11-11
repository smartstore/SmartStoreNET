﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SmartStore.Collections
{
	/// <summary>
	/// Manages data keys and offers a combination of eager and lazy data loading
	/// </summary>
	public class LazyMultimap<T> : Multimap<int, T>
	{
		private readonly Func<int[], Multimap<int, T>> _load;
		private readonly List<int> _loaded;			// to avoid database round trips with empty results
		private List<int> _collect;
		//private int _roundTripCount;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="load"><para>int[]</para> keys like Entity.Id, <para>Multimap{int, T}></para> delegate to load data</param>
		/// <param name="collect">Keys of eager loaded data</param>
		public LazyMultimap(Func<int[], Multimap<int, T>> load, IEnumerable<int> collect = null)
		{
			_load = load;
			_loaded = new List<int>();

			_collect = collect == null ? new List<int>() : new List<int>(collect);
		}

		protected virtual void Load(IEnumerable<int> keys)
		{
			if (keys != null)
			{
				var loadKeys = (_collect.Count == 0 ? keys : _collect.Concat(keys))
					.Distinct()
					.Except(_loaded)
					.ToArray();

				_collect.Clear();	// invalidate, do not load again					

				if (loadKeys.Any())
				{
					//++_roundTripCount;
					//Debug.WriteLine("Round trip {0} of {1}: {2}", _roundTripCount, typeof(T).Name, string.Join(",", loadKeys.OrderBy(x => x)));

					var items = _load(loadKeys);

					_loaded.AddRange(loadKeys);

					if (items != null)
					{
						foreach (var range in items)
						{
							base.AddRange(range.Key, range.Value);
						}
					}
				}
			}
		}

		/// <summary>
		/// Get data. Load it if not already loaded yet.
		/// </summary>
		/// <param name="key">Data key</param>
		/// <returns>Collection of data</returns>
		public virtual ICollection<T> Load(int key)
		{
			if (key == 0)
			{
				return new List<T>();
			}

			if (!_loaded.Contains(key))
			{
				Load(new int[] { key });
			}

			// better not override indexer cause of stack overflow risk
			var result = base[key];

			Debug.Assert(_loaded.Contains(key), "Possible missing multimap result for key {0} and type {1}.".FormatInvariant(key, typeof(T).Name), "");

			return result;
		}

		/// <summary>
		/// Collect keys for combined loading
		/// </summary>
		/// <param name="keys">Data keys</param>
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public virtual void Collect(IEnumerable<int> keys)
		{
			if (keys != null && keys.Any())
			{
				_collect = _collect.Union(keys).ToList();
			}
		}

		/// <summary>
		/// Collect single key for combined loading
		/// </summary>
		/// <param name="key">Data key</param>
		public virtual void Collect(int key)
		{
			if (key != 0 && !_collect.Contains(key))
			{
				_collect.Add(key);
			}
		}

		public override void Clear()
		{
			_loaded.Clear();
			_collect.Clear();
			//_roundTripCount = 0;

			base.Clear();
		}
	}
}
