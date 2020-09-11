using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange.Import
{
    public class ImportDataSegmenter
    {
        private const int BATCHSIZE = 100;

        private readonly IDataTable _table;
        private object[] _currentBatch;
        private readonly IPageable _pageable;
        private bool _bof;
        private CultureInfo _culture;
        private ColumnMap _columnMap;

        private readonly IDictionary<string, string[]> _columnIndexes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        public ImportDataSegmenter(IDataTable table, ColumnMap map)
        {
            Guard.NotNull(table, nameof(table));
            Guard.NotNull(map, nameof(map));

            _table = table;
            _columnMap = map;

            _bof = true;
            _pageable = new PagedList(0, BATCHSIZE, table.Rows.Count);
            _culture = CultureInfo.InvariantCulture;
        }

        public CultureInfo Culture
        {
            get => _culture;
            set => _culture = value ?? CultureInfo.InvariantCulture;
        }

        public ColumnMap ColumnMap
        {
            get => _columnMap;
            set => _columnMap = value ?? new ColumnMap();
        }

        public int TotalRows => _table.Rows.Count;

        public int TotalColumns => _table.Columns.Count;

        public int CurrentSegment => _bof ? 0 : _pageable.PageNumber;

        public int CurrentSegmentFirstRowIndex => _pageable.FirstItemIndex;

        public int TotalSegments => _pageable.TotalPages;

        public int BatchSize => BATCHSIZE;

        /// <summary>
        /// Determines whether a specific column exists in the underlying data table.
        /// </summary>
        /// <param name="name">The name of the column to find</param>
        /// <param name="withAnyIndex">
        ///		If <c>true</c> and a column with the passed <paramref name="name"/> does not exist,
        ///		this method tests for the existence of any indexed column with the same name.
        /// </param>
        /// <returns><c>true</c> if the column exists, <c>false</c> otherwise</returns>
        /// <remarks>
        ///		This method takes mapped column names into account.
        /// </remarks>
        public bool HasColumn(string name, bool withAnyIndex = false)
        {
            var result = HasColumn(name, null);

            if (!result && withAnyIndex)
            {
                // Column does not exist, but withAnyIndex is true:
                // Test for existence of any indexed column.
                result = GetColumnIndexes(name).Length > 0;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the column <c>name[index]</c> exists in the underlying data table.
        /// </summary>
        /// <param name="name">The name of the column to find</param>
        /// <param name="index">The index of the column</param>
        /// <returns><c>true</c> if the column exists, <c>false</c> otherwise</returns>
        /// <remarks>
        ///		This method takes mapped column names into account.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasColumn(string name, string index)
        {
            return _table.HasColumn(_columnMap.GetMapping(name, index).MappedName);
        }

        /// <summary>
        /// Indicates whether to ignore the property that is mapped to columnName
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <returns><c>true</c> ignore, <c>false</c> do not ignore</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIgnored(string columnName)
        {
            return IsIgnored(columnName, null);
        }

        /// <summary>
        /// Indicates whether to ignore the property that is mapped to columnName
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <param name="index">The index of the column</param>
        /// <returns><c>true</c> ignore, <c>false</c> do not ignore</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIgnored(string columnName, string index)
        {
            var mapping = _columnMap.GetMapping(columnName, index);

            return mapping.IgnoreProperty;
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

        public IEnumerable<ImportRow<T>> GetCurrentBatch<T>() where T : BaseEntity
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

            return _currentBatch.Cast<ImportRow<T>>();
        }
    }
}
