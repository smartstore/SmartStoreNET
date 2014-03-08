using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;

namespace SmartStore.Data
{
	
	public static class DbMigrationExtensions
	{

		public static void NewSetting(this IDbMigration migration, string name, string value)
		{
			Guard.ArgumentNotEmpty(() => name);
			value = value.EmptyNull();

			string sql = "INSERT [Setting] ([Name], [Value], [StoreId]) VALUES (N'{0}', N'{1}', 0)".FormatInvariant(name, value);
			if (DataSettings.Current.IsSqlServer)
			{
				sql = "IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [Name] = N'{0}')\nBEGIN\n\t{1}\nEND".FormatInvariant(name, sql);
			}
			migration.AddSqlOperation(sql);
		}

		public static void DeleteSetting(this IDbMigration migration, string name)
		{
			Guard.ArgumentNotEmpty(() => name);

			string sql = "DELETE FROM [Setting] WHERE [name] = N'{0}'".FormatInvariant(name);
			migration.AddSqlOperation(sql);
		}

		private static void AddSqlOperation(this IDbMigration migration, string sql, bool suppressTransaction = false, object anonymousArguments = null)
		{
			SqlOperation operation = new SqlOperation(sql, anonymousArguments)
			{
				SuppressTransaction = suppressTransaction
			};
			migration.AddOperation(operation);

		}

	}

}
