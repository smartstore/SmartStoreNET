using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using SmartStore.Core;

namespace SmartStore.Data.Utilities
{
    /// <summary>
    /// Ensures stable and consistent paging performance over very large datasets.
    /// </summary>
    public sealed class FastPager<T> where T : BaseEntity, new()
    {
        private readonly IQueryable<T> _query;
        private readonly int _pageSize;

        private int? _maxId;

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
        }

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

            _maxId = idSelector(page.Last());
            return true;
        }
    }
}
