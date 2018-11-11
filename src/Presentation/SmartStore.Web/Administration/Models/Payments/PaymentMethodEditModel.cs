using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodEditModel : TabbableModel, ILocalizedModel<PaymentMethodLocalizedModel>
	{
		public PaymentMethodEditModel()
		{
			Locales = new List<PaymentMethodLocalizedModel>();
			FilterConfigurationUrls = new List<string>();
		}

		public IList<PaymentMethodLocalizedModel> Locales { get; set; }
		public string IconUrl { get; set; }
		public IList<string> FilterConfigurationUrls { get; set; }

		[SmartResourceDisplayName("Common.SystemName")]
		public string SystemName { get; set; }

		[SmartResourceDisplayName("Common.FriendlyName")]
		public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ShortDescription")]
		[AllowHtml]
		public string Description { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.FullDescription")]
		[AllowHtml]
		public string FullDescription { get; set; }
	}


	public class PaymentMethodLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Common.FriendlyName")]
		public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ShortDescription")]
		[AllowHtml]
		public string Description { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.FullDescription")]
		[AllowHtml]
		public string FullDescription { get; set; }
	}
}