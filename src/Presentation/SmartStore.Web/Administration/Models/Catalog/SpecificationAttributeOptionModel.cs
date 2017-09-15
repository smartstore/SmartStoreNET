using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(SpecificationAttributeOptionValidator))]
    public class SpecificationAttributeOptionModel : EntityModelBase, ILocalizedModel<SpecificationAttributeOptionLocalizedModel>
    {
        public SpecificationAttributeOptionModel()
        {
            Locales = new List<SpecificationAttributeOptionLocalizedModel>();
        }

        public int SpecificationAttributeId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Alias")]
		public string Alias { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}
        
        public IList<SpecificationAttributeOptionLocalizedModel> Locales { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Multiple")]
		public bool Multiple { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.NumberValue")]
		public decimal NumberValue { get; set; }
	}

    public class SpecificationAttributeOptionLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

		[AllowHtml, SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Alias")]
		public string Alias { get; set; }
	}
}