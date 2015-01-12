using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AutoMapper;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Models.Catalog
{
    public partial class PrintableProductsModel : EntityModelBase
    {
        public PrintableProductsModel()
        {
            MerchantCompanyInfo = new CompanyInformationSettings();
            MerchantContactData = new ContactDataSettings();
            Products = new List<PrintableProductModel>();
        }

        public bool PdfMode { get; set; }
        public string PrintLogoUrl { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }

        public CompanyInformationSettings MerchantCompanyInfo { get; set; }
        public ContactDataSettings MerchantContactData { get; set; }
        public IList<PrintableProductModel> Products { get; set; }
    }

    public partial class PrintableProductModel : EntityModelBase
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string PictureUrl { get; set; }
    }
}