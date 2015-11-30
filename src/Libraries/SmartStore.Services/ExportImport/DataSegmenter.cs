using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Fasterflect;
using OfficeOpenXml;
using SmartStore.Core;
using SmartStore.Core.Data;

namespace SmartStore.Services.ExportImport
{

	internal class DataSegmenter<T> : DisposableObject where T : BaseEntity
	{
		private const int BATCHSIZE = 100;

		private ExcelPackage _excelPackage;
		private ExcelWorksheet _sheet;
		private int _totalRows;
		private int _totalColumns;
		private readonly string[] _columns;
		private readonly IDictionary<string, TargetProperty> _properties;
		private IList<ImportRow<T>> _currentBatch;
		private IPageable _pageable;
		private bool _bof;

		public DataSegmenter(Stream source)
		{
			Guard.ArgumentNotNull(() => source);

			_excelPackage = new ExcelPackage(source);

			// get the first worksheet in the workbook
			_sheet = _excelPackage.Workbook.Worksheets.FirstOrDefault();
			if (_sheet == null)
			{
				throw Error.InvalidOperation("The excel package does not contain any worksheet.");
			}

			if (_sheet.Dimension == null)
			{
				throw Error.InvalidOperation("The excel worksheet does not contain any data.");
			}

			_totalColumns = _sheet.Dimension.End.Column;
			_totalRows = _sheet.Dimension.End.Row - 1; // excluding 1st

			// Determine column names from 1st row (excel indexes start from 1)
			var cols = new List<string>();
			for (int i = 1; i <= _totalColumns; i++)
			{
				cols.Add(_sheet.Cells[1, i].Text);
			}

			_columns = cols.ToArray();
			ValidateColumns(_columns);
			_properties = new Dictionary<string, TargetProperty>(_columns.Length, StringComparer.InvariantCultureIgnoreCase);

			// determine corresponding Properties for given columns 
			var t = typeof(T);
			foreach (var col in _columns)
			{
				var pi = t.GetProperty(col);
				if (pi != null)
				{
					_properties[col] = new TargetProperty
					{
						IsSettable = pi.CanWrite && pi.GetSetMethod().IsPublic,
						PropertyInfo = pi
					};
				}
			}

			_bof = true;
			_pageable = new PagedList(0, BATCHSIZE, _totalRows);
		}

		public int TotalRows
		{
			get { return _totalRows; }
		}

