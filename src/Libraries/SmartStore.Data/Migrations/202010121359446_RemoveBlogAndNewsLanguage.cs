namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Blogs;
    using SmartStore.Core.Domain.Localization;
    using SmartStore.Core.Domain.News;
    using SmartStore.Core.Domain.Seo;
    using SmartStore.Data.Setup;

    public partial class RemoveBlogAndNewsLanguage : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            DropForeignKey("dbo.BlogPost", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.News", "LanguageId", "dbo.Language");

            if (DataSettings.Current.IsSqlServer)
            {
                Sql("IF (OBJECT_ID('BlogPost_Language', 'F') IS NOT NULL) ALTER TABLE [dbo].[BlogPost] DROP CONSTRAINT [BlogPost_Language];");
                Sql("IF (OBJECT_ID('NewsItem_Language', 'F') IS NOT NULL) ALTER TABLE [dbo].[News] DROP CONSTRAINT [NewsItem_Language];");

                // See Indexes.sql.
                Sql("IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_BlogPost_LanguageId' AND object_id = OBJECT_ID('[dbo].[BlogPost]')) DROP INDEX [IX_BlogPost_LanguageId] ON [dbo].[BlogPost];");
                Sql("IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_News_LanguageId' AND object_id = OBJECT_ID('[dbo].[News]')) DROP INDEX [IX_News_LanguageId] ON [dbo].[News];");
            }
            else
            {
                DropIndex("dbo.BlogPost", "IX_BlogPost_LanguageId");
                DropIndex("dbo.News", "IX_News_LanguageId");
            }

            DropIndex("dbo.BlogPost", new[] { "LanguageId" });
            DropIndex("dbo.News", new[] { "LanguageId" });
            DropColumn("dbo.BlogPost", "LanguageId");
            DropColumn("dbo.News", "LanguageId");
        }
        
        public override void Down()
        {
            var defaultLanguageId = 1;

            if (DataSettings.DatabaseIsInstalled())
            {
                defaultLanguageId = new SmartObjectContext().Set<Language>().Select(x => x.Id).FirstOrDefault();
            }

            AddColumn("dbo.News", "LanguageId", c => c.Int(nullable: false, defaultValue: defaultLanguageId));
            AddColumn("dbo.BlogPost", "LanguageId", c => c.Int(nullable: false, defaultValue: defaultLanguageId));
            CreateIndex("dbo.News", "LanguageId");
            CreateIndex("dbo.BlogPost", "LanguageId");
            AddForeignKey("dbo.News", "LanguageId", "dbo.Language", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BlogPost", "LanguageId", "dbo.Language", "Id", cascadeDelete: true);
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
            {
                return;
            }

            // URL records of BlogPost and NewsItem must now have 0 for "Standard" as language ID otherwise no link is created.
            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                var urlRecords = context.Set<UrlRecord>();
                var entityNames = new[] { nameof(BlogPost), nameof(NewsItem) };

                foreach (var entityName in entityNames)
                {
                    var allEntityIds = urlRecords
                        .Where(x => x.EntityName == entityName)
                        .Select(x => x.EntityId)
                        .Distinct()
                        .ToList();

                    foreach (var chunk in allEntityIds.Slice(200))
                    {
                        var entities = urlRecords
                            .Where(x => x.EntityName == entityName && chunk.Contains(x.EntityId))
                            .ToList();
                        var entitiesMap = entities.ToMultimap(x => x.EntityId, x => x);

                        foreach (var kvp in entitiesMap)
                        {
                            if (!kvp.Value.Any(x => x.LanguageId == 0))
                            {
                                // Migrate active and inactive slugs.
                                kvp.Value.Where(x => x.LanguageId != 0).Each(x => x.LanguageId = 0);
                            }
                        }

                        scope.Commit();
                        context.DetachEntities<UrlRecord>();
                    }
                }
            }
        }
    }
}
