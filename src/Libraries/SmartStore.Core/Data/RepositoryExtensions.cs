﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SmartStore.Core.Data
{   
    public static class RepositoryExtensions
    {
        public static T GetFirst<T>(this IRepository<T> rs, Func<T, bool> predicate) where T : BaseEntity
        {
            return rs.Table.FirstOrDefault(predicate);
        }

        public static IEnumerable<T> GetMany<T>(this IRepository<T> rs, IEnumerable<int> ids) where T : BaseEntity
        {
            foreach (var chunk in ids.Chunk())
            {
                var items = rs.Table.Where(a => chunk.Contains(a.Id)).ToList();
                foreach (var item in items)
                {
                    yield return item;
                }
            }
        }

        public static void Delete<T>(this IRepository<T> rs, int id) where T : BaseEntity
        {
			Guard.NotZero(id, nameof(id));
			
			// Perf: work with stub entity
			var entity = rs.Create();
			entity.Id = id;

			rs.Attach(entity);

			// must downcast 'cause of Rhino mocks stub  
			rs.Context.ChangeState((BaseEntity)entity, System.Data.Entity.EntityState.Deleted);
        }

		public static void DeleteRange<T>(this IRepository<T> rs, IEnumerable<int> ids) where T : BaseEntity
		{
			Guard.NotNull(ids, nameof(ids));

			ids.Each(id => rs.Delete(id));
		}

		/// <summary>
		/// Truncates the table
		/// </summary>
		/// <typeparam name="T">Entity type</typeparam>
		/// <param name="rs">The repository</param>
		/// <param name="predicate">An optional filter</param>
		/// <param name="cascade">
		/// <c>false</c>: does not make any attempts to determine dependant entities, just deletes ONLY them (faster).
		/// <c>true</c>: loads all entities into the context first and deletes them, along with their dependencies (slower).
		/// </param>
		/// <returns>The total number of affected entities</returns>
		/// <remarks>
		/// This method turns off auto detection, validation and hooking.
		/// </remarks>
		[SuppressMessage("ReSharper", "UnusedVariable")]
		public static int DeleteAll<T>(this IRepository<T> rs, Expression<Func<T, bool>> predicate = null, bool cascade = false) where T : BaseEntity
		{
			var count = 0;

			using (var scope = new DbContextScope(ctx: rs.Context, autoDetectChanges: false, validateOnSave: false, hooksEnabled: false, autoCommit: false))
			{
				var query = rs.Table;
				if (predicate != null)
				{
					query = query.Where(predicate);
				}

				if (cascade)
				{
					var records = query.ToList();
					foreach (var chunk in records.Chunk(500))
					{
						rs.DeleteRange(chunk.ToList());
						count += rs.Context.SaveChanges();
					}
				}
				else
				{
					var ids = query.Select(x => new { Id = x.Id }).ToList();
					foreach (var chunk in ids.Chunk(500))
					{
						rs.DeleteRange(chunk.Select(x => x.Id));
						count += rs.Context.SaveChanges();
					}
				}
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
