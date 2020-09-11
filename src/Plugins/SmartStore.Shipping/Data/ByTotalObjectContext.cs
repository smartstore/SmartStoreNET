using System.Data.Entity;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.Shipping.Data.Migrations;

namespace SmartStore.Shipping.Data
{

    /// <summary>
    /// Object context
    /// </summary>
    public class ByTotalObjectContext : ObjectContextBase
    {
        public const string ALIASKEY = "sm_object_context_shipping_by_total";

        static ByTotalObjectContext()
        {
            var initializer = new MigrateDatabaseInitializer<ByTotalObjectContext, Configuration>
            {
                TablesToCheck = new[] { "ShippingByTotal" }
            };
            Database.SetInitializer(initializer);
        }

        /// <summary>
        /// For tooling support, e.g. EF Migrations
        /// </summary>
        public ByTotalObjectContext()
            : base()
        {
        }

        public ByTotalObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ByTotalRecordMap());

            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }

    }
}
