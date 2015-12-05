using System;
using SmartStore.Core.Events;
using SmartStore.GoogleMerchantCenter.Domain;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Web.Framework.Events;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.GoogleMerchantCenter
{
	public class Events : 
		IConsumer<TabStripCreated>,
		IConsumer<ModelBoundEvent>/*,
		IConsumer<RowExportingEvent>*/
	{
		private readonly IGoogleFeedService _googleService;

		public Events(IGoogleFeedService googleService)
		{
			this._googleService = googleService;
		}

		public void HandleEvent(TabStripCreated eventMessage)
		{
			if (eventMessage.TabStripName == "product-edit")
			{
				var productId = ((TabbableModel)eventMessage.Model).Id;
				eventMessage.ItemFactory.Add().Text("GMC")
					.Name("tab-gmc")
					.Icon("fa fa-google fa-lg fa-fw")
					.LinkHtmlAttributes(new { data_tab_name = "GMC" })
					.Route("SmartStore.GoogleMerchantCenter", new { action = "ProductEditTab", productId = productId })
					.Ajax();
			}
		}

		//public void HandleEvent(RowExportingEvent eventMessage)
		//{
		//	if (eventMessage.EntityType != ExportEntityType.Product)
		//		return;

		//	var row = eventMessage.Row;
		//	var product = eventMessage.Row.Entity as Product;

		//	if (product == null)
		//		return;

		//	var gmc = _googleService.GetGoogleProductRecord(product.Id);
		//	if (gmc == null)
		//		return;

		//	row["_GMC_AgeGroup"] = gmc.AgeGroup;
		//	row["_GMC_Color"] = gmc.Color;
		//	row["_GMC_Gender"] = gmc.Gender;
		//	row["_GMC_Size"] = gmc.Size;
		//	row["_GMC_Taxonomy"] = gmc.Taxonomy;
		//	row["_GMC_Material"] = gmc.Material;
		//	row["_GMC_Pattern"] = gmc.Pattern;
		//}

		public void HandleEvent(ModelBoundEvent eventMessage)
		{
			if (!eventMessage.BoundModel.CustomProperties.ContainsKey("GMC"))
				return;
			
			var model = eventMessage.BoundModel.CustomProperties["GMC"] as GoogleProductModel;
			if (model == null)
				return;

			var utcNow = DateTime.UtcNow;
			var entity = _googleService.GetGoogleProductRecord(model.ProductId);
			var insert = (entity == null);

			if (entity == null)
			{
				entity = new GoogleProductRecord
				{
					ProductId = model.ProductId,
					CreatedOnUtc = utcNow
				};
			}

			// map objects
			entity.AgeGroup = model.AgeGroup;
			entity.Color = model.Color;
			entity.Gender = model.Gender;
			entity.Size = model.Size;
			entity.Taxonomy = model.Taxonomy;
			entity.Material = model.Material;
			entity.Pattern = model.Pattern;
			entity.Export = model.Exporting;
			entity.UpdatedOnUtc = utcNow;

			entity.IsTouched = entity.IsTouched();

			if (!insert && !entity.IsTouched)
			{
				_googleService.DeleteGoogleProductRecord(entity);
				return;
			}

			if (insert)
			{
				_googleService.InsertGoogleProductRecord(entity);
			}
			else
			{
				_googleService.UpdateGoogleProductRecord(entity);
			}
		}
	}
}