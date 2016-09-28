using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using SmartStore.Core;

namespace SmartStore.Data.Caching
{
	public static partial class QueryCacheExtensions
	{
		public static Task<IList<T>> FromCacheAsync<T>(this IQueryable<T> query, params string[] tags) where T : BaseEntity
		{
			return FromCacheAsync(query, TimeSpan.MaxValue, tags);
		}

		public static Task<IList<T>> FromCacheAsync<T>(this IQueryable<T> query, TimeSpan duration, params string[] tags) where T : BaseEntity
		{
			throw new NotImplementedException();
		}


		#region RequestCache

		#endregion
	}
}
