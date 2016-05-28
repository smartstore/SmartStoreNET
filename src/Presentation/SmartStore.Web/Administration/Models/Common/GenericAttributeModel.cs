﻿using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
    [Validator(typeof(GenericAttributeValidator))]
    public partial class GenericAttributeModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Common.GenericAttributes.Fields.Name")]
        public string Key { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.Common.GenericAttributes.Fields.Value")]
        public string Value { get; set; }

        public string EntityName { get; set; }

        public int EntityId { get; set; }
    }
}