using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Web.Models.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media;

namespace SmartStore.Web
{
	public static class ProductDetailsExtensions
	{		
		public static Picture GetAssignedPicture(this ProductDetailsModel model, IPictureService pictureService, IList<Picture> pictures, int productId = 0)
		{
			if (model != null && model.SelectedCombination != null)
			{
				Picture picture = null;
				var combiAssignedImages = model.SelectedCombination.GetAssignedPictureIds();

				if (combiAssignedImages.Length > 0)
				{
					if (pictures == null)
						picture = pictureService.GetPictureById(combiAssignedImages[0]);
					else
						picture = pictures.FirstOrDefault(p => p.Id == combiAssignedImages[0]);
				}

				if (picture == null && productId != 0)
				{
					picture = pictureService.GetPicturesByProductId(productId, 1).FirstOrDefault();
				}
				return picture;
			}
			return null;
		}

		public static string GetAttributeValueInfo(this ProductDetailsModel.ProductVariantAttributeValueModel model)
		{
			string result = "";

			if (model.PriceAdjustment.HasValue())
				result = " ({0})".FormatWith(model.PriceAdjustment);

			if (model.QuantityInfo > 1)
				return " × {1}".FormatWith(result, model.QuantityInfo) + result;

			return result;
		}

		public static bool ShouldBeRendered(this ProductDetailsModel.ProductVariantAttributeModel variantAttribute)
		{
			switch (variantAttribute.AttributeControlType)
			{
				case AttributeControlType.DropdownList:
				case AttributeControlType.RadioList:
				case AttributeControlType.Checkboxes:
				case AttributeControlType.Boxes:
					return (variantAttribute.Values.Count > 0);
				default:
					return true;
			}
		}

		public static bool ShouldBeRendered(this IEnumerable<ProductDetailsModel.ProductVariantAttributeModel> variantAttributes)
		{
			return variantAttributes?.FirstOrDefault(x => x.ShouldBeRendered()) != null;
		}
	}
}