using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Tax
{
    [Validator(typeof(TaxCategoryValidator))]
    public class TaxCategoryModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.Tax.Categories.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Tax.Categories.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}