using System;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Providers
{
	/// <summary>
	/// Exports XML formatted category data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreCategoryXml")]
	[FriendlyName("SmartStore XML category export")]
	[IsHidden(true)]
	public class CategoryXmlExportProvider : ExportProviderBase
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreCategoryXml"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.Category; }
		}

		public override string FileExtension
		{
			get { return "XML"; }
		}

		public override void Execute(IExportExecuteContext context)
		{
			using (var helper = new ExportXmlHelper(context.DataStream))
			{
				helper.Writer.WriteStartDocument();
				helper.Writer.WriteStartElement("Categories");
				helper.Writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic category in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						Category entity = category.Entity;

						helper.Writer.WriteStartElement("Category");

						try
						{
							helper.WriteCategory(category, null);

							helper.Writer.WriteStartElement("ProductCategories");
							foreach (dynamic productCategory in category.ProductCategories)
							{
								ProductCategory entityProductCategory = productCategory.Entity;
								helper.Writer.WriteStartElement("ProductCategory");
								helper.Writer.Write("Id", entity.Id.ToString());
								helper.Writer.Write("ProductId", entityProductCategory.ProductId.ToString());
								helper.Writer.Write("DisplayOrder", entityProductCategory.DisplayOrder.ToString());
								helper.Writer.Write("IsFeaturedProduct", entityProductCategory.IsFeaturedProduct.ToString());
								helper.Writer.Write("CategoryId", entityProductCategory.CategoryId.ToString());
								helper.Writer.WriteEndElement();	// ProductCategory
							}
							helper.Writer.WriteEndElement();	// ProductCategories

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, entity.Id);
						}

						helper.Writer.WriteEndElement();	// Category
					}
				}

				helper.Writer.WriteEndElement();	// Categories
				helper.Writer.WriteEndDocument();
			}
		}
	}
}
