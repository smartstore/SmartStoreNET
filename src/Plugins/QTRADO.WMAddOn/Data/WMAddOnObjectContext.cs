using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

using SmartStore.Core;
using SmartStore.Data;
using SmartStore.Data.Setup;
using QTRADO.WMAddOn.Data.Migrations;
using QTRADO.WMAddOn.Domain;

namespace QTRADO.WMAddOn.Data
{
    /// <summary>
    /// Object context
    /// </summary>
    public class WMAddOnObjectContext : ObjectContextBase
    {
        public const string ALIASKEY = "sm_object_context_WMAddOn";

        static WMAddOnObjectContext()
        {
            var initializer = new MigrateDatabaseInitializer<WMAddOnObjectContext, Configuration>
            {
                TablesToCheck = new[] { "Grossists" }
            };
            Database.SetInitializer(initializer);
        }

        /// <summary>
        /// For tooling support, e.g. EF Migrations
        /// </summary>
        public WMAddOnObjectContext()
            : base()
        {
        }

        public WMAddOnObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Grossist>();

            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}