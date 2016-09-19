using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SmartStore.Core;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.MegaMenu.Data.Migrations;

namespace SmartStore.MegaMenu.Data
{
	public class MegaMenuObjectContext : ObjectContextBase
	{
        public const string ALIASKEY = "sm_object_context_mega_menu";

		static MegaMenuObjectContext()
		{
			var initializer = new MigrateDatabaseInitializer<MegaMenuObjectContext, Configuration>
			{
				TablesToCheck = new[] { "MegaMenu" }
			};
			Database.SetInitializer(initializer);
		}
        
		public MegaMenuObjectContext()
			: base()
		{
		}

        public MegaMenuObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY) 
		{
		}
        
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new MegaMenuRecordMap());
			base.OnModelCreating(modelBuilder);
		}
	}
}