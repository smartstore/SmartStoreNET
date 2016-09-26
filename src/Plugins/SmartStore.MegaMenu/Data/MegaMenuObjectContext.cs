using System.Data.Entity;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.MegaMenu.Data.Migrations;
using SmartStore.MegaMenu.Domain;

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
            modelBuilder.Entity<MegaMenuRecord>();
            
            base.OnModelCreating(modelBuilder);
        }
	}
}