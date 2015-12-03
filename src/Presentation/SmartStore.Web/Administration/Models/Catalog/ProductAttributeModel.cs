using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(ProductAttributeValidator))]
    public class ProductAttributeModel : EntityModelBase, ILocalizedModel<ProductAttributeLocalizedModel>
    {
        public ProductAttributeModel()
        {
            Locales = new List<ProductAttributeLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Alias")]
        public string Alias { get; set; }
        
        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}
        


        public IList<ProductAttributeLocalizedModel> Locales { get; set; }

    }

    public class ProductAttributeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}
    }
}