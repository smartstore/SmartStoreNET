using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange
{
	public interface IExportSegmenter
	{
		int TotalRecords { get; }

		int TotalSegments { get; }

		int CurrentSegmentIndex { get; }

		bool ReadNextSegment();

		ICollection<ExpandoObject> CurrentSegment { get; }
	}


	public class ExportSegmenter<T> : IExportSegmenter where T : BaseEntity
	{
		private Func<int, IEnumerable<T>> _loadData;
		private Func<T, ExpandoObject> _convertData;
		private List<ExpandoObject> _currentSegment;
		private IPageable _pageable;
		private bool _isBeginOfFile;

		internal ExportSegmenter(Func<int, IEnumerable<T>> loadData, Func<T, ExpandoObject> convertData, int pageSize, int totalRecords)
		{
			_loadData = loadData;
			_convertData = convertData;

			_isBeginOfFile = true;
			_pageable = new PagedList(0, pageSize, totalRecords);
		}

		public int TotalRecords
		{
			get	{ return _pageable.TotalCount; }
		}

		public int TotalSegments
		{
			get { return _pageable.TotalPages; }
		}

		public int CurrentSegmentIndex
		{
			get { return _isBeginOfFile ? 0 : _pageable.PageNumber; }
		}

		public void Reset()
		{
			if (_pageable.PageIndex != 0 && _currentSegment != null)
			{
				_currentSegment.Clear();
				_currentSegment = null;
			}
			_isBeginOfFile = true;
			_pageable.PageIndex = 0;
		}

		public bool ReadNextSegment()
		{
			if (_currentSegment != null)
			{
				_currentSegment.Clear();
				_currentSegment = null;
			}

			if (_isBeginOfFile)
			{
				_isBeginOfFile = false;
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

		public ICollection<ExpandoObject> CurrentSegment
		{
			get
			{
				if (_currentSegment == null)
				{
					_currentSegment = new List<ExpandoObject>();

					var data = _loadData(_pageable.PageIndex);

					foreach (var item in data)
					{
						var expando = _convertData(item);
						_currentSegment.Add(expando);
					}
				}

				return _currentSegment.AsReadOnly();
			}
		}
	}
}
