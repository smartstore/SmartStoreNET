
namespace SmartStore.Core.Domain.Catalog
{
	public partial class ProductBundleItemAttributeFilter : BaseEntity
	{
		/// <summary>
		/// Gets or sets the product bundle item identifier
		/// </summary>
		public int BundleItemId { get; set; }

		/// <summary>
		/// Gets or sets the product attribute identifier
		/// </summary>
		public int AttributeId { get; set; }

		/// <summary>
		/// Gets or sets the product attribute value identifier
		/// </summary>
		public int AttributeValueId { get; set; }

		/// <summary>
		/// Gets or sets whether the filtered value is preselected
		/// </summary>
		public bool IsPreSelected { get; set; }

		/// <summary>
		/// Gets the product bundle item
		/// </summary>
		public virtual ProductBundleItem BundleItem { get; set; }
	}
}
