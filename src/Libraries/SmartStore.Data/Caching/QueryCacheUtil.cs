using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SmartStore.ComponentModel;

namespace SmartStore.Data.Caching
{
	internal static class QueryCacheUtil
	{
		public static ObjectQuery GetObjectQuery(this IQueryable query)
		{
			// CHECK ObjectQuery
			var objectQuery = query as ObjectQuery;
			if (objectQuery != null)
			{
				return objectQuery;
			}

			// check DbQuery
			var dbQuery = query as DbQuery;

			if (dbQuery != null)
			{
				return GetObjectQueryReflected(query, dbQuery.GetType());
			}

			var type = query.GetType();

			if (query.GetType().IsGenericType && query.GetType().GetGenericTypeDefinition() == typeof(DbQuery<>))
			{
				return GetObjectQueryReflected(query, typeof(DbQuery<>).MakeGenericType(query.ElementType));
			}

			if (query.GetType().IsGenericType && query.GetType().GetGenericTypeDefinition() == typeof(DbSet<>))
			{
				return GetObjectQueryReflected(query, typeof(DbSet<>).MakeGenericType(query.ElementType));
			}

			throw new SmartException("Unable to resolve ObjectQuery from IQueryable");
		}

		private static ObjectQuery GetObjectQueryReflected(IQueryable query, Type queryType)
		{
			var internalQueryProperty = FastProperty.GetProperty(queryType, "InternalQuery");
			var internalQuery = internalQueryProperty.GetValue(query);
			var objectQueryContextProperty = FastProperty.GetProperty(internalQuery.GetType(), "ObjectQuery");
			var objectQueryContext = objectQueryContextProperty.GetValue(internalQuery);

			return objectQueryContext as ObjectQuery;
		}

		public static Tuple<string, DbParameterCollection> GetCommandTextAndParameters(this ObjectQuery objectQuery)
		{
			var stateField = objectQuery.GetType().BaseType.GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
			var state = stateField.GetValue(objectQuery);
			var getExecutionPlanMethod = state.GetType().GetMethod("GetExecutionPlan", BindingFlags.NonPublic | BindingFlags.Instance);
			var getExecutionPlan = getExecutionPlanMethod.Invoke(state, new object[] { null });
			var prepareEntityCommandMethod = getExecutionPlan.GetType().GetMethod("PrepareEntityCommand", BindingFlags.NonPublic | BindingFlags.Instance);

			var sql = "";

			using (var entityCommand = (EntityCommand)prepareEntityCommandMethod.Invoke(getExecutionPlan, new object[] { objectQuery.Context, objectQuery.Parameters }))
			{
				var getCommandDefinitionMethod = entityCommand.GetType().GetMethod("GetCommandDefinition", BindingFlags.NonPublic | BindingFlags.Instance);
				var getCommandDefinition = getCommandDefinitionMethod.Invoke(entityCommand, new object[0]);

				var prepareEntityCommandBeforeExecutionMethod = getCommandDefinition.GetType().GetMethod("PrepareEntityCommandBeforeExecution", BindingFlags.NonPublic | BindingFlags.Instance);
				var prepareEntityCommandBeforeExecution = (DbCommand)prepareEntityCommandBeforeExecutionMethod.Invoke(getCommandDefinition, new object[] { entityCommand });

				sql = prepareEntityCommandBeforeExecution.CommandText;
				var parameters = prepareEntityCommandBeforeExecution.Parameters;

				return new Tuple<string, DbParameterCollection>(sql, parameters);
			}
		}

		public static IEnumerable<string> GetAffectedEntitySets(this ObjectQuery objectQuery)
		{
			var result = new HashSet<string>();

			var entityType = objectQuery.GetResultType().EdmType as EntityType;

			if (entityType != null)
			{
				result.Add(entityType.Name);
				foreach (var nav in entityType.NavigationProperties)
				{
					var edmType = nav.TypeUsage.EdmType;
					if (edmType is CollectionType)
					{
						result.Add(((CollectionType)edmType).TypeUsage.EdmType.Name);
					}
					else
					{
						result.Add(edmType.Name);
					}	
				}
			}

			return result;
		}
	}
}
