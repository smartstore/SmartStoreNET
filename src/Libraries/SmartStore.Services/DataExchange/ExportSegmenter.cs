using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange
{
	public interface IExportSegmenterConsumer
	{
		/// <summary>
		/// Total number of records
		/// </summary>
		int RecordTotal { get; }

		/// <summary>
		/// Gets current data segment
		/// </summary>
		List<dynamic> CurrentSegment { get; }

		/// <summary>
		/// Reads the next segment
		/// </summary>
		/// <returns></returns>
		bool ReadNextSegment();
	}

	internal interface IExportSegmenterProvider : IExportSegmenterConsumer, IDisposable
	{
		/// <summary>
		/// Whether there is segmented data available
		/// </summary>
		bool HasData { get; }

		/// <summary>
		/// Record per segment counter
		/// </summary>
		int RecordPerSegmentCount { get; set; }
	}

	public class ExportSegmenter<T> : IExportSegmenterProvider where T : BaseEntity
	{
		private Func<int, List<T>> _load;
		private Action<ICollection<T>> _loaded;
		private Func<T, List<dynamic>> _convert;

		private int _offset;
		private int _limit;
		private int _recordsPerSegment;

		private bool _isDataLoaded;
		private int _countRecords;

		private IPageable _pages;
		private Queue<T> _data;

		public ExportSegmenter(
			Func<int, List<T>> load,
			Action<ICollection<T>> loaded,
			Func<T, List<dynamic>> convert,
			PagedList pages,
			int offset,
			int limit,
			int recordsPerSegment)
		{
			_load = load;
			_loaded = loaded;
			_convert = convert;
			_pages = pages;
			_offset = offset;
			_limit = limit;
			_recordsPerSegment = recordsPerSegment;
		}

		/// <summary>
		/// Whether there is segmented data available
		/// </summary>
		public bool HasData
		{
			get
			{
				if (RecordCount >= _limit && _limit > 0)
					return false;

				if (_data != null && _data.Count > 0)
					return true;

				if (_data == null && _pages.IsFirstPage)
					return true;

				if (_pages.HasNextPage)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Record per segment counter
		/// </summary>
		public int RecordPerSegmentCount { get; set; }

		/// <summary>
		/// Number of processed records
		/// </summary>
		public int RecordCount
		{
			get { return _countRecords - _offset; }
		}

		/// <summary>
		/// Total number of records
		/// </summary>
		public int RecordTotal
		{
			get
			{
				if (_limit != 0 && _limit < _pages.TotalCount)
					return _limit;

				return _pages.TotalCount - _offset;
			}
		}

		/// <summary>
		/// Gets current data segment
		/// </summary>
		public List<dynamic> CurrentSegment
		{
			get
			{
				var records = new List<dynamic>();

				while (_data.Count > 0)
				{
					var entity = _data.Dequeue();
					var skip = (++_countRecords < _offset && _offset > 0);

					if (!skip)
					{
						foreach (var record in _convert(entity))
						{
							records.Add(record);
						}

						if (++RecordPerSegmentCount >= _recordsPerSegment && _recordsPerSegment > 0)
							return records;
					}

					if (RecordCount >= _limit && _limit > 0)
						return records;
				}

				return records;
			}
		}

		/// <summary>
		/// Reads the next segment
		/// </summary>
		/// <returns></returns>
		public bool ReadNextSegment()
		{
			if (RecordCount >= _limit && _limit > 0)
				return false;

			if (RecordPerSegmentCount >= _recordsPerSegment && _recordsPerSegment > 0)
				return false;

			// do not make the queue longer than necessary
			if (_recordsPerSegment > 0 && _data != null && _data.Count >= _recordsPerSegment)
				return true;

			if (_isDataLoaded)
			{
				if (!_pages.HasNextPage)
					return (_data != null && _data.Count > 0);

				++_pages.PageIndex;
			}
			else
			{
				_isDataLoaded = true;
			}

			if (_data != null && _data.Count > 0)
			{
				var data = new List<T>(_data);
				data.AddRange(_load(_pages.PageIndex));

				_data = new Queue<T>(data);
			}
			else
			{
				_data = new Queue<T>(_load(_pages.PageIndex));
			}

			// give provider the opportunity to make something with entity ids
			if (_loaded != null)
			{
				_loaded(_data.AsReadOnly());
			}

			return (_data.Count > 0);
		}

		/// <summary>
		/// Dispose and reset segmenter instance
		/// </summary>
		public void Dispose()
		{
			if (_data != null)
				_data.Clear();

			_isDataLoaded = false;
			_countRecords = 0;
			RecordPerSegmentCount = 0;
		}
	}
}
