using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange.Import
{
	public class ImportDataSegmenter<T> where T : BaseEntity
	{
		private const int BATCHSIZE = 100;

		private readonly IDataTable _table;
		private ImportRow<T>[] _currentBatch;
		private readonly IPageable _pageable;
		private bool _bof;
		private CultureInfo _culture;
		private ColumnMap _columnMap;

		private readonly IDictionary<string, string[]> _columnIndexes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

		public ImportDataSegmenter(IDataTable table, ColumnMap map)
		{
			Guard.ArgumentNotNull(() => table);
			Guard.ArgumentNotNull(() => map);

			_table = table;
			_columnMap = map;

			_bof = true;
			_pageable = new PagedList(0, BATCHSIZE, table.Rows.Count);
			_culture = CultureInfo.InvariantCulture;
        }

		public CultureInfo Culture
		{
			get
			{
				return _culture;
			}
			set
			{
				_culture = value ?? CultureInfo.InvariantCulture;
			}
		}

		public ColumnMap ColumnMap
		{
			get
			{
				return _columnMap;
			}
			set
			{
				_columnMap = value ?? new ColumnMap();
			}
		}

		public int TotalRows
		{
			get { return _table.Rows.Count; }
		}

		public int TotalColumns
		{
			get { return _table.Columns.Count; }
		}

		public int CurrentSegment
		{
			get { return _bof ? 0 : _pageable.PageNumber; }
		}

		public int CurrentSegmentFirstRowIndex
		{
			get { return _pageable.FirstItemIndex; }
		}

		public int TotalSegments
		{
			get { return _pageable.TotalPages; }
		}

		public int BatchSize
		{
			get { return BATCHSIZE; }
		}

		public bool HasColumn(string name)
		{
			return _table.HasColumn(_columnMap.GetMappedProperty(name));
		}

		public bool HasColumn(string name, string index)
		{
			return _table.HasColumn(_columnMap.GetMappedProperty(name, index));
		}

		/// <summary>
		/// Returns an array of exisiting index names for a column
		/// </summary>
		/// <param name="name">The name of the columns without index qualification</param>
		/// <returns>An array of index names</returns>
		/// <remarks>
		/// If following columns exist in source: Attr[Color], Attr[Size]
		/// This method returns: <code>string[] { "Color", "Size" }</code> 
		/// </remarks>
		public string[] GetColumnIndexes(string name)
		{
			string[] indexes;

			if (!_columnIndexes.TryGetValue(name, out indexes))
			{
				var startsWith = name + "[";

				var columns1 = _columnMap.Mappings
					.Where(x => x.Key.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
					.Select(x => x.Key);

				var columns2 = _table.Columns
					.Where(x => x.Name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
					.Select(x => x.Name);

				indexes = columns1.Concat(columns2)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.Select(x => x.Substring(x.IndexOf("[", StringComparison.OrdinalIgnoreCase) + 1).TrimEnd(']'))
					.ToArray();

				_columnIndexes[name] = indexes;
			}

			return indexes;
		} 

		public void Reset()
		{
			if (_pageable.PageIndex != 0 && _currentBatch != null)
			{
				_currentBatch = null;
			}
			_bof = true;
			_pageable.PageIndex = 0;
		}

		public bool ReadNextBatch()
		{
			if (_currentBatch != null)
			{
				_currentBatch = null;
			}

			if (_bof)
			{
				_bof = false;
				return _pageable.TotalCount > 0;
			}

			if (_pageable.HasNextPage)
			{
				_pageable.PageIndex++;
				return true;
			}

			Reset();
			return false;
		}

		public ImportRow<T>[] CurrentBatch
		{
			get
			{
				if (_currentBatch == null)
				{
					int start = _pageable.FirstItemIndex - 1;
					int end = _pageable.LastItemIndex - 1;

					_currentBatch = new ImportRow<T>[(end - start) + 1];

					// Determine values per row
					int i = 0;
					for (int r = start; r <= end; r++)
					{
						_currentBatch[i] = new ImportRow<T>(this, _table.Rows[r], r);
						i++;
					}
				}

				return _currentBatch;
			}
		}
	}
}
