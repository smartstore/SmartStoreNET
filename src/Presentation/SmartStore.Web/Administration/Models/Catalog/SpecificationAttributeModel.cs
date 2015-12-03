using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
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
        }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}

        public IList<SpecificationAttributeLocalizedModel> Locales { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.OptionsCount")]
		public int OptionCount { get; set; }
    }

    public class SpecificationAttributeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}