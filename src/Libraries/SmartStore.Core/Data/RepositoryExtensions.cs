using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SmartStore.Core.Data
{
    
    public static class RepositoryExtensions
    {

        public static IEnumerable<T> LoadAll<T>(this IRepository<T> rs) where T : BaseEntity
        {
            return rs.Table.AsEnumerable();
        }

        public static IEnumerable<T> Where<T>(this IRepository<T> rs, Func<T, bool> predicate) where T : BaseEntity
        {
            return rs.Table.Where(predicate);
        }

        public static T GetSingle<T>(this IRepository<T> rs, Func<T, bool> predicate) where T : BaseEntity
        {
            return rs.Table.SingleOrDefault(predicate);
        }

        public static T GetFirst<T>(this IRepository<T> rs, Func<T, bool> predicate) where T : BaseEntity
        {
            return rs.Table.FirstOrDefault(predicate);
        }

        public static IEnumerable<T> GetMany<T>(this IRepository<T> rs, IEnumerable<int> ids) where T : BaseEntity
        {
            foreach (var chunk in ids.Chunk())
            {
                var query = rs.Table.Where(a => chunk.Contains(a.Id));
                foreach (var item in query)
                {
                    yield return item;
                }
            }
        }

        public static void Delete<T>(this IRepository<T> rs, object id) where T : BaseEntity
        {
            T entityToDelete = rs.GetById(id);
            if (entityToDelete != null)
            {
                rs.Delete(entityToDelete);
            }
        }

		/// <summary>
		/// Truncates the whole table
		/// </summary>
		/// <typeparam name="T">Entity type</typeparam>
		/// <param name="rs"></param>
		/// <returns>The total number of affected entities</returns>
		/// <remarks>
		/// This method turns off auto detection, validation and hooking.
		/// </remarks>
		public static int DeleteAll<T>(this IRepository<T> rs) where T : BaseEntity
		{
			var autoCommit = rs.AutoCommitEnabled;
			rs.AutoCommitEnabled = false;

			var count = 0;

			try
			{
				using (var scope = new DbContextScope(autoDetectChanges: false, validateOnSave: false, hooksEnabled: false))
				{
					var records = rs.Table.ToList();
					foreach (var chunk in records.Chunk(500))
					{
						rs.DeleteRange(chunk.ToList());
						count += rs.Context.SaveChanges();
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				rs.AutoCommitEnabled = autoCommit;
			}	

			return count;
		}

        public static IQueryable<T> Get<T>(
            this IRepository<T> rs,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "") where T : BaseEntity
        {
            IQueryable<T> query = rs.Table;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
				query = query.Expand(includeProperty.Trim());
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

    }

}
