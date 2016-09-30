using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using SmartStore.ComponentModel;

namespace SmartStore.Data.Caching
{
	internal static class DbCacheUtil
	{
		public static ObjectQuery GetObjectQuery<T>(this IQueryable<T> source)
		{
			var dbQuery = source as DbQuery<T>;

			if (dbQuery != null)
			{
				var internalQueryProperty = FastProperty.GetProperty(source.GetType(), "InternalQuery");
				var internalQuery = internalQueryProperty.GetValue(source);

				var objectQueryProperty = FastProperty.GetProperty(internalQuery.GetType(), "ObjectQuery");
				var objectQuery = objectQueryProperty.GetValue(internalQuery);

				return objectQuery as ObjectQuery;
			}

			return null;
		}

		public static CommandInfo GetCommandInfo(this ObjectQuery objectQuery)
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

				return new CommandInfo(sql, parameters);
			}
		}

		public static string[] GetAffectedEntitySets(this ObjectQuery objectQuery)
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

			return result.ToArray();
		}

		internal class CommandInfo : Tuple<string, DbParameterCollection>
		{
			public CommandInfo(string sql, DbParameterCollection parameters)
				: base(sql, parameters)
			{
			}

			public string Sql { get { return base.Item1; } }
			public DbParameterCollection Parameters { get { return base.Item2; } }
		}
	}
}
