using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SmartStore.Core;
using SmartStore.Data;

namespace SmartStore.Plugin.Feed.Froogle.Data
{
	/// <summary>
	/// Object context
	/// </summary>
	public class GoogleProductObjectContext : ObjectContextBase
	{
        public const string ALIASKEY = "sm_object_context_google_product";
        
        public GoogleProductObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY) 
		{
			//((IObjectContextAdapter) this).ObjectContext.ContextOptions.LazyLoadingEnabled = true;
		}


		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new GoogleProductRecordMap());

			//disable EdmMetadata generation
			//modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
			base.OnModelCreating(modelBuilder);
		}

		/// <summary>
		/// Install
		/// </summary>
		public void Install()
		{
			//create the table
			var dbScript = CreateDatabaseScript();
			Database.ExecuteSqlCommand(dbScript);
			SaveChanges();
		}

		/// <summary>
		/// Uninstall
		/// </summary>
		public void Uninstall()
		{
            //drop the table
            string tableName = "GoogleProduct";
            if (Database.SqlQuery<int>("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}", tableName).Any<int>())
            {
                var dbScript = "DROP TABLE [" + tableName + "]";
                Database.ExecuteSqlCommand(dbScript);
            }
            SaveChanges();
            //old way of dropping the table
            //try
            //{
            //    //we place it in try-catch here because previous versions of Froogle didn't have any tables
            //    var dbScript = "DROP TABLE GoogleProduct";
            //    Database.ExecuteSqlCommand(dbScript);
            //    SaveChanges();
            //}
            //catch
            //{
            //}
		}

	}
}