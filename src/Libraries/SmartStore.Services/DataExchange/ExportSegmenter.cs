using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange
{
	public interface IExportSegmenter
	{
		int ProcessedItems { get; }

		int TotalItems { get; }

		int TotalSegments { get; }

		int SegmentIndex { get; }
		
		int FileIndex { get; }

		bool ReadNextSegment();

		ICollection<ExpandoObject> CurrentSegment { get; }
	}


	public class ExportSegmenter : IExportSegmenter, IDisposable
	{
		private Func<int, IEnumerable<object>> _loadData;
		private Func<object, IEnumerable<object>, ExpandoObject> _convertData;
		private List<ExpandoObject> _currentSegment;
		private IPageable _pageable;
		private int _pageIndexReset;
		private bool _isBeginOfSegmentation;

		private int _itemsPerFile;
		private int _fileIndex;
		private int _fileItemsSkip;
		private bool _fileCaptured;

		private int _itemsPerFileCount;
		private int _itemsCount;

		public ExportSegmenter(
			Func<int, IEnumerable<object>> loadData,
			Func<object, IEnumerable<object>, ExpandoObject> convertData,
			PagedList pageable,
			int itemsPerFile)
		{
			_loadData = loadData;
			_convertData = convertData;
			_pageable = pageable;
			_itemsPerFile = itemsPerFile;

			_pageIndexReset = pageable.PageIndex;
			_isBeginOfSegmentation = true;
		}

		protected void ClearSegment()
		{
			if (_currentSegment != null)
			{
				_currentSegment.Clear();
				_currentSegment = null;
			}
		}

		public void Dispose()
		{
			ClearSegment();
		}

		public int ProcessedItems
		{
			get { return _itemsCount; }
		}

		public int TotalItems
		{
			get	{ return _pageable.TotalCount; }
		}

		public int TotalSegments
		{
			get { return _pageable.TotalPages; }
		}

		public int SegmentIndex
		{
			get { return _isBeginOfSegmentation ? 0 : _pageable.PageNumber; }
		}

		public int FileIndex
		{
			get { return _fileIndex; }
		}

		public void Start(Func<bool> callback)
		{
			for (int oldFileIndex = 0; oldFileIndex < 9999999; ++oldFileIndex)
			{
				_fileCaptured = false;

				if (!callback())
					break;

				if (_fileIndex <= oldFileIndex)
					break;
			}

			Dispose();
		}

		public void Reset()
		{
			//if (_pageable.PageIndex != 0 && _currentSegment != null)
			//{
			//	_currentSegment.Clear();
			//	_currentSegment = null;
			//}

			ClearSegment();

			_isBeginOfSegmentation = true;
			_pageable.PageIndex = _pageIndexReset;
			_fileIndex = 0;
			_fileItemsSkip = 0;
			_fileCaptured = false;
			_itemsPerFileCount = 0;
		}

		public bool ReadNextSegment()
		{
			ClearSegment();

			if (_isBeginOfSegmentation)
			{
				_isBeginOfSegmentation = false;
				//_itemsCount = 0;
				return _pageable.TotalCount > 0;
			}

			if (_fileCaptured)
			{
				return false;
			}

			if (_fileItemsSkip > 0)
			{
				return true;
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

					foreach (var item in data.Skip(_fileItemsSkip))
					{
						if (_itemsPerFile > 0 && _itemsPerFileCount >= _itemsPerFile)
						{
							_fileCaptured = true;
							_fileIndex++;
							_itemsPerFileCount = 0;
							_fileItemsSkip = _currentSegment.Count;

							return _currentSegment.AsReadOnly();
						}

						var expando = _convertData(item, data);

						_currentSegment.Add(expando);

						++_itemsPerFileCount;
						++_itemsCount;
					}

					_fileItemsSkip = 0;
				}

				return _currentSegment.AsReadOnly();
			}
		}
	}
}
