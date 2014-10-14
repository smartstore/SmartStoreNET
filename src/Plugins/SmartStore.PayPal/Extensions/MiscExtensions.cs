using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Web;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.PayPal.Extensions
{
	public static class MiscExtensions
	{

        public static PayPalExpressSettings LoadPayPalExpressSettings(this ISettingService settingService, string systemName, int storeId = 0)
		{
			if (systemName.IsCaseInsensitiveEqual("PayPalExpress"))
                return settingService.LoadSetting<PayPalExpressSettings>(storeId);

            //if (systemName.IsCaseInsensitiveEqual(AccardaKarCore.SystemNamePurchaseOnAccount))
            //    return settingService.LoadSetting<AccardaKarPurchaseOnAccountSettings>(storeId);

			return null;
		}

		/// <remarks>We need to deal with NameValueCollection cause of PaymentControllerBase overrides.</remarks>
        //public static void ToModel(this PaymentInfoModel model, NameValueCollection form, string paymentMethodSystemName)
        //{
        //    if (paymentMethodSystemName.IsCaseInsensitiveEqual(AccardaKarCore.SystemNamePaymentByInstalments))
        //        model.Method = AccardaKarCore.MethodKaufAufRaten;
        //    else if (paymentMethodSystemName.IsCaseInsensitiveEqual(AccardaKarCore.SystemNamePurchaseOnAccount))
        //        model.Method = AccardaKarCore.MethodKaufAufRechnung;
        //    else
        //        model.Method = "";

        //    Debug.Assert(model.Method == AccardaKarCore.MethodKaufAufRaten || model.Method == AccardaKarCore.MethodKaufAufRechnung, "Missing or wrong Accarda-KaR method type!");

        //    if (form["CustomerType"] == null)
        //        model.CustomerType = AccardaKarCustomerType.B2C;
        //    else
        //        model.CustomerType = (AccardaKarCustomerType)form["CustomerType"].ToInt();

        //    if (form["Salutation"] == null)
        //        model.Salutation = AccardaKarSalutationType.Mr;
        //    else
        //        model.Salutation = (AccardaKarSalutationType)form["Salutation"].ToInt();

        //    model.DateOfBirthDay = form["DateOfBirthDay"].ToInt();
        //    model.DateOfBirthMonth = form["DateOfBirthMonth"].ToInt();
        //    model.DateOfBirthYear = form["DateOfBirthYear"].ToInt();

        //    model.Street = form["Street"];
        //    model.HouseNumber = form["HouseNumber"];

        //    model.PhysicalInvoice = form["PhysicalInvoice"].EmptyNull().ToLower().Contains("true");

        //    model.CompanyName = form["CompanyName"];
        //    model.CompanyNumber = form["CompanyNumber"];
        //    model.CompanyUid = form["CompanyUid"];
        //    model.CompanyLegalForm = form["CompanyLegalForm"];
        //    model.CompanyLegalFormOther = form["CompanyLegalFormOther"];

        //    model.CompanyFoundationDateDay = form["CompanyFoundationDateDay"].ToInt();
        //    model.CompanyFoundationDateMonth = form["CompanyFoundationDateMonth"].ToInt();
        //    model.CompanyFoundationDateYear = form["CompanyFoundationDateYear"].ToInt();
        //}
	}
}
