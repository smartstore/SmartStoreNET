using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Core.Search.Facets;
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

        [AllowHtml, SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Alias")]
		public string Alias { get; set; }
        
        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Description")]
        [AllowHtml]
        public string Description {get; set;}

		[SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.AllowFiltering")]
		public bool AllowFiltering { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.FacetTemplateHint")]
		public FacetTemplateHint FacetTemplateHint { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.IndexOptionNames")]
        public bool IndexOptionNames { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.ExportMappings")]
		public string ExportMappings { get; set; }

		public IList<ProductAttributeLocalizedModel> Locales { get; set; }
    }

    public class ProductAttributeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Alias")]
		public string Alias { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.ProductAttributes.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}
    }
}