using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
	public class ProductAttributeOptionsSetModel : EntityModelBase
	{
		public int ProductAttributeId { get; set; }
		public string Name { get; set; }
	}
}