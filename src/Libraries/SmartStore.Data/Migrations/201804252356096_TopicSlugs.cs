namespace SmartStore.Data.Migrations
{
    using System;
	using System.Linq;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Topics;
	using SmartStore.Data.Setup;
	using SmartStore.Core.Domain.Seo;
	using SmartStore.Utilities;

	public partial class TopicSlugs : DbMigration, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
        }
        
        public override void Down()
        {
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			var allTopics = context.Set<Topic>()
				.AsNoTracking()
				.Select(x => new { x.Id, x.SystemName, x.Title })
				.ToList();

			var urlRecords = context.Set<UrlRecord>();

			foreach (var topic in allTopics)
			{
				var slug = SeoHelper.GetSeName(topic.SystemName, true, false).Truncate(400);
				int i = 2;
				var tempSlug = slug;

				while (urlRecords.Any(x => x.Slug == tempSlug))
				{
					tempSlug = string.Format("{0}-{1}", slug, i);
					i++;
				}

				slug = tempSlug;

				var ur = urlRecords.FirstOrDefault(x => x.LanguageId == 0 && x.EntityName == "Topic" && x.EntityId == topic.Id);
				if (ur != null)
				{
					ur.Slug = slug;
				}
				else
				{
					urlRecords.Add(new UrlRecord
					{
						EntityId = topic.Id,
						EntityName = "Topic",
						IsActive = true,
						LanguageId = 0,
						Slug = slug
					});
				}

				context.SaveChanges();
			}	
		}
	}
}
