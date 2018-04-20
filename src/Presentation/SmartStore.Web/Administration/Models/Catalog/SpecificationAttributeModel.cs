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
	[Validator(typeof(SpecificationAttributeValidator))]
    public class SpecificationAttributeModel : EntityModelBase, ILocalizedModel<SpecificationAttributeLocalizedModel>
    {
        public SpecificationAttributeModel()
        {
            Locales = new List<SpecificationAttributeLocalizedModel>();
			AllowFiltering = true;
			ShowOnProductPage = true;
		}

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Alias")]
		public string Alias { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.DisplayOrder")]
		public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.AllowFiltering")]
		public bool AllowFiltering { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.ShowOnProductPage")]
		public bool ShowOnProductPage { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.FacetSorting")]
		public FacetSorting FacetSorting { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.FacetTemplateHint")]
		public FacetTemplateHint FacetTemplateHint { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.IndexOptionNames")]
        public bool IndexOptionNames { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.OptionsCount")]
		public int OptionCount { get; set; }

        public IList<SpecificationAttributeLocalizedModel> Locales { get; set; }
    }

    public class SpecificationAttributeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Alias")]
		public string Alias { get; set; }
	}
}