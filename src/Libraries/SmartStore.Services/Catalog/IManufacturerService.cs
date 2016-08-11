using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Manufacturer service
	/// </summary>
	public partial interface IManufacturerService
    {
        /// <summary>
        /// Deletes a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        void DeleteManufacturer(Manufacturer manufacturer);

		/// <summary>
		/// Get manufacturer query
		/// </summary>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Manufacturer query</returns>
		IQueryable<Manufacturer> GetManufacturers(bool showHidden = false, int storeId = 0);

        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturer collection</returns>
        IList<Manufacturer> GetAllManufacturers(bool showHidden = false);

		/// <summary>
		/// Gets all manufacturers
		/// </summary>
		/// <param name="manufacturerName">Manufacturer name</param>
		/// <param name="storeId">Whether to filter result by store identifier</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Manufacturer collection</returns>
		IList<Manufacturer> GetAllManufacturers(string manufacturerName, int storeId = 0, bool showHidden = false);

		/// <summary>
		/// Gets all manufacturers
		/// </summary>
		/// <param name="manufacturerName">Manufacturer name</param>
		/// <param name="pageIndex">Page index</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="storeId">Whether to filter result by store identifier</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Manufacturers</returns>
		IPagedList<Manufacturer> GetAllManufacturers(string manufacturerName,
            int pageIndex, int pageSize, int storeId = 0, bool showHidden = false);

        /// <summary>
        /// Gets a manufacturer
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>Manufacturer</returns>
        Manufacturer GetManufacturerById(int manufacturerId);

        /// <summary>
        /// Inserts a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        void InsertManufacturer(Manufacturer manufacturer);

        /// <summary>
        /// Updates the manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        void UpdateManufacturer(Manufacturer manufacturer);

		/// <summary>
		/// Update HasDiscountsApplied property (used for performance optimization)
		/// </summary>
		/// <param name="manufacturer">Manufacturer</param>
		void UpdateHasDiscountsApplied(Manufacturer manufacturer);

		/// <summary>
		/// Deletes a product manufacturer mapping
		/// </summary>
		/// <param name="productManufacturer">Product manufacturer mapping</param>
		void DeleteProductManufacturer(ProductManufacturer productManufacturer);
        
        /// <summary>
        /// Gets product manufacturer collection
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product manufacturer collection</returns>
        IPagedList<ProductManufacturer> GetProductManufacturersByManufacturerId(int manufacturerId,
            int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Gets a product manufacturer mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product manufacturer mapping collection</returns>
        IList<ProductManufacturer> GetProductManufacturersByProductId(int productId, bool showHidden = false);

		/// <summary>
		/// Get product manufacturers by manufacturer identifiers
		/// </summary>
		/// <param name="manufacturerIds">Manufacturer identifiers</param>
		/// <returns>Product manufacturers</returns>
		Multimap<int, ProductManufacturer> GetProductManufacturersByManufacturerIds(int[] manufacturerIds);

		/// <summary>
		/// Get product manufacturers by product identifiers
		/// </summary>
		/// <param name="productIds">Product identifiers</param>
		/// <returns>Product manufacturers</returns>
		Multimap<int, ProductManufacturer> GetProductManufacturersByProductIds(int[] productIds);

        /// <summary>
        /// Gets a product manufacturer mapping 
        /// </summary>
        /// <param name="productManufacturerId">Product manufacturer mapping identifier</param>
        /// <returns>Product manufacturer mapping</returns>
        ProductManufacturer GetProductManufacturerById(int productManufacturerId);

        /// <summary>
        /// Inserts a product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        void InsertProductManufacturer(ProductManufacturer productManufacturer);

        /// <summary>
        /// Updates the product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        void UpdateProductManufacturer(ProductManufacturer productManufacturer);
    }
}
