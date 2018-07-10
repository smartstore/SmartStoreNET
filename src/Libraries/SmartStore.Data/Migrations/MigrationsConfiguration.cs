namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Setup;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			Seed(context);
		}

		protected override void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);

			context.SaveChanges();
        }

		public void MigrateSettings(SmartObjectContext context)
		{

		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.ReturnRequests.MaxRefundAmount",
				"Maximum refund amount",
				"Maximaler Erstattungsbetrag",
				"The maximum amount that can be refunded for this return request.",
				"Der maximale Betrag, der für diesen Rücksendewunsch erstattet werden kann.");

			builder.AddOrUpdate("Admin.Customers.Customers.Fields.Title",
				"Title",
				"Titel",
				"Specifies the title.",
				"Legt den Titel fest.");

            builder.AddOrUpdate("Admin.DataExchange.Export.FolderName.Validate",
                "Please enter a valid, relative folder path for the export data. The path must be at least 3 characters long and not the application folder.",
                "Bitte einen gültigen, relativen Ordnerpfad für die zu exportierenden Daten eingeben. Der Pfad muss mindestens 3 Zeichen lang und nicht der Anwendungsordner sein.");

            builder.AddOrUpdate("Admin.Catalog.Customers.CustomerSearchType", "Search in:", "Suche in:");

			// Get rid of duplicate validator resource entries
			builder.Delete(
				"Admin.Catalog.Products.Fields.Name.Required",
				"Admin.Catalog.Categories.Fields.Name.Required",
				"Admin.Catalog.Manufacturers.Fields.Name.Required",
				"Admin.Validation.RequiredField",
				"Admin.Catalog.Attributes.ProductAttributes.Fields.Name.Required",
				"Admin.Catalog.ProductReviews.Fields.Title.Required",
				"Admin.Catalog.ProductReviews.Fields.ReviewText.Required",
				"Admin.Catalog.ProductTags.Fields.Name.Required",
				"Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name.Required",
				"Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity.GreaterOrEqualToOne",
				"Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name.Required",
				"Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name.Required",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.Title.Required",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.Body.Required",
                "Admin.Common.GenericAttributes.Fields.Name.Required",
                "Admin.Customers.CustomerRoles.Fields.Name.Required",
                "Admin.Configuration.Countries.Fields.Name.Required",
                "Admin.Configuration.Countries.Fields.TwoLetterIsoCode.Required",
                "Admin.Configuration.Countries.Fields.TwoLetterIsoCode.Length",
                "Admin.Configuration.Countries.Fields.ThreeLetterIsoCode.Required",
                "Admin.Configuration.Countries.Fields.ThreeLetterIsoCode.Length",
                "Admin.Configuration.Measures.Dimensions.Fields.Name.Required",
                "Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword.Required",
                "Admin.Configuration.Measures.Weights.Fields.Name.Required",
                "Admin.Configuration.Measures.Weights.Fields.SystemKeyword.Required",
                "Admin.Configuration.Countries.States.Fields.Name.Required",
                "Admin.Configuration.DeliveryTimes.Fields.Name.Required",
                "Admin.Configuration.DeliveryTimes.Fields.ColorHexValue.Required",
                "Admin.Configuration.DeliveryTimes.Fields.ColorHexValue.Range",
                "Admin.Configuration.DeliveryTimes.Fields.Name.Range",
                "Admin.Configuration.Currencies.Fields.Name.Required",
                "Admin.Configuration.Currencies.Fields.Name.Range",
                "Admin.Configuration.Currencies.Fields.CurrencyCode.Required",
                "Admin.Configuration.Currencies.Fields.CurrencyCode.Range",
                "Admin.Configuration.Currencies.Fields.Rate.Range",
                "Admin.Configuration.Currencies.Fields.CustomFormatting.Validation",
                "Admin.Promotions.Discounts.Fields.Name.Required",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.Name.Required",
                "Admin.ContentManagement.Forums.Forum.Fields.Name.Required",
                "Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId.Required",
                "Admin.Configuration.Languages.Resources.Fields.Name.Required",
                "Admin.Configuration.Languages.Resources.Fields.Value.Required",
                "Admin.Configuration.Languages.Fields.Name.Required",
                "Admin.Configuration.Languages.Fields.UniqueSeoCode.Required",
                "Admin.Configuration.Languages.Fields.UniqueSeoCode.Length",
                "Admin.Promotions.Campaigns.Fields.Name.Required",
                "Admin.Promotions.Campaigns.Fields.Subject.Required",
                "Admin.Promotions.Campaigns.Fields.Body.Required",
                "Admin.ContentManagement.MessageTemplates.Fields.Subject.Required",
                "Admin.ContentManagement.MessageTemplates.Fields.Body.Required",
                "Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required",
                "Admin.System.QueuedEmails.Fields.Priority.Required",
                "Admin.System.QueuedEmails.Fields.From.Required",
                "Admin.System.QueuedEmails.Fields.To.Required",
                "Admin.System.QueuedEmails.Fields.SentTries.Required",
                "Admin.System.QueuedEmails.Fields.Priority.Range",
                "Admin.System.QueuedEmails.Fields.SentTries.Range",
                "Admin.ContentManagement.News.NewsItems.Fields.Title.Required",
                "Admin.ContentManagement.News.NewsItems.Fields.Short.Required",
                "Admin.ContentManagement.News.NewsItems.Fields.Full.Required",
                "Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name.Required",
                "Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name.Required",
                "Admin.Configuration.Plugins.Fields.FriendlyName.Required",
                "Admin.ContentManagement.Polls.Answers.Fields.Name.Required",
                "Admin.ContentManagement.Polls.Fields.Name.Required",
                "Admin.Configuration.Shipping.Methods.Fields.Name.Required",
                "Admin.Configuration.Stores.Fields.Name.Required",
                "Admin.Configuration.Stores.Fields.Url.Required",
                "Admin.Configuration.Settings.AllSettings.Fields.Name.Required",
                "Admin.System.ScheduleTasks.Name.Required",
                "Admin.Configuration.Tax.Categories.Fields.Name.Required",
                "Admin.ContentManagement.Topics.Fields.SystemName.Required",
                "Admin.Address.Fields.FirstName.Required",
                "Admin.Address.Fields.LastName.Required",
                "Admin.Address.Fields.Email.Required",
                "Admin.Address.Fields.Company.Required",
                "Admin.Address.Fields.City.Required",
                "Admin.Address.Fields.Address1.Required",
                "Admin.Address.Fields.Address2.Required",
                "Admin.Address.Fields.ZipPostalCode.Required",
                "Admin.Address.Fields.PhoneNumber.Required",
                "Admin.Address.Fields.FaxNumber.Required",
                "Admin.Address.Fields.EmailMatch.Required",
                "Admin.Customers.Customers.Fields.FirstName.Required",
                "Admin.Customers.Customers.Fields.LastName.Required",
                "Admin.Customers.Customers.Fields.Company.Required",
                "Admin.Customers.Customers.Fields.StreetAddress.Required",
                "Admin.Customers.Customers.Fields.StreetAddress2.Required",
                "Admin.Customers.Customers.Fields.ZipPostalCode.Required",
                "Admin.Customers.Customers.Fields.City.Required",
                "Admin.Customers.Customers.Fields.Phone.Required",
                "Admin.Customers.Customers.Fields.Fax.Required",
                "Admin.Validation.Name",
                "Admin.Validation.EmailAddress",
                "Admin.Validation.Url",
                "Admin.Validation.UsernamePassword",
                "Admin.DataExchange.Export.FileNamePattern.Validate",
                "Admin.DataExchange.Export.Partition.Validate",
                "Admin.Common.WrongEmail"
            );
        }
    }
}
