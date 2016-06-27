﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Directory;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Directory
{
    public class QuantityUnitModel : EntityModelBase, ILocalizedModel<QuantityUnitLocalizedModel>
    {
        public QuantityUnitModel()
        {
            Locales = new List<QuantityUnitLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Configuration.QuantityUnit.Fields.Name")]
        public string Name { get; set; }

		[SmartResourceDisplayName("Common.Description")]
        [AllowHtml]
        public string Description { get; set; }

		[SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.QuantityUnit.Fields.IsDefault")]
        public bool IsDefault { get; set; }

        public IList<QuantityUnitLocalizedModel> Locales { get; set; }
    }

    public class QuantityUnitLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.QuantityUnit.Fields.Name")]
        public string Name { get; set; }

		[SmartResourceDisplayName("Common.Description")]
        public string Description { get; set; }
    }
}