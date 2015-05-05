namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Customers;
	using SmartStore.Data.Setup;

	public partial class PdfConverterRoleFix : DbMigration, IDataSeeder<SmartObjectContext>
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
			var pdfUser = context.Set<Customer>().Include(x => x.CustomerRoles).FirstOrDefault(x => x.SystemName == SystemCustomerNames.PdfConverter);

			if (pdfUser == null)
				return;

			if (!pdfUser.CustomerRoles.Any())
			{
				var guestRole = pdfUser.CustomerRoles.FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests);
				if (guestRole == null)
				{
					guestRole = context.Set<CustomerRole>().FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests);
					if (guestRole != null)
					{
						pdfUser.CustomerRoles.Add(guestRole);
						context.SaveChanges();
					}
				}
			}
		}
	}
}
