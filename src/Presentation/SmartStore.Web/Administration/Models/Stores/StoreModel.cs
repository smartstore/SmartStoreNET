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

		[SmartResourceDisplayName("Admin.Configuration.Stores.Fields.DisplayOrder")]
		public int DisplayOrder { get; set; }
	}
}