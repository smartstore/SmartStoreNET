using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Web.Models.Catalog;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Web.Extensions
{
	public static class ProductDetailsExtensions
	{
		public static string UpdateProductDetailsUrl(this ProductDetailsModel model, string itemType = null)
		{
			var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

			string url = urlHelper.Action("UpdateProductDetails", "Catalog", new
			{
				productId = model.Id,
				bundleItemId = model.BundleItem.Id,
				itemType = itemType
			});

			return url;
		}
		public static bool RenderBundleTitle(this ProductDetailsModel model)
		{
			return model.BundleTitleText.HasValue() && model.BundledItems.Where(x => x.BundleItem.Visible).Count() > 0;
		}

		public static bool ShouldBeRendered(this ProductDetailsModel.ProductVariantAttributeModel variantAttribute)
		{
			switch (variantAttribute.AttributeControlType)
			{
				case AttributeControlType.DropdownList:
				case AttributeControlType.RadioList:
				case AttributeControlType.Checkboxes:
				case AttributeControlType.ColorSquares:
					return (variantAttribute.Values.Count > 0);
				default:
					return true;
			}
		}
		public static bool ShouldBeRendered(this IEnumerable<ProductDetailsModel.ProductVariantAttributeModel> variantAttributes)
		{
			if (variantAttributes != null)
			{
				foreach (var item in variantAttributes)
				{
					if (item.ShouldBeRendered())
						return true;
				}
			}
			return false;
		}
	}
}