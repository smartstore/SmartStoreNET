using System.Collections.Generic;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Shipping
{
    /// <summary>
    /// Shipping service interface
    /// </summary>
    public partial interface IShippingService
    {
        /// <summary>
        /// Load active shipping rate computation methods
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
		IEnumerable<Provider<IShippingRateComputationMethod>> LoadActiveShippingRateComputationMethods(int storeId = 0);

        /// <summary>
        /// Load shipping rate computation method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found Shipping rate computation method</returns>
		Provider<IShippingRateComputationMethod> LoadShippingRateComputationMethodBySystemName(string systemName, int storeId = 0);

        /// <summary>
        /// Load all shipping rate computation methods
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
		IEnumerable<Provider<IShippingRateComputationMethod>> LoadAllShippingRateComputationMethods(int storeId = 0);




        /// <summary>
        /// Deletes a shipping method
        /// </summary>
        /// <param name="shippingMethod">The shipping method</param>
        void DeleteShippingMethod(ShippingMethod shippingMethod);

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>Shipping method</returns>
        ShippingMethod GetShippingMethodById(int shippingMethodId);


        /// <summary>
        /// Gets all shipping methods
        /// </summary>
		/// <param name="customer">Filter shipping methods by customer and apply payment method restrictions; null to load all records</param>
        /// <returns>Shipping method collection</returns>
		IList<ShippingMethod> GetAllShippingMethods(Customer customer = null);

        /// <summary>
        /// Inserts a shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        void InsertShippingMethod(ShippingMethod shippingMethod);

        /// <summary>
        /// Updates the shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        void UpdateShippingMethod(ShippingMethod shippingMethod);


        /// <summary>
        /// Gets shopping cart item weight (of one item)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
		decimal GetShoppingCartItemWeight(OrganizedShoppingCartItem shoppingCartItem);


        /// <summary>
        /// Gets shopping cart item total weight
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
		decimal GetShoppingCartItemTotalWeight(OrganizedShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Gets shopping cart weight
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Shopping cart weight</returns>
		decimal GetShoppingCartTotalWeight(IList<OrganizedShoppingCartItem> cart);
        
        /// <summary>
        /// Create shipment package from shopping cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <returns>Shipment package</returns>
		GetShippingOptionRequest CreateShippingOptionRequest(IList<OrganizedShoppingCartItem> cart, Address shippingAddress);

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="allowedShippingRateComputationMethodSystemName">Filter by shipping rate computation method identifier; null to load shipping options of all shipping rate computation methods</param>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Shipping options</returns>
		GetShippingOptionResponse GetShippingOptions(IList<OrganizedShoppingCartItem> cart, Address shippingAddress,
			string allowedShippingRateComputationMethodSystemName = "", int storeId = 0);
    }
}
