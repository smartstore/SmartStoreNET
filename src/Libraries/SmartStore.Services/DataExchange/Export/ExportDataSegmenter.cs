using System;
using System.Collections.Generic;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange.Export
{
	public interface IExportDataSegmenterConsumer
	{
		/// <summary>
		/// Total number of records
		/// </summary>
		int TotalRecords { get; }

		/// <summary>
		/// Gets current data segment
		/// </summary>
		IReadOnlyCollection<dynamic> CurrentSegment { get; }

		/// <summary>
		/// Reads the next segment
		/// </summary>
		/// <returns></returns>
		bool ReadNextSegment();
	}

	internal interface IExportDataSegmenterProvider : IExportDataSegmenterConsumer, IDisposable
	{
		/// <summary>
		/// Whether there is data available
		/// </summary>
		bool HasData { get; }

		/// <summary>
		/// Record per segment counter
		/// </summary>
		int RecordPerSegmentCount { get; set; }
	}

	public class ExportDataSegmenter<T> : IExportDataSegmenterProvider where T : BaseEntity
	{
		private Func<int, List<T>> _load;
		private Action<ICollection<T>> _loaded;
		private Func<T, List<dynamic>> _convert;

		private int _offset;
		private int _take;
		private int _limit;
		private int _recordsPerSegment;
		private int _totalRecords;

		private int _skip;
		private int _countRecords;

		private Queue<T> _data;

		public ExportDataSegmenter(
			Func<int, List<T>> load,
			Action<ICollection<T>> loaded,
			Func<T, List<dynamic>> convert,
			int offset,
			int take,
			int limit,
			int recordsPerSegment,
			int totalRecords)
		{
			_load = load;
			_loaded = loaded;
			_convert = convert;
			_offset = offset;
			_take = take;
			_limit = limit;
			_recordsPerSegment = recordsPerSegment;
			_totalRecords = totalRecords;

			_skip = _offset;
		}

		/// <summary>
		/// Total number of records
		/// </summary>
		public int TotalRecords
		{
			get
			{
				var total = Math.Max(_totalRecords - _offset, 0);

				if (_limit > 0 && _limit < total)
					return _limit;

				return total;
			}
		}

		/// <summary>
		/// Number of processed records
		/// </summary>
		public int RecordCount
		{
			get { return _countRecords; }
		}

		/// <summary>
		/// Record per segment counter
		/// </summary>
		public int RecordPerSegmentCount { get; set; }

		/// <summary>
		/// Whether there is data available
		/// </summary>
		public bool HasData
		{
			get
			{
				if (_limit > 0 && _countRecords >= _limit)
					return false;

				if (_data != null && _data.Count > 0)
					return true;

				if (_skip >= _totalRecords)
					return false;

				return true;
			}
		}

		/// <summary>
		/// Gets current data segment
		/// </summary>
		public IReadOnlyCollection<dynamic> CurrentSegment
		{
			get
			{
				T entity;
				var records = new List<dynamic>();

				while (_data.Count > 0 && (entity = _data.Dequeue()) != null)
				{
					_convert(entity).Each(x => records.Add(x));

					if (++_countRecords >= _limit && _limit > 0)
						return records;

					if (++RecordPerSegmentCount >= _recordsPerSegment && _recordsPerSegment > 0)
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
			if (_limit > 0 && _countRecords >= _limit)
				return false;

			if (_recordsPerSegment > 0 && RecordPerSegmentCount >= _recordsPerSegment)
				return false;

			// do not make the queue longer than necessary
			if (_recordsPerSegment > 0 && _data != null && _data.Count >= _recordsPerSegment)
				return true;

			if (_skip >= _totalRecords)
				return false;

			if (_data != null)
				_skip += _take;

			if (_data != null && _data.Count > 0)
			{
				var data = new List<T>(_data);
				data.AddRange(_load(_skip));

				_data = new Queue<T>(data);
			}
			else
			{
				_data = new Queue<T>(_load(_skip));
			}

			// give provider the opportunity to make something with entity ids
			_loaded?.Invoke(_data.AsReadOnly());

			return (_data.Count > 0);
		}

		/// <summary>
		/// Dispose and reset segmenter instance
		/// </summary>
		public void Dispose()
		{
			if (_data != null)
				_data.Clear();

			_skip = _offset;
			_countRecords = 0;
			RecordPerSegmentCount = 0;
		}
	}
}
