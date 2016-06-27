namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class AddSyncMapping : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SyncMapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityId = c.Int(nullable: false),
                        SourceKey = c.String(nullable: false, maxLength: 150),
                        EntityName = c.String(nullable: false, maxLength: 100),
                        ContextName = c.String(nullable: false, maxLength: 100),
                        SourceHash = c.String(maxLength: 40),
                        CustomInt = c.Int(),
                        CustomString = c.String(),
                        CustomBool = c.Boolean(),
                        SyncedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.EntityId, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_ByEntity")
                .Index(t => new { t.SourceKey, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_BySource");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_BySource");
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_ByEntity");
            DropTable("dbo.SyncMapping");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			string attachHint = "A file that is to be appended to each sent email (eg Terms, Conditions etc.)";
			string attachHintDe = "Eine Datei, die jedem gesendeten E-Mail angehangen werden soll (z.B. AGB, Widerrufsbelehrung etc.)";
			
			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Fields.Attachment1FileId",
				"Attachment 1",
				"Anhang 1",
				attachHint,
				attachHintDe);
			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Fields.Attachment2FileId",
				"Attachment 2",
				"Anhang 2",
				attachHint,
				attachHintDe);
			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Fields.Attachment3FileId",
				"Attachment 3",
				"Anhang 3",
				attachHint,
				attachHintDe);

			builder.AddOrUpdate("Common.FileUploader.EnterUrl",
				"Enter URL",
				"URL eingeben");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerNumberEnabled",
                "Save customer number",
                "Kundennummer speichern",
                "Specifies whether customer numbers can be saved.",
                "Bestimmt ob Kundennummern hinterlegt werden können.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.DisplayCustomerNumber",
                "Display customer numbers in frontend",
                "Kundennummern im Frontend anzeigen",
                "Specifies whether customer numbers will be displayed to customers in their account area.",
                "Bestimmt ob Kunden ihre Kundennummer in Ihrem Account-Bereich einsehen können.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerCanEditNumberIfEmpty",
                "Customers can enter a customer number",
                "Kunden können Kundennummer hinterlegen",
                "Specifies whether customers can enter a customer number if the customer number doesn't contain a value yet.",
                "Bestimmt ob Kunden eine Kundennummer angeben können, wenn für diese noch kein Wert hinterlegt wurde.");

            builder.AddOrUpdate("Common.FreeShipping",
                "Free shipping",
                "Versandkostenfrei");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.ExtraRobotsDisallows",
                "Extra Disallows for robots.txt",
                "Extra Disallows für robots.txt",
                "Enter additional paths that should be included as Disallow entries in your robots.txt. Each entry has to be entered in a new line.",
                "Geben Sie hier zusätzliche Pfade an, die als Disallow-Einträge zur robots.txt hinzugefügt werden sollen. Jeder Eintrag muss in einer neuen Zeile erfolgen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DefaultSortOrderMode",
                "Default product sort order",
                "Standardsortierreihenfolge für Produkte",
                "Specifies the default product sort order.",
                "Legt die Standardsortierreihenfolge für Produkte fest.");

            builder.AddOrUpdate("Common.CustomerNumberAlreadyExists",
                "Customer number already exists, please choose another.",
				"Die von Ihnen gewählte Kundennummer existiert bereits. Bitte geben Sie eine andere Kundennummer an.");
		}
    }
}
