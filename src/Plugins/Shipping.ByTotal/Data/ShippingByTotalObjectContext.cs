using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Data;
using SmartStore.Data.Initializers;
using SmartStore.Plugin.Shipping.ByTotal.Data.Migrations;

namespace SmartStore.Plugin.Shipping.ByTotal.Data
{
	
	/// <summary>
    /// Object context
    /// </summary>
    public class ShippingByTotalObjectContext : ObjectContextBase
    {
        public const string ALIASKEY = "sm_object_context_shipping_by_total";

		static ShippingByTotalObjectContext()
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersionEx<ShippingByTotalObjectContext, Configuration>());
		}

		/// <summary>
		/// For tooling support, e.g. EF Migrations
		/// </summary>
		public ShippingByTotalObjectContext()
			: base()
		{
		}

        public ShippingByTotalObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ShippingByTotalRecordMap());

            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Install
        /// </summary>
        public void Install()
        {
			////create table
			//var dbScript = CreateDatabaseScript();
			//Database.ExecuteSqlCommand(dbScript);
			//SaveChanges();
        }

        /// <summary>
        /// Uninstall
        /// </summary>
        public void Uninstall()
        {
			//var dbScript = "DROP TABLE ShippingByTotal";
			//Database.ExecuteSqlCommand(dbScript);
			//SaveChanges();

			var migrator = new DbMigrator(new Configuration());
			migrator.Update("0");
        }

    }
}
