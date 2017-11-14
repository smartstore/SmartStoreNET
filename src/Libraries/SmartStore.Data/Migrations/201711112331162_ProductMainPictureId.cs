namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity;
	using System.Collections.Generic;
	using System.Data.Entity.Migrations;
	using Core.Domain.Catalog;
	using Setup;
	using Core;
	using Utilities;

	public partial class ProductMainPictureId : DbMigration, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
        {
            AddColumn("dbo.Product", "MainPictureId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "MainPictureId");
        }

		public bool RollbackOnFailure
		{
			get { return true; }
		}

		public void Seed(SmartObjectContext context)
		{
			DataNormalizer.FixProductMainPictureIds(context, true);

			//var query = context.Set<Product>()
			//	.AsNoTracking()
			//	.Where(x => x.MainPictureId == null)
			//	.OrderBy(x => x.Id)
			//	.Select(x => x.Id);

			//int pageIndex = 0;
			//PagedList<int> productIds = null;

			//while (true)
			//{	
			//	productIds = new PagedList<int>(query, pageIndex, 1000);

			//	var map = GetPoductPictureMap(context, productIds);

			//	using (var tx = context.Database.BeginTransaction())
			//	{
			//		foreach (var kvp in map)
			//		{
			//			context.ExecuteSqlCommand("Update [Product] Set [MainPictureId] = {0} WHERE [Id] = {1}", false, null, kvp.Value, kvp.Key);
			//		}

			//		if (map.Any())
			//		{
			//			context.SaveChanges();
			//			tx.Commit();
			//		}
			//	}

			//	if (!productIds.HasNextPage)
			//		break;
			//}
		}

		//private IDictionary<int, int> GetPoductPictureMap(SmartObjectContext context, IEnumerable<int> productIds)
		//{
		//	var map = new Dictionary<int, int>();

		//	var query = from pp in context.Set<ProductPicture>().AsNoTracking()
		//				where productIds.Contains(pp.ProductId)
		//				group pp by pp.ProductId into g
		//				select new
		//				{
		//					ProductId = g.Key,
		//					PictureIds = g.OrderBy(x => x.DisplayOrder)
		//						.Take(1)
		//						.Select(x => x.PictureId)
		//				};

		//	map = query.ToList().ToDictionary(x => x.ProductId, x => x.PictureIds.First());

		//	return map;
		//}
	}
}
