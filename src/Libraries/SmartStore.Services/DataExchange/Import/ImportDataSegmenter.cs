﻿using System;
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

		private IDataTable _table;
		private ImportRow<T>[] _currentBatch;
		private IPageable _pageable;
		private bool _bof;
		private CultureInfo _culture;
		private ColumnMap _columnMap;

		public ImportDataSegmenter(IDataTable table)
		{
			Guard.ArgumentNotNull(() => table);

			_table = table;

			_bof = true;
			_pageable = new PagedList(0, BATCHSIZE, table.Rows.Count);
			_culture = CultureInfo.InvariantCulture;
			_columnMap = new ColumnMap();
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

		public bool HasDataColumn(string name)
		{
			return _table.HasColumn(_columnMap.GetMappedName(name));
		}

		public bool HasDataColumn(string name, string index)
		{
			return _table.HasColumn(_columnMap.GetMappedName(name, index));
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
