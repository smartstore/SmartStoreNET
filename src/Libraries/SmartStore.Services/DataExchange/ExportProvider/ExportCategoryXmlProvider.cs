using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.ExportProvider
{
	/// <summary>
	/// Exports XML formatted category data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreCategoryXml")]
	[FriendlyName("SmartStore XML category export")]
	[IsHidden(true)]
	public class ExportCategoryXmlProvider : IExportProvider
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreCategoryXml"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Category; }
		}

		public string FileExtension
		{
			get { return "XML"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				CheckCharacters = false,
				Indent = true,
				IndentChars = "\t"
			};

			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var xmlHelper = new ExportXmlHelper(writer, CultureInfo.InvariantCulture);

				writer.WriteStartDocument();
				writer.WriteStartElement("Categories");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic category in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						writer.WriteStartElement("Category");

						try
						{
							xmlHelper.WriteCategory(category, null);

							writer.WriteStartElement("ProductCategories");
							foreach (dynamic productCategory in category.ProductCategories)
							{
								writer.WriteStartElement("ProductCategory");
								writer.Write("Id", ((int)productCategory.Id).ToString());
								writer.Write("ProductId", ((int)productCategory.ProductId).ToString());
								writer.Write("DisplayOrder", ((int)productCategory.DisplayOrder).ToString());
								writer.Write("IsFeaturedProduct", ((bool)productCategory.IsFeaturedProduct).ToString());
								writer.Write("CategoryId", ((int)productCategory.CategoryId).ToString());
								writer.WriteEndElement();	// ProductCategory
							}
							writer.WriteEndElement();	// ProductCategories

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)category.Id);
						}

						writer.WriteEndElement();	// Category
					}
				}

				writer.WriteEndElement();	// Categories
				writer.WriteEndDocument();
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
