using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(ProductReviewValidator))]
    public class ProductReviewModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Product")]
        public int ProductId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Product")]
        public string ProductName { get; set; }
		public string ProductTypeName { get; set; }
		public string ProductTypeLabelHint { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Customer")]
        public int CustomerId { get; set; }
		public string CustomerName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.IPAddress")]
        public string IpAddress { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Title")]
        public string Title { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.ReviewText")]
        public string ReviewText { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Rating")]
        public int Rating { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.IsApproved")]
        public bool IsApproved { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}