using System.Collections.Generic;
using SmartStore.GoogleMerchantCenter.Domain;
using SmartStore.GoogleMerchantCenter.Models;
using Telerik.Web.Mvc;

namespace SmartStore.GoogleMerchantCenter.Services
{
	public partial interface IGoogleFeedService
    {
		GoogleProductRecord GetGoogleProductRecord(int productId);

		List<GoogleProductRecord> GetGoogleProductRecords(int[] productIds);

		void InsertGoogleProductRecord(GoogleProductRecord record);

		void UpdateGoogleProductRecord(GoogleProductRecord record);
		
		void DeleteGoogleProductRecord(GoogleProductRecord record);

		void Upsert(int pk, string name, string value);

		GridModel<GoogleProductModel> GetGridModel(GridCommand command, string searchProductName = null, string touched = null);

		List<string> GetTaxonomyList(string searchTerm);
    }
}
