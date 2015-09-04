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

		int TotalRecords { get; }

		int TotalSegments { get; }

		int SegmentIndex { get; }
		
		int FileIndex { get; }

		bool ReadNextSegment();

		ICollection<ExpandoObject> CurrentSegment { get; }
	}

	internal interface IExportExecuter
	{
		void Start(Func<bool> callback);
		void Dispose();
	}


	public class ExportSegmenter<T> : IExportSegmenter, IExportExecuter, IDisposable where T : BaseEntity
	{
		private Func<int, List<T>> _loadData;
		private Func<T, List<T>, List<ExpandoObject>> _convertData;
		private List<T> _currentData;
		private List<ExpandoObject> _currentSegment;
		private IPageable _pageable;
		private int _pageIndexReset;
		private bool _isBeginOfSegmentation;

		private int _recordsPerFile;
		private int _fileIndex;
		private int _fileRecordsSkip;
		private bool _fileCaptured;

		private int _recordsPerFileCount;
		private int _recordsCount;

		public ExportSegmenter(
			Func<int, List<T>> loadData,
			Func<T, List<T>, List<ExpandoObject>> convertData,
			PagedList pageable,
			int itemsPerFile)
		{
			_loadData = loadData;
			_convertData = convertData;
			_pageable = pageable;
			_recordsPerFile = itemsPerFile;

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

			if (_currentData != null)
			{
				_currentData.Clear();
				_currentData = null;
			}
		}

		public void Dispose()
		{
			ClearSegment();
		}

		public int ProcessedItems
		{
			get { return _recordsCount; }
		}

		public int TotalRecords
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
			_fileRecordsSkip = 0;
			_fileCaptured = false;
			_recordsPerFileCount = 0;
		}

		public bool ReadNextSegment()
		{
			ClearSegment();

			if (_isBeginOfSegmentation)
			{
				_isBeginOfSegmentation = false;
				_recordsCount = 0;
				return _pageable.TotalCount > 0;
			}

			if (_fileCaptured)
			{
				return false;
			}

			if (_fileRecordsSkip > 0)	// read rest of last segment for new file
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

					var tmpCount = 0;
					_currentData = _loadData(_pageable.PageIndex);

					foreach (var record in _currentData.Skip(_fileRecordsSkip))
					{
						if (_recordsPerFile > 0 && _recordsPerFileCount >= _recordsPerFile)
						{
							_fileCaptured = true;
							_fileIndex++;
							_recordsPerFileCount = 0;
							_fileRecordsSkip = tmpCount;

							return _currentSegment.AsReadOnly();
						}

						foreach (var obj in _convertData(record, _currentData))
						{
							_currentSegment.Add(obj);

							++_recordsPerFileCount;
							++_recordsCount;
						}

						++tmpCount;
					}

					_fileRecordsSkip = 0;
				}

				return _currentSegment.AsReadOnly();
			}
		}
	}
}
