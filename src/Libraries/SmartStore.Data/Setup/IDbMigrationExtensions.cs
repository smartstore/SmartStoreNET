using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SmartStore.Core.Data;
using SmartStore.Utilities;

namespace SmartStore.Data.Setup
{
	
	public static class IDbMigrationExtensions
	{

		public static void SqlFileOrResource(this IDbMigration migration, string fileName, Assembly assembly = null, string location = null)
		{
			Guard.NotEmpty(fileName, nameof(fileName));

			var tokenizer = new SqlFileTokenizer(fileName, assembly ?? Assembly.GetExecutingAssembly(), location);
			foreach (var cmd in tokenizer.Tokenize())
			{
				if (cmd.HasValue())
				{
					migration.AddSqlOperation(cmd);
				}
			}
		}

		private static void AddSqlOperation(this IDbMigration migration, string sql, bool suppressTransaction = false, object anonymousArguments = null)
		{
			var operation = new SqlOperation(sql, anonymousArguments)
			{
				SuppressTransaction = suppressTransaction
			};
			migration.AddOperation(operation);
		}

	}

}
