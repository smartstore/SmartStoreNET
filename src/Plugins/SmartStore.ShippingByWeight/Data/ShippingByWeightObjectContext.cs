using System.Data.Entity;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.ShippingByWeight.Data.Migrations;

namespace SmartStore.ShippingByWeight.Data
{
    /// <summary>
    /// Object context
    /// </summary>
    public class ShippingByWeightObjectContext : ObjectContextBase
    {
        public const string ALIASKEY = "sm_object_context_shipping_weight_zip";

        static ShippingByWeightObjectContext()
        {
            var initializer = new MigrateDatabaseInitializer<ShippingByWeightObjectContext, Configuration>
            {
                TablesToCheck = new[] { "ShippingByWeight" }
            };
            Database.SetInitializer(initializer);
        }

        /// <summary>
        /// For tooling support, e.g. EF Migrations
        /// </summary>
        public ShippingByWeightObjectContext()
            : base()
        {
        }

        public ShippingByWeightObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ShippingByWeightRecordMap());

            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }

    }
}