using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Stores
{
	[Validator(typeof(StoreValidator))]
	public partial class StoreModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
		[AllowHtml]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.Url")]
		[AllowHtml]
		public string Url { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.SslEnabled")]
		public virtual bool SslEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.SecureUrl")]
		[AllowHtml]
		public virtual string SecureUrl { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.Hosts")]
		[AllowHtml]
		public string Hosts { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.StoreLogo")]
		[UIHint("Picture")]
		public int LogoPictureId { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.DisplayOrder")]
		public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.HtmlBodyId")]
		public string HtmlBodyId { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.ContentDeliveryNetwork")]
	    [AllowHtml]
	    public string ContentDeliveryNetwork { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryStoreCurrencyId")]
		public int PrimaryStoreCurrencyId { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryStoreCurrencyId")]
		public string PrimaryStoreCurrencyName
		{
			get
			{
				try
				{
					return AvailableCurrencies.First(x => x.Value == PrimaryStoreCurrencyId.ToString()).Text;
				}
				catch { }

				return null;
			}
		}

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryExchangeRateCurrencyId")]
		public int PrimaryExchangeRateCurrencyId { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryExchangeRateCurrencyId")]
		public string PrimaryExchangeRateCurrencyName
		{
			get
			{
				try
				{
					return AvailableCurrencies.First(x => x.Value == PrimaryExchangeRateCurrencyId.ToString()).Text;
				}
				catch { }

				return null;
			}
		}

		public List<SelectListItem> AvailableCurrencies { get; set; }
	}
}