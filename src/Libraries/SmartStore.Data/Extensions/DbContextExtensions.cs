using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;

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

        public static bool TableExists(this DbContext context, string tableName)
        {
            if (context != null && tableName.HasValue())
            {
                var sql = @"Select table_name From INFORMATION_SCHEMA.TABLES Where table_name = {0}";
                var table = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<string>(sql, tableName).FirstOrDefault();
                return table.HasValue();
            }

            return false;
        }

        public static bool DatabaseExists(this DbContext context, string databaseName)
        {
            if (context != null && databaseName.HasValue())
            {
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

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void DumpAttachedEntities(this DbContext context)
        {
            context.ChangeTracker.Entries()
                .Where(x => x.State != System.Data.Entity.EntityState.Detached)
                .ToList()
                .ForEach(x => "{0} {1} {2}".FormatInvariant((x.Entity as BaseEntity).Id, x.State.ToString(), x.Entity.GetType().Name).Dump());
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void CreateSqlTimeout(this DbContext context)
        {
            var timeoutErrorSql =
                "CREATE PROCEDURE [dbo].[GetTimeoutError]\r\n" +
                "AS\r\n" +
                "BEGIN\r\n" +
                "  WAITFOR DELAY '00:01:00'\r\n" +
                "  SELECT GETDATE()\r\n" +
                "END";

            var storedProcedureSql =
                "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[GetTimeoutError]') AND OBJECTPROPERTY(object_id,N'IsProcedure') = 1)" +
                "BEGIN EXEC('" + timeoutErrorSql.Replace("'", "''") + "') END;";

            context.Execute(storedProcedureSql);

            var connectionString = DataSettings.Current.DataConnectionString;
            using (var cmd = new System.Data.SqlClient.SqlCommand("GetTimeoutError", new System.Data.SqlClient.SqlConnection(connectionString)))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandTimeout = 30;    // Seconds.

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
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
