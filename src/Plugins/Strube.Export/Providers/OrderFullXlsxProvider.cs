using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using System.IO;
using SmartStore.Core.Domain.Orders;
using OfficeOpenXml;
using Strube.Export.Models;
using SmartStore.Core.Domain.Security;

namespace Strube.Export.Providers
{
    /// <summary>
    /// Provider for Export Order Infos without Prices as XLSX File
    /// </summary>
    [SystemName("Strube.OrdersExportXLSX")]
    [FriendlyName("Strube Full Order xlsx-Export")]
    [DisplayOrder(2)]
    [ExportFeatures(Features =
    ExportFeatures.CreatesInitialPublicDeployment |
    ExportFeatures.CanOmitGroupedProducts |
    ExportFeatures.CanProjectAttributeCombinations |
    ExportFeatures.CanProjectDescription |
    ExportFeatures.UsesSkuAsMpnFallback |
    ExportFeatures.OffersBrandFallback |
    ExportFeatures.UsesSpecialPrice |
    ExportFeatures.UsesAttributeCombination |
    ExportFeatures.CanOmitCompletionMail)]
    public class OrderFullXlsxProvider: ExportProviderBase
    {
        private readonly IEncryptionService _encryptionService;
        private readonly ISettingService _settingService;
        private readonly string _encryptionKey;

        public OrderFullXlsxProvider(IEncryptionService encryptionService, ISettingService settingService)
        {
            _encryptionService = encryptionService;
            _settingService = settingService;
            var securitySettings = _settingService.LoadSetting<SecuritySettings>();
            _encryptionKey = securitySettings.EncryptionKey;
        }

        public override ExportEntityType EntityType
        {
            get { return ExportEntityType.Order; }
        }

        public static string SystemName
        {
            get { return "Strube.OrdersExportXLSX"; }
        }

        public override string FileExtension
        {
            get { return "xlsx"; }
        }

        protected override void Export(ExportExecuteContext context)
        {
            dynamic currency = context.Currency;
            OrderDetails orderDetails = new OrderDetails();
            // convert the lines
            while (context.Abort == DataExchangeAbortion.None && context.DataSegmenter.ReadNextSegment())
            {
                var segment = context.DataSegmenter.CurrentSegment;
                foreach (dynamic order in segment)
                {
                    Order orderEntity = order.Entity;
                    List<OrderItem> orderItem = orderEntity.OrderItems.ToList();

                    if (context.Abort != DataExchangeAbortion.None)
                    {
                        break;
                    }
                    
                    try
                    {
                        foreach (OrderItem item in orderItem)
                        {
                            OrderDetail tmp = new OrderDetail(item,_encryptionService,_encryptionKey);
                            orderDetails.Add(tmp);
                            ++context.RecordsSucceeded;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.RecordException(ex, orderEntity.Id);
                    }
                }
            }
            //create ExcelPackage

            using (ExcelPackage excelPackage = new ExcelPackage(context.DataStream))
            {
                var workSheet = excelPackage.Workbook.Worksheets.Add(
                    DateTime.Now.Year.ToString() +
                    "_" +
                    DateTime.Now.Month.ToString() +
                    "_" +
                    DateTime.Now.Day.ToString() +
                    "_" +
                    DateTime.Now.Hour.ToString() +
                    "_" +
                    DateTime.Now.Minute.ToString());
                workSheet.Cells["A1"].LoadFromCollection<OrderDetail>(orderDetails, true, OfficeOpenXml.Table.TableStyles.Medium13);
                excelPackage.Workbook.Properties.Company = "Strube D&S GmbH";
                excelPackage.Workbook.Properties.Author = "Strube Web Shop";
                excelPackage.Save();
            }

        }

        //public override ExportConfigurationInfo ConfigurationInfo
        //{
        //    get { return null; }
        //}

        public override void OnExecuted(ExportExecuteContext context)
        {

        }

    }
}