		public int TotalColumns
		{
			get { return _totalColumns; }
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

		public void Reset()
		{
			if (_pageable.PageIndex != 0 && _currentBatch != null)
			{
				_currentBatch.Clear();
				_currentBatch = null;
			}
			_bof = true;
			_pageable.PageIndex = 0;
		}

		public bool ReadNextBatch()
		{
			if (_currentBatch != null)
			{
				_currentBatch.Clear();
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

		public ICollection<ImportRow<T>> CurrentBatch
		{
			get
			{
				if (_currentBatch == null)
				{
					_currentBatch = new List<ImportRow<T>>();

					int start = _pageable.FirstItemIndex + 1;
					int end = _pageable.LastItemIndex + 1;

					// Determine cell values per row
					for (int r = start; r <= end; r++)
					{
						var values = new List<object>();
						for (int c = 1; c <= _totalColumns; c++)
						{
							values.Add(_sheet.Cells[r, c].Value);
						}

						_currentBatch.Add(new ImportRow<T>(_columns, values.ToArray(), _properties, r - 1));
					}
				}

				return _currentBatch.AsReadOnly();
			}
		}

		protected override void OnDispose(bool disposing)
		{
			if (disposing)
			{
				_sheet = null;
				if (_excelPackage != null)
				{
					_excelPackage.Dispose();
					_excelPackage = null;
				}
			}
		}

		private void ValidateColumns(string[] columns)
		{
			if (columns.Any(x => x.IsEmpty()))
			{
				throw Error.InvalidOperation("The first row must contain the column names and therefore cannot have empty cells.");
			}

			if (columns.Select(x => x.ToLower()).Distinct().ToArray().Length != columns.Length)
			{
				throw Error.InvalidOperation("The first row cannot contain duplicate column names.");
			}
		}
	}

	internal class ImportRow<T> : Dictionary<string, object> where T : BaseEntity
	{
		private bool _initialized = false;
		private T _entity;
		private string _entityDisplayName;
		private int _position;
		private bool _isNew;
		private ImportRowInfo _rowInfo;

		public ImportRow(string[] columns, object[] values, IDictionary<string, TargetProperty> properties, int position)
			: base(columns.Length, StringComparer.InvariantCultureIgnoreCase)
		{
			_position = position;

			for (int i = 0; i < columns.Length; i++)
			{
				var col = columns[i];
				var val = values[i];

				if (val != null && val.ToString().HasValue())
				{
					if (!properties.ContainsKey(col) || properties[col].IsSettable)
					{
						// only add value when no correponding property exists (special field)
						// or when property exists but it's publicly settable.
						this[col] = val;
					}
				}
			}
		}

		public void Initialize(T entity, string entityDisplayName)
		{
			_entity = entity;
			_entityDisplayName = entityDisplayName;
			_isNew = _entity.Id == 0;

			_initialized = true;
		}

		private void CheckInitialized()
		{
			if (_initialized)
			{
				throw Error.InvalidOperation("A row must be initialized before interacting with the entity or the data store");
			}
		}

		public bool IsTransient
		{
			get { return _entity.Id == 0; }
		}

		public bool IsNew
		{
			get { return _isNew; }
		}

		public T Entity
		{
			get { return _entity; }
		}

		public string EntityDisplayName
		{
			get { return _entityDisplayName; }
		}

		public bool NameChanged
		{
			get;
			set;
		}

		public int Position
		{
			get { return _position; }
		}

		public TProp GetValue<TProp>(string columnName)
		{
			object value;
			if (this.TryGetValue(columnName, out value))
			{
				return value.Convert<TProp>();
			}

			return default(TProp);
		}

		public bool SetProperty<TProp>(ImportResult result, T target, Expression<Func<T, TProp>> prop, TProp defaultValue = default(TProp), Func<object, TProp> converter = null)
		{
			// TBD: (MC) do not check for perf reason?
			//CheckInitialized();

			var pi = prop.ExtractPropertyInfo();
			var propName = pi.Name;

			try
			{
				object value;
				if (this.TryGetValue(propName, out value))
				{
					// source contains field value. Set it.
					TProp converted;
					if (converter != null)
					{
						converted = converter(value);
					}
					else
					{
						if (value.ToString().ToUpper().Equals("NULL"))
						{
							// prop is set "explicitly" to null.
							converted = default(TProp);
						}
						else
						{
							converted = value.Convert<TProp>();
						}
					}
					return target.TrySetPropertyValue(propName, converted);
				}
				else
				{
					// source does not contain field data or it's empty...
					if (IsTransient && defaultValue != null)
					{
						// ...but the entity is new. In this case
						// set the default value if given.
						return target.TrySetPropertyValue(propName, defaultValue);
					}
				}
			}
			catch (Exception ex)
			{
				result.AddWarning("Conversion failed: " + ex.Message, this.GetRowInfo(), propName);
			}

			return false;
		}

		public ImportRowInfo GetRowInfo()
		{
			if (_rowInfo == null)
			{
				_rowInfo = new ImportRowInfo(this.Position, this.EntityDisplayName);
			}

			return _rowInfo;
		}

		public override string ToString()
		{
			var str = "Pos: {0} - Name: {1}, IsNew: {2}, IsTransient: {3}".FormatCurrent(
				Position,
				EntityDisplayName.EmptyNull(),
				_initialized ? IsNew.ToString() : "-",
				_initialized ? IsTransient.ToString() : "-");
			return str;
		}
	}


	internal class TargetProperty
	{
		public bool IsSettable { get; set; }
		public PropertyInfo PropertyInfo { get; set; }
	}
}
