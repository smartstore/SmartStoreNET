using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Orders
{
	/// <summary>
	/// Checkout attribute service
	/// </summary>
	public partial interface ICheckoutAttributeService
    {
        #region Checkout attributes

        /// <summary>
        /// Deletes a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        void DeleteCheckoutAttribute(CheckoutAttribute checkoutAttribute);

		/// <summary>
		/// Gets checkout attributes
		/// </summary>
		/// <param name="storeId">Whether to filter result by store identifier</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Checkout attributes query</returns>
		IQueryable<CheckoutAttribute> GetCheckoutAttributes(int storeId = 0, bool showHidden = false);

		/// <summary>
		/// Gets all checkout attributes
		/// </summary>
		/// <param name="storeId">Whether to filter result by store identifier</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Checkout attribute collection</returns>
		IList<CheckoutAttribute> GetAllCheckoutAttributes(int storeId = 0, bool showHidden = false);

        /// <summary>
        /// Gets a checkout attribute 
        /// </summary>
        /// <param name="checkoutAttributeId">Checkout attribute identifier</param>
        /// <returns>Checkout attribute</returns>
        CheckoutAttribute GetCheckoutAttributeById(int checkoutAttributeId);

        /// <summary>
        /// Inserts a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        void InsertCheckoutAttribute(CheckoutAttribute checkoutAttribute);

        /// <summary>
        /// Updates the checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        void UpdateCheckoutAttribute(CheckoutAttribute checkoutAttribute);

        #endregion

        #region Checkout variant attribute values

        /// <summary>
        /// Deletes a checkout attribute value
        /// </summary>
        /// <param name="checkoutAttributeValue">Checkout attribute value</param>
        void DeleteCheckoutAttributeValue(CheckoutAttributeValue checkoutAttributeValue);

        /// <summary>
        /// Gets checkout attribute values by checkout attribute identifier
        /// </summary>
        /// <param name="checkoutAttributeId">The checkout attribute identifier</param>
        /// <returns>Checkout attribute value collection</returns>
        IList<CheckoutAttributeValue> GetCheckoutAttributeValues(int checkoutAttributeId);
        
        /// <summary>
        /// Gets a checkout attribute value
        /// </summary>
        /// <param name="checkoutAttributeValueId">Checkout attribute value identifier</param>
        /// <returns>Checkout attribute value</returns>
        CheckoutAttributeValue GetCheckoutAttributeValueById(int checkoutAttributeValueId);

        /// <summary>
        /// Inserts a checkout attribute value
        /// </summary>
        /// <param name="checkoutAttributeValue">Checkout attribute value</param>
        void InsertCheckoutAttributeValue(CheckoutAttributeValue checkoutAttributeValue);

        /// <summary>
        /// Updates the checkout attribute value
        /// </summary>
        /// <param name="checkoutAttributeValue">Checkout attribute value</param>
        void UpdateCheckoutAttributeValue(CheckoutAttributeValue checkoutAttributeValue);
        
        #endregion
    }
}
