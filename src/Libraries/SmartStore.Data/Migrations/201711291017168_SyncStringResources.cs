namespace SmartStore.Data.Migrations
{
	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using SmartStore.Collections;
	using SmartStore.Core.Domain.Localization;
	using SmartStore.Data.Setup;

	public partial class SyncStringResources : DbMigration, IDataSeeder<SmartObjectContext>
	{
		private static HashSet<string> _unusedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"Enums.SmartStore.Core.Domain.Catalog.AttributeControlType.ColorSquares",
			"Plugins.Payments.PayPal.IpnLogInfo",
			"Plugins.Widgets.TrustedShopsCustomerProtection.Prepared",
			"Plugins.Shipping.ByWeight.SmallQuantityThresholdNotReached",
			"Products.ProductNotAddedToTheCart.Link"
		};

		private static Dictionary<string, string> _missingEnglishResources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.DropoffType.BusinessServiceCenter", "Business service center" },
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.DropoffType.DropBox", "Drop box" },
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.DropoffType.RegularPickup", "Regular pickup" },
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.DropoffType.RequestCourier", "Request courier" },
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.DropoffType.Station", "Station" },
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.PackingType.PackByDimensions", "Pack by dimensions" },
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.PackingType.PackByOneItemPerPackage", "Pack by one item per package" },
			{ "Enums.SmartStore.Plugin.Shipping.Fedex.PackingType.PackByVolume", "Pack by volume" },
			{ "Order.OrderDetails", "Order Details" },
			{ "Plugins.SmartStore.AccardaKar.Amount", "Amount" },
			{ "Plugins.SmartStore.Exports.BMEcatProductXml.ExportProfileInfo", "Please click <a href=\"{0}\">here</a> to view existing BMEcat export profiles or to create a new one." },
			{ "Plugins.Description.SmartStore.DiscountRules", "Provides standard discount rules, e.g. \"Country is\", \"Customer group is\", \"Has amount x spent\" etc." },
			{ "Plugins.FriendlyName.DiscountRequirement.BillingCountryIs", "Country of invoice address is" },
			{ "Plugins.FriendlyName.DiscountRequirement.HadSpentAmount", "Customer has spent amount x" },
			{ "Plugins.FriendlyName.DiscountRequirement.HasAllProducts", "Customer has the following products in his shopping cart" },
			{ "Plugins.FriendlyName.DiscountRequirement.HasOneProduct", "Customer has one of the following products in his shopping cart" },
			{ "Plugins.FriendlyName.DiscountRequirement.MustBeAssignedToCustomerRole", "The customer must be assigned to a customer group" },
			{ "Plugins.FriendlyName.DiscountRequirement.ShippingCountryIs", "Country of delivery address is" },
			{ "Plugins.FriendlyName.DiscountRequirement.Store", "Limited to specific shop" },
			{ "Plugins.FriendlyName.SmartStore.DiscountRules", "Standard discount rules" },
			{ "Plugins.FriendlyName.DiscountRequirement.HasPaymentMethod", "Customer selected specific payment method" },
			{ "Plugins.FriendlyName.SmartStore.DiscountRules.HasPaymentMethod", "\"Customer has selected certain payment method\" discount rule" },
			{ "Plugins.FriendlyName.DiscountRequirement.HasShippingOption", "Customer selected specific shipping method" },
			{ "Plugins.FriendlyName.SmartStore.DiscountRules.HasShippingOption", "\"Customer has selected certain shipping method\" discount rule" },
			{ "Plugins.FriendlyName.DiscountRequirement.PurchasedAllProducts", "Customer has already purchased the following products" },
			{ "Plugins.FriendlyName.DiscountRequirement.PurchasedOneProduct", "The customer has already purchased one of the following products" },
			{ "Plugins.FriendlyName.SmartStore.DiscountRules.PurchasedProducts", "\"Customer has already purchased the following products\" discount rule" },
			{ "Plugins.SmartStore.GoogleRemarketing.ConversionId.Hint", "You'll find your conversion id in the admin area of google adwords" },
			{ "Plugins.Description.SmartStore.OfflinePayment", "Provides offline payment methods, e.g. direct debit, credit card, invoice, prepayment, cash on delivery etc." },
			{ "Plugins.FriendlyName.SmartStore.OfflinePayment", "Offline payment methods" },
			{ "Plugins.FriendlyName.SmartStore.OutputCache", "Output Cache" },
			{ "Plugins.Description.SmartStore.OutputCache", "Allows the temporary storage of entire shop pages and thus contributes to a considerable increase in performance." },
			{ "Plugins.FriendlyName.OutputCacheProvider.Memory", "Local memory" },
			{ "Plugins.FriendlyName.OutputCacheProvider.Database", "Database" },
			{ "Plugins.FriendlyName.SmartStore.PostFinanceECommerce", "PostFinance" },
			{ "Plugins.Description.SmartStore.PostFinanceECommerce", "Enables payment with the swiss PostFinance." },
			{ "Plugins.SmartStore.ShopConnector.FinalResult", "{0}... {1} of {2} processed. {3} success, {4} failed, {5} skipped. {6} added, {7} updated." },
			{ "Plugins.Widgets.TrustedShopsCustomerProtection.Password", "Trusted Shops password" },
			{ "Plugins.Widgets.TrustedShopsCustomerProtection.Password.Hint", "Please enter the password provided by Trusted Shops here." },
			{ "Plugins.Widgets.TrustedShopsCustomerProtection.UserName", "Trusted Shops user name" },
			{ "Plugins.Widgets.TrustedShopsCustomerProtection.UserName.Hint", "Please enter the user name provided by Trusted Shops here." },
		};

		public override void Up()
		{
		}

		public override void Down()
		{
		}

		public bool RollbackOnFailure
		{
			get { return true; }
		}

		public void Seed(SmartObjectContext context)
		{
			var resourceSet = context.Set<LocaleStringResource>();
			var allLanguages = context.Set<Language>().ToList();

			// Accidents.
			var accidents = resourceSet.Where(x => x.ResourceName == "Admin.Configuration.ActivityLog.ActivityLogTy pe").ToList();
			if (accidents.Any())
			{
				accidents.Each(x => x.ResourceName = "Admin.Configuration.ActivityLog.ActivityLogType");
				context.SaveChanges();
			}

			// Remove unused resources that could be included in the German set.
			var unusedResources = resourceSet.Where(x => _unusedNames.Contains(x.ResourceName)).ToList();
			if (unusedResources.Any())
			{
				resourceSet.RemoveRange(unusedResources);
				context.SaveChanges();
				unusedResources.Clear();
			}

			// Remove duplicate resources.
			foreach (var language in allLanguages)
			{
				var resources = resourceSet.Where(x => x.LanguageId == language.Id).ToList();
				var deleteResources = new List<LocaleStringResource>();
				var resourcesMap = new Multimap<string, LocaleStringResource>(StringComparer.OrdinalIgnoreCase);
				resources.Each(x => resourcesMap.Add(x.ResourceName, x));

				foreach (var item in resourcesMap)
				{
					if (item.Value.Count > 1)
					{
						// First is ok, rest is bad.
						foreach (var resource in item.Value.OrderByDescending(x => x.IsTouched).Skip(1))
						{
							deleteResources.Add(resource);
						}
					}
				}

				if (deleteResources.Any())
				{
					resourceSet.RemoveRange(deleteResources);
					context.SaveChanges();
					deleteResources.Clear();
				}
			}

			// Remove resources that are not included in the German set.
			// Unfortunately we cannot do that. We have no information about the origin of a resource. We would delete resources of other developers.

			// Add resources included in the German set but missing in the English set.
			var deLanguage = allLanguages.FirstOrDefault(x => x.LanguageCulture.IsCaseInsensitiveEqual("de-DE"));
			var enLanguage = allLanguages.FirstOrDefault(x => x.LanguageCulture.IsCaseInsensitiveEqual("en-US"));
			if (deLanguage != null && enLanguage != null)
			{
				var deResources = resourceSet.AsNoTracking().Where(x => x.LanguageId == deLanguage.Id).ToList();

				var enNames = resourceSet
					.Where(x => x.LanguageId == enLanguage.Id)
					.Select(x => x.ResourceName)
					.Distinct()
					.ToList();
				var enNamesSet = new HashSet<string>(enNames, StringComparer.OrdinalIgnoreCase);

				foreach (var resource in deResources)
				{
					if (!enNames.Contains(resource.ResourceName) && _missingEnglishResources.TryGetValue(resource.ResourceName, out string value))
					{
						resourceSet.Add(new LocaleStringResource
						{
							LanguageId = enLanguage.Id,
							ResourceName = resource.ResourceName,
							ResourceValue = value,
							IsFromPlugin = resource.IsFromPlugin
						});
					}
				}

				context.SaveChanges();
				deResources.Clear();
			}
		}
	}
}
