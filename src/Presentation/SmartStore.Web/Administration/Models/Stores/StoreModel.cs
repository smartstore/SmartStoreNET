using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

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
	}
}