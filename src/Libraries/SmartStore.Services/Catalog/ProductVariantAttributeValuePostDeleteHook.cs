using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
	public class ProductVariantAttributeValuePostDeleteHook : PostDeleteHook<ProductVariantAttributeValue>
	{
		private readonly IProductAttributeService _productAttributeService;

		public ProductVariantAttributeValuePostDeleteHook(IProductAttributeService productAttributeService)
		{
			_productAttributeService = productAttributeService;
		}

		public override void Hook(ProductVariantAttributeValue entity, HookEntityMetadata metadata)
		{
			if (entity != null)
			{
				_productAttributeService.DeleteProductBundleItemAttributeFilter(entity.ProductVariantAttributeId, entity.Id);
			}
		}
	}
}
