using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using SmartStore.Core;
using SmartStore.Data;

namespace SmartStore.Plugin.Shipping.ByWeight.Data
{
    /// <summary>
    /// Object context
    /// </summary>
    public class ShippingByWeightObjectContext : ObjectContextBase
    {
        public const string ALIASKEY = "sm_object_context_shipping_weight_zip";
        
        public ShippingByWeightObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
            //((IObjectContextAdapter) this).ObjectContext.ContextOptions.LazyLoadingEnabled = true;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ShippingByWeightRecordMap());

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
            var dbScript = "DROP TABLE ShippingByWeight";
            Database.ExecuteSqlCommand(dbScript);
            SaveChanges();
        }
       
    }
}