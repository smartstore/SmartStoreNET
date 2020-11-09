using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web
{
    public static class ProductDetailsExtensions
    {
        public static bool ShouldBeRendered(this ProductDetailsModel.ProductVariantAttributeModel variantAttribute)
        {
            switch (variantAttribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.Checkboxes:
                case AttributeControlType.Boxes:
                    return variantAttribute.Values.Count > 0;
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