namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Customers;
	using SmartStore.Data.Setup;

	public partial class PdfConverterUser : DbMigration, IDataSeeder<SmartObjectContext>
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
			context.Set<Customer>().AddOrUpdate(x => x.SystemName,
				new Customer
				{
					Email = "builtin@pdf-converter-record.com",
					CustomerGuid = Guid.NewGuid(),
					PasswordFormat = PasswordFormat.Clear,
					AdminComment = "Built-in system record used for the PDF converter.",
					Active = true,
					IsSystemAccount = true,
					SystemName = SystemCustomerNames.PdfConverter,
					CreatedOnUtc = DateTime.UtcNow,
					LastActivityDateUtc = DateTime.UtcNow,
				}
			);
		}
	}
}
