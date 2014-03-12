using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SmartStore.Core;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.Plugin.Feed.Froogle.Data.Migrations;

namespace SmartStore.Plugin.Feed.Froogle.Data
{
	/// <summary>
	/// Object context
	/// </summary>
	public class GoogleProductObjectContext : ObjectContextBase
	{
        public const string ALIASKEY = "sm_object_context_google_product";

		static GoogleProductObjectContext()
		{
			var initializer = new MigrateDatabaseInitializer<GoogleProductObjectContext, Configuration>
			{
				TablesToCheck = new[] { "GoogleProduct" }
			};
			Database.SetInitializer(initializer);
		}

		/// <summary>
		/// For tooling support, e.g. EF Migrations
		/// </summary>
		public GoogleProductObjectContext()
			: base()
		{
		}

        public GoogleProductObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY) 
		{
		}


		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new GoogleProductRecordMap());

			//disable EdmMetadata generation
			//modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
			base.OnModelCreating(modelBuilder);
		}

	}
}