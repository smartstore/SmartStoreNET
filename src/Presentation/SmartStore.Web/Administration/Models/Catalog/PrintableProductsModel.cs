using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public PrintableProductModel()
        {
            SpecificationAttributes = new List<PrintableProductSpecificationModel>();
            BundledItems = new List<PrintableProductModel>();
            AssociatedProducts = new List<PrintableProductModel>();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string PictureUrl { get; set; }

        public string Sku { get; set; }
        public string Price { get; set; }
        public string Manufacturer { get; set; }
        public string Weight { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }

        public IList<PrintableProductSpecificationModel> SpecificationAttributes { get; set; }

        public ProductType ProductType { get; set; }
        public IList<PrintableProductModel> BundledItems { get; set; }
        public IList<PrintableProductModel> AssociatedProducts { get; set; }
    }

    public partial class PrintableProductSpecificationModel : ModelBase
    {
        public int SpecificationAttributeId { get; set; }

        public string SpecificationAttributeName { get; set; }

        public string SpecificationAttributeOption { get; set; }
    }
}