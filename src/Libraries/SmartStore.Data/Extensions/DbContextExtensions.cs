using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace SmartStore
{

	public class SqlServerInfo
	{
		public string ProductVersion { get; set; }
		public string PatchLevel { get; set; }
		public string ProductEdition { get; set; }
		public string ClrVersion { get; set; }
		public string DefaultCollation { get; set; }
		public string Instance { get; set; }
		public int Lcid { get; set; }
		public string ServerName { get; set; }
	}

	public static class DbContextExtensions
	{

		public static bool ColumnExists(this DbContext context, string tableName, string columnName) 
		{
			if (context != null && tableName.HasValue() && columnName.HasValue()) 
			{
				//string sql = @"Select column_name From INFORMATION_SCHEMA.COLUMNS Where table_name = @tableName And column_name = @columnName";
		
				//string col = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<string>(sql,
				//	new SqlParameter("@tableName", tableName), new SqlParameter("@columnName", columnName)).FirstOrDefault();

				string sql = @"Select column_name From INFORMATION_SCHEMA.COLUMNS Where table_name = {0} And column_name = {1}";
				string col = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<string>(sql, tableName, columnName).FirstOrDefault();
				return col.HasValue();
			}

			return false;
		}

		public static void ColumnEnsure(this DbContext context, string tableName, string columnName, string columnDataType) 
		{
			if (!context.ColumnExists(tableName, columnName)) 
			{
				//context.Database.ExecuteSqlCommand("ALTER TABLE dbo.[{0}] ADD {1} {2}".FormatWith(tableName, columnName, columnDataType));
				context.Database.ExecuteSqlCommand("ALTER TABLE {0} ADD {1} {2}".FormatWith(tableName, columnName, columnDataType));
			}
		}

		public static void ColumnDelete(this DbContext context, string tableName, string columnName) 
		{
			if (context.ColumnExists(tableName, columnName)) 
			{
				context.Database.ExecuteSqlCommand("ALTER TABLE {0} DROP COLUMN {1}".FormatWith(tableName, columnName));
			}
		}

		public static bool DatabaseExists(this DbContext context, string databaseName)
		{
			if (context != null && databaseName.HasValue()) {
				//string sql = @"Select database_id From sys.databases Where Name = @databaseName";

				//int databaseID = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<int>(sql, new SqlParameter("@databaseName", databaseName)).FirstOrDefault();

				string sql = @"Select database_id From sys.databases Where Name = {0}";

				int databaseID = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<int>(sql, databaseName).FirstOrDefault();

				return databaseID > 0;
			}

			return false;
		}

		public static int InsertInto(this DbContext context, string sql, params object[] parameters) 
		{
			return (int)context.Database.SqlQuery<decimal>(sql + "; SELECT @@IDENTITY;", parameters).FirstOrDefault();
		}

		public static int Execute(this DbContext context, string sql, params object[] parameters) 
		{
			return context.Database.ExecuteSqlCommand(sql, parameters);
		}

		internal static SqlServerInfo GetSqlServerInfo(this DbContext context)
		{
			string sql = @"SELECT  
    SERVERPROPERTY('productversion') as 'ProductVersion', 
    SERVERPROPERTY('productlevel') as 'PatchLevel',  
    SERVERPROPERTY('edition') as 'ProductEdition',
    SERVERPROPERTY('buildclrversion') as 'ClrVersion',
    SERVERPROPERTY('collation') as 'DefaultCollation',
    SERVERPROPERTY('instancename') as 'Instance',
    SERVERPROPERTY('lcid') as 'Lcid',
    SERVERPROPERTY('servername') as 'ServerName'";

			return context.Database.SqlQuery<SqlServerInfo>(sql).FirstOrDefault();
		}

	}
}
