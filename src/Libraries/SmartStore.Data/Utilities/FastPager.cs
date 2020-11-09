using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SmartStore.Core;

namespace SmartStore.Data.Utilities
{
    /// <summary>
    /// Ensures stable and consistent paging performance over very large datasets.
    /// Other than LINQs Skip(x).Take(y) approach the entity set is sorted 
    /// descending by id and a specified amount of records are returned.
    /// The FastPager remembers the last (lowest) returned id and uses
    /// it for the next batches' WHERE clause. This way Skip() can be avoided which
    /// is known for performing really bad on large tables.
    /// </summary>
    public sealed class FastPager<T> where T : BaseEntity, new()
    {
        private readonly IQueryable<T> _query;
        private readonly int _pageSize;

        private int? _maxId;
        private int? _currentPage;

        public FastPager(IQueryable<T> query, int pageSize = 1000)
        {
            Guard.NotNull(query, nameof(query));
            Guard.IsPositive(pageSize, nameof(pageSize));

            _query = query;
            _pageSize = pageSize;
        }

        public void Reset()
        {
            _maxId = null;
            _currentPage = null;
        }

        public int? MaxId => _maxId;

        public int? CurrentPage => _currentPage;

        public bool ReadNextPage(out IList<T> page)
        {
            return ReadNextPage<T>(x => x, x => x.Id, out page);
        }

        public bool ReadNextPage<TShape>(
            Expression<Func<T, TShape>> shaper,
            Func<TShape, int> idSelector,
            out IList<TShape> page)
        {
            Guard.NotNull(shaper, nameof(shaper));

            page = null;

            if (_maxId == null)
            {
                _maxId = int.MaxValue;
                _currentPage = 0;
            }
            if (_maxId.Value <= 1)
            {
                return false;
            }

            page = _query
                .Where(x => x.Id < _maxId.Value)
                .OrderByDescending(x => x.Id)
                .Take(() => _pageSize)
                .Select(shaper)
                .ToList();

            if (page.Count == 0)
            {
                _maxId = -1;
                page = null;
                return false;
            }

            _currentPage++;
            _maxId = idSelector(page.Last());
            return true;
        }

        public async Task<IList<T>> ReadNextPageAsync<TShape>()
        {
            return await ReadNextPageAsync(x => x, x => x.Id);
        }

        public async Task<IList<TShape>> ReadNextPageAsync<TShape>(
            Expression<Func<T, TShape>> shaper,
            Func<TShape, int> idSelector)
        {
            Guard.NotNull(shaper, nameof(shaper));

            if (_maxId == null)
            {
                _maxId = int.MaxValue;
                _currentPage = 0;
            }
            if (_maxId.Value <= 1)
            {
                return null;
            }

            var page = await _query
                .Where(x => x.Id < _maxId.Value)
                .OrderByDescending(x => x.Id)
                .Take(() => _pageSize)
                .Select(shaper)
                .ToListAsync();

            if (page.Count == 0)
            {
                _maxId = -1;
                page = null;
                return null;
            }

            _currentPage++;
            _maxId = idSelector(page.Last());
            return page;
        }
    }
}
