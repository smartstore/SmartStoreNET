using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Models.Catalog;

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

	}
}