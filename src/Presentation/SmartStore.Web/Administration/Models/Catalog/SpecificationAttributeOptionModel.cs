using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
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
        public string NameString { get; set; }

        [AllowHtml, SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Alias")]
        public string Alias { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<SpecificationAttributeOptionLocalizedModel> Locales { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Multiple")]
        public bool Multiple { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.NumberValue")]
        public decimal NumberValue { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.ColorSquaresRgb")]
        [AllowHtml, UIHint("Color")]
        public string Color { get; set; }

        [UIHint("Media"), AdditionalMetadata("album", "catalog")]
        [SmartResourceDisplayName("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Picture")]
        public int PictureId { get; set; }
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

    public partial class SpecificationAttributeOptionValidator : AbstractValidator<SpecificationAttributeOptionModel>
    {
        public SpecificationAttributeOptionValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public class SpecificationAttributeOptionMapper :
        IMapper<SpecificationAttributeOption, SpecificationAttributeOptionModel>,
        IMapper<SpecificationAttributeOptionModel, SpecificationAttributeOption>
    {
        public void Map(SpecificationAttributeOption from, SpecificationAttributeOptionModel to)
        {
            MiniMapper.Map(from, to);
            to.PictureId = from.MediaFileId;
        }

        public void Map(SpecificationAttributeOptionModel from, SpecificationAttributeOption to)
        {
            MiniMapper.Map(from, to);
            to.MediaFileId = from.PictureId;
        }
    }
}