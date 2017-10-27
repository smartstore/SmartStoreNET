using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Data;
using EfState = System.Data.Entity.EntityState;

namespace SmartStore 
{
    public static class IDbContextExtensions 
	{   
		/// <summary>
        /// Loads the database copy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="currentCopy">The current copy.</param>
        /// <returns></returns>
        public static T LoadDatabaseCopy<T>(this IDbContext context, T currentCopy) where T : BaseEntity 
		{
            return InnerGetCopy(context, currentCopy, e => e.GetDatabaseValues());
        }

        /// <summary>
        /// Loads the original copy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="currentCopy">The current copy.</param>
        /// <returns></returns>
        public static T LoadOriginalCopy<T>(this IDbContext context, T currentCopy) where T : BaseEntity 
		{
            return InnerGetCopy(context, currentCopy, e => e.OriginalValues);
        }

		public static DbEntityEntry<T> GetEntry<T>(this IDbContext context, T entity) where T : BaseEntity
		{
			var entry = CastOrThrow<DbContext>(context).Entry<T>(entity);
			return entry;
		}

		public static bool IsPropertyModified<T>(this IDbContext ctx, T entity, Expression<Func<T, object>> propertySelector)
			where T : BaseEntity
		{
			object originalValue;
			return TryGetModifiedProperty(ctx, entity, propertySelector, out originalValue);
		}

		public static bool TryGetModifiedProperty<T>(this IDbContext ctx, T entity, Expression<Func<T, object>> propertySelector, out object originalValue)
			where T : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(propertySelector, nameof(propertySelector));

			var propertyName = propertySelector.ExtractMemberInfo().Name;
			return ctx.TryGetModifiedProperty(entity, propertyName, out originalValue);
		}

		/// <summary>
		/// Executes the <c>DBCC SHRINKDATABASE(0)</c> command against the SQL Server (Express) database
		/// </summary>
		/// <param name="context">The context</param>
		/// <returns><c>true</c>, when the operation completed successfully.</returns>
		public static bool ShrinkDatabase(this IDbContext context)
		{
			if (DataSettings.Current.IsSqlServer)
			{
				try
				{
					context.ExecuteSqlCommand("DBCC SHRINKDATABASE(0)", true);
					return true;
				}
				catch { }
			}

			return false;
		}

		/// <summary>
		/// Executes sql by using SQL-Server Management Objects which supports GO statements.
		/// </summary>
		public static int ExecuteSqlThroughSmo(this IDbContext ctx, string sql)
		{
			Guard.NotEmpty(sql, "sql");

			int result = 0;

			try
			{
				bool isSqlServer = DataSettings.Current.IsSqlServer;

				if (!isSqlServer)
				{
					result = ctx.ExecuteSqlCommand(sql);
				}
				else
				{
					using (var sqlConnection = new SqlConnection(ObjectContextBase.GetConnectionString()))
					{
						var serverConnection = new ServerConnection(sqlConnection);
						var server = new Server(serverConnection);

						result = server.ConnectionContext.ExecuteNonQuery(sql);
					}
				}
			}
			catch (Exception)
			{
				// remove the GO statements
				sql = Regex.Replace(sql, @"\r{0,1}\n[Gg][Oo]\r{0,1}\n", "\n");

				result = ctx.ExecuteSqlCommand(sql);
			}
			return result;
		}

		#region Utils

		private static bool IsInSaveOperation(this IDbContext context)
		{
			return (context as ObjectContextBase)?.IsInSaveOperation == true;
		}

		private static T InnerGetCopy<T>(IDbContext context, T currentCopy, Func<DbEntityEntry<T>, DbPropertyValues> func) where T : BaseEntity
		{
			// Get the database context
			var dbContext = CastOrThrow<DbContext>(context);

			// Get the entity tracking object
			DbEntityEntry<T> entry = GetEntityOrDefault(currentCopy, dbContext);

			// The output 
			T output = null;

			// Try and get the values
			if (entry != null)
			{
				DbPropertyValues dbPropertyValues = func(entry);
				if (dbPropertyValues != null)
				{
					output = dbPropertyValues.ToObject() as T;
				}
			}

			return output;
		}

		/// <summary>
		/// Gets the entity or return null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="currentCopy">The current copy.</param>
		/// <param name="dbContext">The db context.</param>
		/// <returns></returns>
		private static DbEntityEntry<T> GetEntityOrDefault<T>(T currentCopy, DbContext dbContext) where T : BaseEntity
		{
			return dbContext.ChangeTracker.Entries<T>().Where(e => e.Entity == currentCopy).FirstOrDefault();
		}

		private static DbContext CastOrThrow(IDbContext context)
		{
			return CastOrThrow<DbContext>(context);
		}

		private static T CastOrThrow<T>(IDbContext context) where T : DbContext
		{
			var dbContext = (context as T);

			if (dbContext == null)
			{
				throw new InvalidOperationException("Context does not support operation.");
			}

			return dbContext;
		}

		#endregion
	}
}