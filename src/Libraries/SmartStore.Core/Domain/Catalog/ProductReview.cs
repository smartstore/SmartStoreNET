using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product review
    /// </summary>
    public partial class ProductReview : CustomerContent
    {
        private ICollection<ProductReviewHelpfulness> _productReviewHelpfulnessEntries;

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the review text
        /// </summary>
        public string ReviewText { get; set; }

        /// <summary>
        /// Review rating
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Review helpful votes total
        /// </summary>
        public int HelpfulYesTotal { get; set; }

        /// <summary>
        /// Review not helpful votes total
        /// </summary>
        public int HelpfulNoTotal { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// Gets the entries of product review helpfulness
        /// </summary>
        public virtual ICollection<ProductReviewHelpfulness> ProductReviewHelpfulnessEntries
        {
			get { return _productReviewHelpfulnessEntries ?? (_productReviewHelpfulnessEntries = new HashSet<ProductReviewHelpfulness>()); }
            protected set { _productReviewHelpfulnessEntries = value; }
        }
    }
}
