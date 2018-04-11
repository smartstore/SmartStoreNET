using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Validators.Catalog;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductReviewOverviewModel : ModelBase
    {
        public int ProductId { get; set; }

        public int RatingSum { get; set; }

        public int TotalReviews { get; set; }

        public bool AllowCustomerReviews { get; set; }
    }

    [Validator(typeof(ProductReviewsValidator))]
    public partial class ProductReviewsModel : ModelBase
    {
        public ProductReviewsModel()
        {
            Items = new List<ProductReviewModel>();
        }

        public int ProductId { get; set; }
        public LocalizedValue<string> ProductName { get; set; }
        public string ProductSeName { get; set; }

		public int TotalReviewsCount { get; set; }
        public IList<ProductReviewModel> Items { get; set; }

		#region Add

		[AllowHtml]
		[SmartResourceDisplayName("Reviews.Fields.Title")]
		public string Title { get; set; }

		[AllowHtml]
		[SmartResourceDisplayName("Reviews.Fields.ReviewText")]
		public string ReviewText { get; set; }

		[SmartResourceDisplayName("Reviews.Fields.Rating")]
		public int Rating { get; set; }

		public bool DisplayCaptcha { get; set; }

		public bool CanCurrentCustomerLeaveReview { get; set; }
		public bool SuccessfullyAdded { get; set; }
		public string Result { get; set; }

		#endregion
	}

    public partial class ProductReviewModel : EntityModelBase
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public bool AllowViewingProfiles { get; set; }
        
        public string Title { get; set; }

        public string ReviewText { get; set; }

        public int Rating { get; set; }

        public ProductReviewHelpfulnessModel Helpfulness { get; set; }

        public string WrittenOnStr { get; set; }
    }


    public partial class ProductReviewHelpfulnessModel : ModelBase
    {
        public int ProductReviewId { get; set; }

        public int HelpfulYesTotal { get; set; }

        public int HelpfulNoTotal { get; set; }
    }
}