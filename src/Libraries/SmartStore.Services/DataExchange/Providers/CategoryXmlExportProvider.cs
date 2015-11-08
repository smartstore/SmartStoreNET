using System;
using System.IO;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
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
			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var helper = new ExportXmlHelper(stream))
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

						helper.Writer.WriteStartElement("Category");

						try
						{
							helper.WriteCategory(category, null);

							helper.Writer.WriteStartElement("ProductCategories");
							foreach (dynamic productCategory in category.ProductCategories)
							{
								helper.Writer.WriteStartElement("ProductCategory");
								helper.Writer.Write("Id", ((int)productCategory.Id).ToString());
								helper.Writer.Write("ProductId", ((int)productCategory.ProductId).ToString());
								helper.Writer.Write("DisplayOrder", ((int)productCategory.DisplayOrder).ToString());
								helper.Writer.Write("IsFeaturedProduct", ((bool)productCategory.IsFeaturedProduct).ToString());
								helper.Writer.Write("CategoryId", ((int)productCategory.CategoryId).ToString());
								helper.Writer.WriteEndElement();	// ProductCategory
							}
							helper.Writer.WriteEndElement();	// ProductCategories

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)category.Id);
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
