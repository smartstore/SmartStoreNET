﻿using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Topics;
using SmartStore.Data.Setup;

namespace SmartStore.Web.Infrastructure.Installation
{

    public class DeDESeedData : InvariantSeedData
    {

		public DeDESeedData()
        {
        }

        protected override void Alter(Customer entity)
        {
            base.Alter(entity);

			if (entity.SystemName == SystemCustomerNames.SearchEngine)
            {
                entity.AdminComment = "System-Gastkonto für Suchmaschinenanfragen.";
            }
			else if (entity.SystemName == SystemCustomerNames.BackgroundTask)
            {
                entity.AdminComment = "Systemkonto für geplante Aufgaben.";
            }
			else if (entity.SystemName == SystemCustomerNames.PdfConverter)
			{
				entity.AdminComment = "Systemkonto für den PDF-Konverter.";
			}
        }

        protected override void Alter(IList<MeasureDimension> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemKeyword)
                .Alter("mm", x =>
                {
                    x.Name = "Millimeter";
                    x.Ratio = 0.001M;
                })
                .Alter("cm", x =>
                {
                    x.Name = "Zentimeter";
                    x.Ratio = 0.01M;
                })
                .Alter("m", x =>
                {
                    x.Name = "Meter";
                    x.Ratio = 1M;
                })
                .Alter("inch", x =>
                {
                    x.Name = "Zoll";
                    x.Ratio = 0.0254M;
                })
                .Alter("ft", x =>
                {
                    x.Name = "Fuß";
                    x.Ratio = 0.3048M;
                });
        }

        protected override void Alter(IList<MeasureWeight> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemKeyword)
                .Alter("oz", x =>
                {
                    x.Name = "Unze";
                    x.Ratio = 0.02835M;
                    x.DisplayOrder = 10;
                })
                .Alter("lb", x =>
                {
                    x.Name = "Pfund";
                    x.Ratio = 0.4536M;
                    x.DisplayOrder = 10;
                })
                .Alter("kg", x =>
                {
                    x.Name = "Kilogramm";
                    x.Ratio = 1M;
                    x.DisplayOrder = 1;
                })
                .Alter("g", x =>
                {
                    x.Name = "Gramm";
                    x.Ratio = 0.001M;
                    x.DisplayOrder = 2;
                })
                .Alter("l", x =>
                {
                    x.Name = "Liter";
                    x.Ratio = 1M;
                    x.DisplayOrder = 3;
                })
                .Alter("ml", x =>
                {
                    x.Name = "Milliliter";
                    x.Ratio = 0.001M;
                    x.DisplayOrder = 4;
                });
        }

        protected override void Alter(IList<MessageTemplate> entities)
        {
            base.Alter(entities);

            string cssString = @"<style type=""text/css"">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;font-family: 'Segoe UI', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:720px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; } .legal-infos, .legal-infos p { font-size:11px; color: #aaa} .supplier-identification, .supplier-identification td { color: #646464; font-size: 11px } .supplier-identification { width:100%; border-top: 1px solid #ccc; border-bottom: 1px solid #ccc }</style>";
            string templateHeader = cssString + "<center><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" bgcolor=\"#ffffff\" class=\"template-body\"><tbody><tr><td>";
            string templateFooter = "</td></tr></tbody></table></center>";


            entities.WithKey(x => x.Name)

            
                .Alter("Blog.BlogComment", x =>
                {
                    x.Subject = "%Store.Name%. Neuer Blog-Kommentar";
                    x.Body = templateHeader + "<p><a href=\" % Store.URL % \">%Store.Name%</a>&nbsp;</p> <p>Ein neuer Kommentar wurde zu dem Blog-Eintrag&nbsp;\" % BlogComment.BlogPostTitle % \" abgegeben.<br /><br /></p>" + templateFooter;
                })
                .Alter("Customer.BackInStock", x =>
                {
                    x.Subject = "%Store.Name%. Artikel wieder verfügbar";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p></p> <p>Sehr geehrte(r) Frau/Herr&nbsp;%Customer.FullName%,&nbsp;</p> <p></p> <p>der Artikel&nbsp;\"%BackInStockSubscription.ProductName%\" ist wieder verf&uuml;gbar.</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p> <p><br /><br /></p>" + templateFooter;
                })
                .Alter("Customer.EmailValidationMessage", x =>
                {
                    x.Subject = "%Store.Name%. Registrierung bestätigen";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br /></p> <p>Bitte best&auml;tigen Sie Ihre Registrierung mit einem Klick auf diesen <a href=\"%Customer.AccountActivationURL%\">Link</a>.</p> <p></p> <p><br />Ihr Shop-Team</p>" + templateFooter;
                })
                .Alter("Customer.NewPM", x =>
                {
                    x.Subject = "%Store.Name%. Neue persönliche Nachricht";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />Sie haben eine neue pers&ouml;nliche Nachricht erhalten.</p>" + templateFooter;
                })
                .Alter("Customer.PasswordRecovery", x =>
                {
                    x.Subject = "%Store.Name%. Kennwort zurücksetzen";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p>Um Ihr Kennwort zur&uuml;ckzusetzen klicken Sie bitte <a href=\"%Customer.PasswordRecoveryURL%\">hier</a>.</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p> <p><br /><br /></p>" + templateFooter;
                })
                .Alter("Customer.WelcomeMessage", x =>
                {
                    x.Subject = "Willkommen im %Store.Name% Shop";
                    x.Body = templateHeader + "<p>Herzlich Willkommen in unserem Online-Shop <a href=\"%Store.URL%\">%Store.Name%</a>!</p> <p>St&ouml;bern Sie in Warengruppen und Produkte, Lesen Sie im Blog und tauschen Sie Ihre Meinung im Forum aus.</p> <p>Nehmen Sie auch an unseren Umfragen teil!</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p>" + templateFooter;
                })
                .Alter("Forums.NewForumPost", x =>
                {
                    x.Subject = "%Store.Name%. - Benachrichtigung über einen neuen Beitrag";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p></p> <p>Ein neuer Beitrag wurde in&nbsp;<a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a>&nbsp;im Forum&nbsp;<a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a>&nbsp;erstellt.</p> <p>Klicken Sie <a href=\"%Forums.TopicURL%\">hier</a> f&uuml;r weitere Informationen.</p> <p>Autor des Beitrags:&nbsp;%Forums.PostAuthor%<br />Inhalt des Beitrags: %Forums.PostBody%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p>" + templateFooter;
                })
                .Alter("Forums.NewForumTopic", x =>
                {
                    x.Subject = "%Store.Name%. Benachrichtigung über ein neues Thema";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p></p> <p>Ein neuer Beitrag <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a>&nbsp;wurde im Forum &nbsp;<a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a>&nbsp;erstellt.</p> <p>Klicken Sie <a href=\"%Forums.TopicURL%\">hier</a> f&uuml;r weitere Informationen.</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p>" + templateFooter;
                })
                .Alter("GiftCard.Notification", x =>
                {
                    x.Subject = "%GiftCard.SenderName% hat Ihnen einen Geschenkgutschein für %Store.Name% geschickt";
                    x.Body = templateHeader + "<p>Sehr geehrte(r)&nbsp;%GiftCard.RecipientName%,</p> <p></p> <p>Sie haben einen Geschenkgutschein in H&ouml;he von %GiftCard.Amount%&nbsp;f&uuml;r den Online-Shop&nbsp;%Store.Name% erhalten</p> <p>Ihr Gutscheincode lautet&nbsp;%GiftCard.CouponCode%</p> <p>Diese Nachricht wurde mit gesendet:</p> <p>%GiftCard.Message%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p>" + templateFooter;
                })
                .Alter("NewCustomer.Notification", x =>
                {
                    x.Subject = "%Store.Name% - Neu-Kunden Registrierung";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p>Ein neuer Kunde hat sich registriert:<br /><br />Name: %Customer.FullName%<br />E-Mail: %Customer.Email%</p>" + templateFooter;
                })
                .Alter("NewReturnRequest.StoreOwnerNotification", x =>
                {
                    x.Subject = "%Store.Name%. Rückgabe-Anforderung";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p>%Customer.FullName% hat eine R&uuml;ckgabe-Anforderung geschickt.&nbsp;</p> <p>Anforderungs-ID: %ReturnRequest.ID%<br />Artikel: %ReturnRequest.Product.Quantity% x %ReturnRequest.Product.Name%<br />R&uuml;ckgabegrund: %ReturnRequest.Reason%<br />Gew&uuml;nschte Aktion: %ReturnRequest.RequestedAction%<br />Nachricht vom Kunden:<br />%ReturnRequest.CustomerComment%</p>" + templateFooter;
                })
                .Alter("News.NewsComment", x =>
                {
                    x.Subject = "%Store.Name%. Neuer Kommentar zu einer News-Meldung";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p>Zu der News \"%NewsComment.NewsTitle%\" wurde ein neuer Kommentar eingestellt.</p>" + templateFooter;
                })
                .Alter("NewsLetterSubscription.ActivationMessage", x =>
                {
                    x.Subject = "%Store.Name%. Bestätigen Sie Ihr Newsletter-Abonnement";
                    x.Body = templateHeader + "<p></p> <p><a href=\"%NewsLetterSubscription.ActivationUrl%\">Klicken Sie hier, um Ihre Newsletter-Registrierung zu &nbsp;best&auml;tigen</a></p> <p>Sollten Sie diese E-Mail f&auml;lschlich erhalten haben, l&ouml;schen Sie bitte diese E-Mail.</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p></p>" + templateFooter;
                })
                .Alter("NewsLetterSubscription.DeactivationMessage", x =>
                {
                    x.Subject = "%Store.Name%. Bestätigen Sie Ihre Newsletter-Abmeldung";
                    x.Body = templateHeader + "<p><a href=\"%NewsLetterSubscription.DeactivationUrl%\">Klicken Sie hier, um Ihre Newsletter-Registrierung zu stornieren.</a></p> <p>Sollten Sie diese E-Mail f&auml;lschlich erhalten haben, l&ouml;schen Sie bitte diese E-Mail.</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p>" + templateFooter;
                })
                .Alter("NewVATSubmitted.StoreOwnerNotification", x =>
                {
                    x.Subject = "%Store.Name%. Neue Umsatzsteuer-ID wurde übermittelt";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />%Customer.FullName% (%Customer.Email%) hat eine neue Umsatzsteuer-ID &uuml;bermittelt:</p> <p><br />Umsatzsteuer-ID: %Customer.VatNumber%<br />Status: %Customer.VatNumberStatus%<br />&Uuml;bermittelt von: %VatValidationResult.Name% -&nbsp;%VatValidationResult.Address%</p>" + templateFooter;
                })
                .Alter("OrderCancelled.CustomerNotification", x =>
                {
                    x.Subject = "%Store.Name% - Ihr Auftrag wurde storniert";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a></p> <p>Hallo %Order.CustomerFullName%,&nbsp;</p> <p>Ihr Auftrag wurde storniert. Details finden Sie unten.<br /><br />Auftragsnummer: %Order.OrderNumber%<br />Auftrags-Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Auftrags-Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%<br />Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p>" + templateFooter;
                })
                .Alter("OrderCompleted.CustomerNotification", x =>
                {
                    x.Subject = "%Store.Name% - Ihre Bestellung wurde bearbeitet";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />Hallo %Order.CustomerFullName%,&nbsp;</p> <p>Ihre Bestellung wurde bearbeitet.&nbsp;</p> <p></p> <p>Auftrags-Nummer: %Order.OrderNumber%<br />Details zum Auftrag:&nbsp;<a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%<br />Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p>" + templateFooter;
                })
                .Alter("ShipmentDelivered.CustomerNotification", x =>
                {
                    x.Subject = "Ihre Bestellung bei %Store.Name% wurde ausgeliefert";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />Hallo %Order.CustomerFullName%,&nbsp;</p> <p>Ihre Bestellung wurde ausgeliefert.</p> <p>Auftrags-Nummer: %Order.OrderNumber%<br />Auftrags-Details:&nbsp;<a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br />Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%&nbsp;<br />Zahlart: %Order.PaymentMethod%<br /><br />Gelieferte Artikel:&nbsp;<br /><br />%Shipment.Product(s)%</p>" + templateFooter;
                })
                .Alter("OrderPlaced.CustomerNotification", x =>
                {
                    x.Subject = "Auftragsbestätigung - %Store.Name%";
                    x.Body = templateHeader + "<p> <a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hallo %Order.CustomerFullName%, <br /> Vielen Dank für Ihre Bestellung bei <a href=\"%Store.URL%\">%Store.Name%</a>.  Eine Übersicht über Ihre Bestellung finden Sie unten. <br /><br />Order Number: %Order.OrderNumber%<br />  Bestellübersicht: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />  Datum: %Order.CreatedOn%<br /><br /><br /><br />  Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />  %Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />  Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />  %Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />  Versandart: %Order.ShippingMethod%<br /> Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p><p></p><p>%Store.SupplierIdentification%</p><p>%Order.ConditionsOfUse%</p><p>%Order.Disclaimer%</p>" + templateFooter;
                })
                .Alter("OrderPlaced.StoreOwnerNotification", x =>
                {
                    x.Subject = "%Store.Name% - Neue Bestellung #%Order.OrderNumber%";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p></p> <p>Eine neue Bestellung wurde get&auml;tigt:</p> <p><br />Kunde: %Order.CustomerFullName% (%Order.CustomerEmail%) .&nbsp;<br /><br />Auftrags-Nummer: %Order.OrderNumber%<br />Datum: %Order.CreatedOn%<br /><br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod% <br /> Zahlart: %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p>" + templateFooter;
                })
                .Alter("ShipmentSent.CustomerNotification", x =>
                {
                    x.Subject = "Ihre Bestellung bei %Store.Name% wurde verschickt";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />Sehr geehrter Herr/Frau %Order.CustomerFullName%,&nbsp;</p> <p><br />Ihre Bestellung wurde soeben versendet:</p> <p><br />Auftrags-Nummer: %Order.OrderNumber%<br />Auftrags-Details:&nbsp;<a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br />Datum: %Order.CreatedOn%<br /><br /><br />Rechnungsadresse<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Lieferadresse<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Versandart: %Order.ShippingMethod%&nbsp;<br />Zahlart: %Order.PaymentMethod%<br /><br />Versendete Artikel:&nbsp;<br /><br />%Shipment.Product(s)%</p>" + templateFooter;
                })
                .Alter("Product.ProductReview", x =>
                {
                    x.Subject = "%Store.Name%. Neue Produktrezension";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p>Eine neue Produktrezension zu dem Produkt&nbsp;\"%ProductReview.ProductName%\" wurde verfasst.<br /><br /></p>" + templateFooter;
                })
                .Alter("QuantityBelow.StoreOwnerNotification", x =>
                {
                    x.Subject = "%Store.Name% - Mindestlagerbestand unterschritten: %Product.Name%";
					x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p>Der Mindestlagerbestand f&uuml;r folgendes Produkt wurde unterschritte;<br />%Product.Name% (ID: %Product.ID%, SKU: %Product.Sku%) &nbsp;<br /><br />Menge: %Product.StockQuantity%</p>" + templateFooter;
                })
                .Alter("ReturnRequestStatusChanged.CustomerNotification", x =>
                {
                    x.Subject = "%Store.Name%. Rücksendung - Status-Änderung";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />Hallo %Customer.FullName%,</p> <p>der Status Ihrer R&uuml;cksendung&nbsp;#%ReturnRequest.ID% wurde aktualisiert: %ReturnRequest.Status%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr&nbsp;%Store.Name% - Team</p>" + templateFooter;
                })
                .Alter("Service.EmailAFriend", x =>
                {
                    x.Subject = "%Store.Name% - Produktempfehlung von %EmailAFriend.Email%";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />%EmailAFriend.Email% m&ouml;chte Ihnen bei %Store.Name% ein Produkt empfehlen:<br /><br /><b><a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">%Product.Name%</a></b>&nbsp;<br />%Product.ShortDescription%&nbsp;</p> <p></p> <p>Weitere Details finden Sie <a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">hier</a><br /><br /><br />%EmailAFriend.PersonalMessage%</p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p><br />Ihr %Store.Name% - Team</p>" + templateFooter;
                })
                .Alter("Wishlist.EmailAFriend", x =>
                {
                    x.Subject = "%Store.Name% - Wunschliste von %Wishlist.Email%";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;<br /><br />%Wishlist.Email% m&ouml;chte mit Ihnen ihre/seine Wunschliste teilen.<br /><br /></p> <p>Um die Wunschliste einzusehen, klicken Sie bitte <a target=\"_blank\" href=\"%Wishlist.URLForCustomer%\">hier</a>.<br /><br /><br /></p> <p>%Wishlist.PersonalMessage%<br /><br />%Store.Name%</p>" + templateFooter;
                })
                .Alter("Customer.NewOrderNote", x =>
                {
                    x.Subject = "%Store.Name% - Wunschliste von %Wishlist.Email%";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p></p> <p>Hallo&nbsp;%Customer.FullName%,&nbsp;</p> <p></p> <p>Ihrem Auftrag wurde eine Notiz hinterlegt:</p> <p>\"%Order.NewNoteText%\".<br /><a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a></p> <p></p> <p>Mit freundlichen Gr&uuml;&szlig;en,</p> <p></p> <p>Ihr Shop-Team</p>" + templateFooter;
                })
                .Alter("RecurringPaymentCancelled.StoreOwnerNotification", x =>
                {
                    x.Subject = "%Store.Name%. Wiederkehrende Zahlung storniert";
                    x.Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a>&nbsp;</p> <p>Folgende wiederkehrende Zahlung wurde vom Kunden storniert:</p> <p>Zahlungs-ID=%RecurringPayment.ID%<br />Kunden-Name und E-Mail: %Customer.FullName% (%Customer.Email%)&nbsp;</p>" + templateFooter;
                })
                .Alter("Product.AskQuestion", x =>
                {
                    x.Subject = "%Store.Name% - Frage zu '%Product.Name%' von %ProductQuestion.SenderName%";
					x.Body = templateHeader + "<p>%ProductQuestion.Message%</p><p><strong>ID:</strong> %Product.ID%<br /><strong>SKU:</strong> %Product.Sku%<br /><strong>Email:</strong> %ProductQuestion.SenderEmail%<br /><strong>Name: </strong>%ProductQuestion.SenderName%<br /><strong>Telefon: </strong>%ProductQuestion.SenderPhone%</p>" + templateFooter;
                })


                ;
        }


        protected override void Alter(IList<ShippingMethod> entities)
        {
            base.Alter(entities);
            entities.WithKey(x => x.DisplayOrder)
                .Alter(0, x =>
                {
                    x.Name = "Abholung";
                    x.Description = "Holen Sie Ihre Bestellung direkt bei uns ab.";
                })
                .Alter(1, x =>
                {
                    x.Name = "Versand";
                    x.Description = "Ihre Bestellung wird Ihnen durch unsere Versandpartner zugestellt.";
                });   
        }

        protected override void Alter(IList<Currency> entities)
        {
            base.Alter(entities);

            // unpublish all currencies
            entities.Each(x => x.Published = false);

            entities.WithKey(x => x.DisplayLocale)
               .Alter("de-DE", x =>
               {
                   x.Published = true;
                   x.Rate = 1M;
                   x.DisplayOrder = -10;
               })
               .Alter("de-CH", x =>
               {
                   x.Rate = 1.20M;
                   x.DisplayOrder = -5;
               })
               .Alter("en-US", x => x.Rate = 1.29M)
               .Alter("en-AU", x => x.Rate = 1.24M)
               .Alter("en-CA", x => x.Rate = 1.28M)
               .Alter("tr-TR", x => x.Rate = 2.31M)
               .Alter("zh-CN", x => x.Rate = 8.02M)
               .Alter("zh-HK", x => x.Rate = 9.98M)
               .Alter("ja-JP", x => x.Rate = 106.21M)
               .Alter("ru-RU", x => x.Rate = 40.16M)
               .Alter("sv-SE", x => x.Rate = 8.60M);
        }

        protected override void Alter(IList<CustomerRole> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
               .Alter("Administrators", x =>
               {
                   x.Name = "Administratoren";
               })
               .Alter("Forum Moderators", x =>
               {
                   x.Name = "Foren Moderatoren";
               })
               .Alter("Registered", x =>
               {
                   x.Name = "Registriert";
               })
               .Alter("Guests", x =>
               {
                   x.Name = "Gäste";
               })
               ;
        }

        protected override void Alter(Address entity)
        {
            base.Alter(entity);
            var cCountry = base.DbContext.Set<Country>().Where(x => x.ThreeLetterIsoCode == "DEU");
            
            entity.FirstName = "Max";
            entity.LastName ="Mustermann";
            entity.Email ="admin@meineshopurl.de";
            entity.Company ="Max Mustermann";
            entity.Address1 = "Musterweg 1";
            entity.City   = "Musterstadt";
            entity.StateProvince = cCountry.FirstOrDefault().StateProvinces.FirstOrDefault();
            entity.Country = cCountry.FirstOrDefault();
            entity.ZipPostalCode = "12345";
        }


        protected override string TaxNameBooks
        {
            get { return "Ermäßigt"; }
        }
        protected override string TaxNameDigitalGoods
        {
            get { return "Normal"; }
        }
        protected override string TaxNameJewelry
        {
            get { return "Normal"; }
        }
        protected override string TaxNameApparel
        {
            get { return "Normal"; }
        }
        protected override string TaxNameFood
        {
            get { return "Ermäßigt"; }
        }
        protected override string TaxNameElectronics
        {
            get { return "Normal"; }
        }
        protected override string TaxNameTaxFree
        {
            get { return "Befreit"; }
        }
        public override decimal[] FixedTaxRates
        {
            get { return new decimal[] { 19, 7, 0 }; }
        }

        protected override void Alter(IList<TaxCategory> entities)
        {
            base.Alter(entities);

            // clear all tax categories
            entities.Clear();

            // add de-DE specific ones
            entities.Add(new TaxCategory
            {
                Name = "Normal",
                DisplayOrder = 0,
            });
            entities.Add(new TaxCategory
            {
                Name = "Ermäßigt",
                DisplayOrder = 1,
            });
            entities.Add(new TaxCategory
            {
                Name = TaxNameTaxFree,
                DisplayOrder = 2,
            });
        }

        protected override void Alter(IList<Country> entities)
        {
            base.Alter(entities);

            entities.Each(x => x.Published = false);

            entities.WithKey(x => x.NumericIsoCode)

            .Alter(276, x =>
            {
                x.Name = "Deutschland";
                x.DisplayOrder = -10;
                x.Published = true;
                #region Provinces
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Baden-Württemberg",
                    Abbreviation = "BW",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Bayern",
                    Abbreviation = "BY",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Berlin",
                    Abbreviation = "BE",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Brandenburg",
                    Abbreviation = "BB",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Bremen",
                    Abbreviation = "HB",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Hamburg",
                    Abbreviation = "HH",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Hessen",
                    Abbreviation = "HE",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Mecklenburg-Vorpommern",
                    Abbreviation = "MV",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Niedersachsen",
                    Abbreviation = "NI",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Nordrhein-Westfalen",
                    Abbreviation = "NW",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Rheinland-Pfalz",
                    Abbreviation = "RP",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Saarland",
                    Abbreviation = "SL",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Sachsen",
                    Abbreviation = "SN",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Sachsen-Anhalt",
                    Abbreviation = "ST",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Schleswig-Holstein",
                    Abbreviation = "SH",
                    Published = true,
                    DisplayOrder = 1,
                });
                x.StateProvinces.Add(new StateProvince()
                {
                    Name = "Thüringen",
                    Abbreviation = "TH",
                    Published = true,
                    DisplayOrder = 1,
                });
                #endregion Provinces
            })
            .Alter(40, x =>
                {
                    x.Name = "Österreich";
                    x.DisplayOrder = -5;
                    #region Provinces
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Burgenland",
                        Abbreviation = "Bgld.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Kärnten",
                        Abbreviation = "Ktn.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Niderösterreich",
                        Abbreviation = "NÖ",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Oberösterreich",
                        Abbreviation = "OÖ",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Salzburg",
                        Abbreviation = "Sbg.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Steiermark",
                        Abbreviation = "Stmk.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Tirol",
                        Abbreviation = "T",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Vorarlberg",
                        Abbreviation = "Vbg.",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Wien",
                        Abbreviation = "W",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    #endregion Provinces
                })
            .Alter(756, x =>
                {
                    x.Name = "Schweiz";
                    x.DisplayOrder = -1;
                    #region Provinces
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Zürich",
                        Abbreviation = "ZH",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Bern",
                        Abbreviation = "BE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Luzern",
                        Abbreviation = "LU",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Uri",
                        Abbreviation = "UR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Schwyz",
                        Abbreviation = "SZ",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Obwalden",
                        Abbreviation = "OW",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Nidwalden",
                        Abbreviation = "ST",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Glarus",
                        Abbreviation = "GL",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Zug",
                        Abbreviation = "ZG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Freiburg",
                        Abbreviation = "FR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Solothurn",
                        Abbreviation = "SO",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Basel-Stadt",
                        Abbreviation = "BS",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Basel-Landschaft",
                        Abbreviation = "BL",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Schaffhausen",
                        Abbreviation = "SH",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Appenzell Ausserrhoden",
                        Abbreviation = "AR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Appenzell Innerrhoden",
                        Abbreviation = "AI",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "St. Gallen",
                        Abbreviation = "SG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Graubünden",
                        Abbreviation = "GR",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Aargau",
                        Abbreviation = "AG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Thurgau",
                        Abbreviation = "TG",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Tessin",
                        Abbreviation = "Ti",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Waadt",
                        Abbreviation = "VD",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Wallis",
                        Abbreviation = "VS",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Neuenburg",
                        Abbreviation = "NE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Genf",
                        Abbreviation = "GE",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    x.StateProvinces.Add(new StateProvince()
                    {
                        Name = "Jura",
                        Abbreviation = "JU",
                        Published = true,
                        DisplayOrder = 1,
                    });
                    #endregion Provinces
                })
            .Alter(840, x =>
                {
                    x.Name = "Vereinigte Staaten von Amerika";
                    x.DisplayOrder = 100;
                })
            .Alter(124, x =>
                {
                    x.Name = "Kanada";
                    x.DisplayOrder = 100;
                })
            .Alter(32, x =>
                {
                    x.Name = "Argentinien";
                    x.DisplayOrder = 100;
                })
            .Alter(51, x =>
                {
                    x.Name = "Armenien";
                    x.DisplayOrder = 100;
                })
            .Alter(533, x =>
                {
                    x.Name = "Aruba";
                    x.DisplayOrder = 100;
                })
            .Alter(36, x =>
                {
                    x.Name = "Australien";
                    x.DisplayOrder = 100;
                })
            .Alter(31, x =>
                {
                    x.Name = "Aserbaidschan";
                    x.DisplayOrder = 100;
                })
            .Alter(44, x =>
                {
                    x.Name = "Bahamas";
                    x.DisplayOrder = 100;
                })
            .Alter(50, x =>
                {
                    x.Name = "Bangladesh";
                    x.DisplayOrder = 100;
                })
            .Alter(112, x =>
                {
                    x.Name = "Weissrussland";
                    x.DisplayOrder = 100;
                })
            .Alter(56, x =>
                {
                    x.Name = "Belgien";
                    x.DisplayOrder = 100;
                })
            .Alter(84, x =>
                {
                    x.Name = "Belize";
                    x.DisplayOrder = 100;
                })
            .Alter(60, x =>
                {
                    x.Name = "Bermudas";
                    x.DisplayOrder = 100;
                })
            .Alter(68, x =>
                {
                    x.Name = "Bolivien";
                    x.DisplayOrder = 100;
                })
            .Alter(70, x =>
                {
                    x.Name = "Bosnien-Herzegowina";
                    x.DisplayOrder = 100;
                })
            .Alter(76, x =>
                {
                    x.Name = "Brasilien";
                    x.DisplayOrder = 100;
                })
            .Alter(100, x =>
                {
                    x.Name = "Bulgarien";
                    x.DisplayOrder = 100;
                })
            .Alter(136, x =>
                {
                    x.Name = "Kaiman Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(152, x =>
                {
                    x.Name = "Chile";
                    x.DisplayOrder = 100;
                })
            .Alter(156, x =>
                {
                    x.Name = "China";
                    x.DisplayOrder = 100;
                })
            .Alter(170, x =>
                {
                    x.Name = "Kolumbien";
                    x.DisplayOrder = 100;
                })
            .Alter(188, x =>
                {
                    x.Name = "Costa Rica";
                    x.DisplayOrder = 100;
                })
            .Alter(191, x =>
                {
                    x.Name = "Kroatien";
                    x.DisplayOrder = 100;
                })
            .Alter(192, x =>
                {
                    x.Name = "Kuba";
                    x.DisplayOrder = 100;
                })
            .Alter(196, x =>
                {
                    x.Name = "Zypern";
                    x.DisplayOrder = 100;
                })
            .Alter(203, x =>
                {
                    x.Name = "Tschechische Republik";
                    x.DisplayOrder = 100;
                })
            .Alter(208, x =>
                {
                    x.Name = "Dänemark";
                    x.DisplayOrder = 100;
                })
            .Alter(214, x =>
                {
                    x.Name = "Dominikanische Republik";
                    x.DisplayOrder = 100;
                })
            .Alter(218, x =>
                {
                    x.Name = "Ecuador";
                    x.DisplayOrder = 100;
                })
            .Alter(818, x =>
                {
                    x.Name = "Ägypten";
                    x.DisplayOrder = 100;
                })
            .Alter(246, x =>
                {
                    x.Name = "Finnland";
                    x.DisplayOrder = 100;
                })
            .Alter(250, x =>
                {
                    x.Name = "Frankreich";
                    x.DisplayOrder = 100;
                })
            .Alter(268, x =>
                {
                    x.Name = "Georgien";
                    x.DisplayOrder = 100;
                })
            .Alter(292, x =>
                {
                    x.Name = "Gibraltar";
                    x.DisplayOrder = 100;
                })
            .Alter(300, x =>
                {
                    x.Name = "Griechenland";
                    x.DisplayOrder = 100;
                })
            .Alter(320, x =>
                {
                    x.Name = "Guatemala";
                    x.DisplayOrder = 100;
                })
            .Alter(344, x =>
                {
                    x.Name = "Hong Kong";
                    x.DisplayOrder = 100;
                })
            .Alter(348, x =>
                {
                    x.Name = "Ungarn";
                    x.DisplayOrder = 100;
                })
            .Alter(356, x =>
                {
                    x.Name = "Indien";
                    x.DisplayOrder = 100;
                })
            .Alter(360, x =>
                {
                    x.Name = "Indonesien";
                    x.DisplayOrder = 100;
                })
            .Alter(372, x =>
                {
                    x.Name = "Irland";
                    x.DisplayOrder = 100;
                })
            .Alter(376, x =>
                {
                    x.Name = "Israel";
                    x.DisplayOrder = 100;
                })
            .Alter(380, x =>
                {
                    x.Name = "Italien";
                    x.DisplayOrder = 100;
                })
            .Alter(388, x =>
                {
                    x.Name = "Jamaika";
                    x.DisplayOrder = 100;
                })
            .Alter(392, x =>
                {
                    x.Name = "Japan";
                    x.DisplayOrder = 100;
                })
            .Alter(400, x =>
                {
                    x.Name = "Jordanien";
                    x.DisplayOrder = 100;
                })
            .Alter(398, x =>
                {
                    x.Name = "Kasachstan";
                    x.DisplayOrder = 100;
                })
            .Alter(408, x =>
                {
                    x.Name = "Nord Korea";
                    x.DisplayOrder = 100;
                })
            .Alter(414, x =>
                {
                    x.Name = "Kuwait";
                    x.DisplayOrder = 100;
                })
            .Alter(458, x =>
                {
                    x.Name = "Malaysia";
                    x.DisplayOrder = 100;
                })
            .Alter(484, x =>
                {
                    x.Name = "Mexiko";
                    x.DisplayOrder = 100;
                })
            .Alter(528, x =>
                {
                    x.Name = "Niederlande";
                    x.DisplayOrder = 100;
                })
            .Alter(554, x =>
                {
                    x.Name = "Neuseeland";
                    x.DisplayOrder = 100;
                })
            .Alter(578, x =>
                {
                    x.Name = "Norwegen";
                    x.DisplayOrder = 100;
                })
            .Alter(586, x =>
                {
                    x.Name = "Pakistan";
                    x.DisplayOrder = 100;
                })
            .Alter(600, x =>
                {
                    x.Name = "Paraguay";
                    x.DisplayOrder = 100;
                })
            .Alter(604, x =>
                {
                    x.Name = "Peru";
                    x.DisplayOrder = 100;
                })
            .Alter(608, x =>
                {
                    x.Name = "Philippinen";
                    x.DisplayOrder = 100;
                })
            .Alter(616, x =>
                {
                    x.Name = "Polen";
                    x.DisplayOrder = 100;
                })
            .Alter(620, x =>
                {
                    x.Name = "Portugal";
                    x.DisplayOrder = 100;
                })
            .Alter(630, x =>
                {
                    x.Name = "Puerto Rico";
                    x.DisplayOrder = 100;
                })
            .Alter(634, x =>
                {
                    x.Name = "Qatar";
                    x.DisplayOrder = 100;
                })
            .Alter(642, x =>
                {
                    x.Name = "Rumänien";
                    x.DisplayOrder = 100;
                })
            .Alter(643, x =>
                {
                    x.Name = "Rußland";
                    x.DisplayOrder = 100;
                })
            .Alter(682, x =>
                {
                    x.Name = "Saudi Arabien";
                    x.DisplayOrder = 100;
                })
            .Alter(702, x =>
                {
                    x.Name = "Singapur";
                    x.DisplayOrder = 100;
                })
            .Alter(703, x =>
                {
                    x.Name = "Slowakei";
                    x.DisplayOrder = 100;
                })
            .Alter(705, x =>
                {
                    x.Name = "Slowenien";
                    x.DisplayOrder = 100;
                })
            .Alter(710, x =>
                {
                    x.Name = "Südafrika";
                    x.DisplayOrder = 100;
                })
            .Alter(724, x =>
                {
                    x.Name = "Spanien";
                    x.DisplayOrder = 100;
                })
            .Alter(752, x =>
                {
                    x.Name = "Schweden";
                    x.DisplayOrder = 100;
                })
            .Alter(158, x =>
                {
                    x.Name = "Taiwan";
                    x.DisplayOrder = 100;
                })
            .Alter(764, x =>
                {
                    x.Name = "Thailand";
                    x.DisplayOrder = 100;
                })
            .Alter(792, x =>
                {
                    x.Name = "Türkei";
                    x.DisplayOrder = 100;
                })
            .Alter(804, x =>
                {
                    x.Name = "Ukraine";
                    x.DisplayOrder = 100;
                })
            .Alter(784, x =>
                {
                    x.Name = "Vereinigte Arabische Emirate";
                    x.DisplayOrder = 100;
                })
            .Alter(826, x =>
                {
                    x.Name = "Großbritannien";
                    x.DisplayOrder = 100;
                })
            .Alter(581, x =>
                {
                    x.Name = "United States Minor Outlying Islands";
                    x.DisplayOrder = 100;
                })
            .Alter(858, x =>
                {
                    x.Name = "Uruguay";
                    x.DisplayOrder = 100;
                })
            .Alter(860, x =>
                {
                    x.Name = "Usbekistan";
                    x.DisplayOrder = 100;
                })
            .Alter(862, x =>
                {
                    x.Name = "Venezuela";
                    x.DisplayOrder = 100;
                })
            .Alter(688, x =>
                {
                    x.Name = "Serbien";
                    x.DisplayOrder = 100;
                })
            .Alter(4, x =>
                {
                    x.Name = "Afghanistan";
                    x.DisplayOrder = 100;
                })
            .Alter(8, x =>
                {
                    x.Name = "Albanien";
                    x.DisplayOrder = 100;
                })
            .Alter(12, x =>
                {
                    x.Name = "Algerien";
                    x.DisplayOrder = 100;
                })
            .Alter(16, x =>
                {
                    x.Name = "Samoa";
                    x.DisplayOrder = 100;
                })
            .Alter(20, x =>
                {
                    x.Name = "Andorra";
                    x.DisplayOrder = 100;
                })
            .Alter(24, x =>
                {
                    x.Name = "Angola";
                    x.DisplayOrder = 100;
                })
            .Alter(660, x =>
                {
                    x.Name = "Anguilla";
                    x.DisplayOrder = 100;
                })
            .Alter(10, x =>
                {
                    x.Name = "Antarktis";
                    x.DisplayOrder = 100;
                })
            .Alter(28, x =>
                {
                    x.Name = "Antigua und Barbuda";
                    x.DisplayOrder = 100;
                })
            .Alter(48, x =>
                {
                    x.Name = "Bahrain";
                    x.DisplayOrder = 100;
                })
            .Alter(52, x =>
                {
                    x.Name = "Barbados";
                    x.DisplayOrder = 100;
                })
            .Alter(204, x =>
                {
                    x.Name = "Benin";
                    x.DisplayOrder = 100;
                })
            .Alter(64, x =>
                {
                    x.Name = "Bhutan";
                    x.DisplayOrder = 100;
                })
            .Alter(72, x =>
                {
                    x.Name = "Botswana";
                    x.DisplayOrder = 100;
                })
            .Alter(74, x =>
                {
                    x.Name = "Bouvet Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(86, x =>
                {
                    x.Name = "Britisch-Indischer Ozean";
                    x.DisplayOrder = 100;
                })
            .Alter(96, x =>
                {
                    x.Name = "Brunei";
                    x.DisplayOrder = 100;
                })
            .Alter(854, x =>
                {
                    x.Name = "Burkina Faso";
                    x.DisplayOrder = 100;
                })
            .Alter(108, x =>
                {
                    x.Name = "Burundi";
                    x.DisplayOrder = 100;
                })
            .Alter(116, x =>
                {
                    x.Name = "Kambodscha";
                    x.DisplayOrder = 100;
                })
            .Alter(120, x =>
                {
                    x.Name = "Kamerun";
                    x.DisplayOrder = 100;
                })
            .Alter(132, x =>
                {
                    x.Name = "Kap Verde";
                    x.DisplayOrder = 100;
                })
            .Alter(140, x =>
                {
                    x.Name = "Zentralafrikanische Republik";
                    x.DisplayOrder = 100;
                })
            .Alter(148, x =>
                {
                    x.Name = "Tschad";
                    x.DisplayOrder = 100;
                })
            .Alter(162, x =>
                {
                    x.Name = "Christmas Island";
                    x.DisplayOrder = 100;
                })
            .Alter(166, x =>
                {
                    x.Name = "Kokosinseln";
                    x.DisplayOrder = 100;
                })
            .Alter(174, x =>
                {
                    x.Name = "Komoren";
                    x.DisplayOrder = 100;
                })
            .Alter(178, x =>
                {
                    x.Name = "Kongo";
                    x.DisplayOrder = 100;
                })
            .Alter(184, x =>
                {
                    x.Name = "Cook Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(384, x =>
                {
                    x.Name = "Elfenbeinküste";
                    x.DisplayOrder = 100;
                })
            .Alter(262, x =>
                {
                    x.Name = "Djibuti";
                    x.DisplayOrder = 100;
                })
            .Alter(212, x =>
                {
                    x.Name = "Dominika";
                    x.DisplayOrder = 100;
                })
            .Alter(222, x =>
                {
                    x.Name = "El Salvador";
                    x.DisplayOrder = 100;
                })
            .Alter(226, x =>
                {
                    x.Name = "Äquatorial Guinea";
                    x.DisplayOrder = 100;
                })
            .Alter(232, x =>
                {
                    x.Name = "Eritrea";
                    x.DisplayOrder = 100;
                })
            .Alter(233, x =>
                {
                    x.Name = "Estland";
                    x.DisplayOrder = 100;
                })
            .Alter(231, x =>
                {
                    x.Name = "Äthiopien";
                    x.DisplayOrder = 100;
                })
            .Alter(238, x =>
                {
                    x.Name = "Falkland Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(234, x =>
                {
                    x.Name = "Färöer Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(242, x =>
                {
                    x.Name = "Fidschi";
                    x.DisplayOrder = 100;
                })
            .Alter(254, x =>
                {
                    x.Name = "Französisch Guyana";
                    x.DisplayOrder = 100;
                })
            .Alter(258, x =>
                {
                    x.Name = "Französisch Polynesien";
                    x.DisplayOrder = 100;
                })
            .Alter(260, x =>
                {
                    x.Name = "Französisches Süd-Territorium";
                    x.DisplayOrder = 100;
                })
            .Alter(266, x =>
                {
                    x.Name = "Gabun";
                    x.DisplayOrder = 100;
                })
            .Alter(270, x =>
                {
                    x.Name = "Gambia";
                    x.DisplayOrder = 100;
                })
            .Alter(288, x =>
                {
                    x.Name = "Ghana";
                    x.DisplayOrder = 100;
                })
            .Alter(304, x =>
                {
                    x.Name = "Grönland";
                    x.DisplayOrder = 100;
                })
            .Alter(308, x =>
                {
                    x.Name = "Grenada";
                    x.DisplayOrder = 100;
                })
            .Alter(312, x =>
                {
                    x.Name = "Guadeloupe";
                    x.DisplayOrder = 100;
                })
            .Alter(316, x =>
                {
                    x.Name = "Guam";
                    x.DisplayOrder = 100;
                })
            .Alter(324, x =>
                {
                    x.Name = "Guinea";
                    x.DisplayOrder = 100;
                })
            .Alter(624, x =>
                {
                    x.Name = "Guinea Bissau";
                    x.DisplayOrder = 100;
                })
            .Alter(328, x =>
                {
                    x.Name = "Guyana";
                    x.DisplayOrder = 100;
                })
            .Alter(332, x =>
                {
                    x.Name = "Haiti";
                    x.DisplayOrder = 100;
                })
            .Alter(334, x =>
                {
                    x.Name = "Heard und McDonald Islands";
                    x.DisplayOrder = 100;
                })
            .Alter(340, x =>
                {
                    x.Name = "Honduras";
                    x.DisplayOrder = 100;
                })
            .Alter(352, x =>
                {
                    x.Name = "Island";
                    x.DisplayOrder = 100;
                })
            .Alter(364, x =>
                {
                    x.Name = "Iran";
                    x.DisplayOrder = 100;
                })
            .Alter(368, x =>
                {
                    x.Name = "Irak";
                    x.DisplayOrder = 100;
                })
            .Alter(404, x =>
                {
                    x.Name = "Kenia";
                    x.DisplayOrder = 100;
                })
            .Alter(296, x =>
                {
                    x.Name = "Kiribati";
                    x.DisplayOrder = 100;
                })
            .Alter(410, x =>
                {
                    x.Name = "Süd Korea";
                    x.DisplayOrder = 100;
                })
            .Alter(417, x =>
                {
                    x.Name = "Kirgisistan";
                    x.DisplayOrder = 100;
                })
            .Alter(418, x =>
                {
                    x.Name = "Laos";
                    x.DisplayOrder = 100;
                })
            .Alter(428, x =>
                {
                    x.Name = "Lettland";
                    x.DisplayOrder = 100;
                })
            .Alter(422, x =>
                {
                    x.Name = "Libanon";
                    x.DisplayOrder = 100;
                })
            .Alter(426, x =>
                {
                    x.Name = "Lesotho";
                    x.DisplayOrder = 100;
                })
            .Alter(430, x =>
                {
                    x.Name = "Liberia";
                    x.DisplayOrder = 100;
                })
            .Alter(434, x =>
                {
                    x.Name = "Libyen";
                    x.DisplayOrder = 100;
                })
            .Alter(438, x =>
                {
                    x.Name = "Liechtenstein";
                    x.DisplayOrder = 100;
                })
            .Alter(440, x =>
                {
                    x.Name = "Litauen";
                    x.DisplayOrder = 100;
                })
            .Alter(442, x =>
                {
                    x.Name = "Luxemburg";
                    x.DisplayOrder = 100;
                })
            .Alter(446, x =>
                {
                    x.Name = "Macao";
                    x.DisplayOrder = 100;
                })
            .Alter(807, x =>
                {
                    x.Name = "Mazedonien";
                    x.DisplayOrder = 100;
                })
            .Alter(450, x =>
                {
                    x.Name = "Madagaskar";
                    x.DisplayOrder = 100;
                })
            .Alter(454, x =>
                {
                    x.Name = "Malawi";
                    x.DisplayOrder = 100;
                })
            .Alter(462, x =>
                {
                    x.Name = "Malediven";
                    x.DisplayOrder = 100;
                })
            .Alter(466, x =>
                {
                    x.Name = "Mali";
                    x.DisplayOrder = 100;
                })
            .Alter(470, x =>
                {
                    x.Name = "Malta";
                    x.DisplayOrder = 100;
                })
            .Alter(584, x =>
                {
                    x.Name = "Marshall Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(474, x =>
                {
                    x.Name = "Martinique";
                    x.DisplayOrder = 100;
                })
            .Alter(478, x =>
                {
                    x.Name = "Mauretanien";
                    x.DisplayOrder = 100;
                })
            .Alter(480, x =>
                {
                    x.Name = "Mauritius";
                    x.DisplayOrder = 100;
                })
            .Alter(175, x =>
                {
                    x.Name = "Mayotte";
                    x.DisplayOrder = 100;
                })
            .Alter(583, x =>
                {
                    x.Name = "Mikronesien";
                    x.DisplayOrder = 100;
                })
            .Alter(498, x =>
                {
                    x.Name = "Moldavien";
                    x.DisplayOrder = 100;
                })
            .Alter(492, x =>
                {
                    x.Name = "Monaco";
                    x.DisplayOrder = 100;
                })
            .Alter(496, x =>
                {
                    x.Name = "Mongolei";
                    x.DisplayOrder = 100;
                })
            .Alter(500, x =>
                {
                    x.Name = "Montserrat";
                    x.DisplayOrder = 100;
                })
            .Alter(504, x =>
                {
                    x.Name = "Marokko";
                    x.DisplayOrder = 100;
                })
            .Alter(508, x =>
                {
                    x.Name = "Mocambique";
                    x.DisplayOrder = 100;
                })
            .Alter(104, x =>
                {
                    x.Name = "Birma";
                    x.DisplayOrder = 100;
                })
            .Alter(516, x =>
                {
                    x.Name = "Namibia";
                    x.DisplayOrder = 100;
                })
            .Alter(520, x =>
                {
                    x.Name = "Nauru";
                    x.DisplayOrder = 100;
                })
            .Alter(524, x =>
                {
                    x.Name = "Nepal";
                    x.DisplayOrder = 100;
                })
            .Alter(530, x =>
                {
                    x.Name = "Niederländische Antillen";
                    x.DisplayOrder = 100;
                })
            .Alter(540, x =>
                {
                    x.Name = "Neukaledonien";
                    x.DisplayOrder = 100;
                })
            .Alter(558, x =>
                {
                    x.Name = "Nicaragua";
                    x.DisplayOrder = 100;
                })
            .Alter(562, x =>
                {
                    x.Name = "Niger";
                    x.DisplayOrder = 100;
                })
            .Alter(566, x =>
                {
                    x.Name = "Nigeria";
                    x.DisplayOrder = 100;
                })
            .Alter(570, x =>
                {
                    x.Name = "Niue";
                    x.DisplayOrder = 100;
                })
            .Alter(574, x =>
                {
                    x.Name = "Norfolk Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(580, x =>
                {
                    x.Name = "Marianen";
                    x.DisplayOrder = 100;
                })
            .Alter(512, x =>
                {
                    x.Name = "Oman";
                    x.DisplayOrder = 100;
                })
            .Alter(585, x =>
                {
                    x.Name = "Palau";
                    x.DisplayOrder = 100;
                })
            .Alter(591, x =>
                {
                    x.Name = "Panama";
                    x.DisplayOrder = 100;
                })
            .Alter(598, x =>
                {
                    x.Name = "Papua Neuguinea";
                    x.DisplayOrder = 100;
                })
            .Alter(612, x =>
                {
                    x.Name = "Pitcairn";
                    x.DisplayOrder = 100;
                })
            .Alter(638, x =>
                {
                    x.Name = "Reunion";
                    x.DisplayOrder = 100;
                })
            .Alter(646, x =>
                {
                    x.Name = "Ruanda";
                    x.DisplayOrder = 100;
                })
            .Alter(659, x =>
                {
                    x.Name = "St. Kitts Nevis Anguilla";
                    x.DisplayOrder = 100;
                })
            .Alter(662, x =>
                {
                    x.Name = "Saint Lucia";
                    x.DisplayOrder = 100;
                })
            .Alter(670, x =>
                {
                    x.Name = "St. Vincent";
                    x.DisplayOrder = 100;
                })
            .Alter(882, x =>
                {
                    x.Name = "Samoa";
                    x.DisplayOrder = 100;
                })
            .Alter(674, x =>
                {
                    x.Name = "San Marino";
                    x.DisplayOrder = 100;
                })
            .Alter(678, x =>
                {
                    x.Name = "Sao Tome";
                    x.DisplayOrder = 100;
                })
            .Alter(686, x =>
                {
                    x.Name = "Senegal";
                    x.DisplayOrder = 100;
                })
            .Alter(690, x =>
                {
                    x.Name = "Seychellen";
                    x.DisplayOrder = 100;
                })
            .Alter(694, x =>
                {
                    x.Name = "Sierra Leone";
                    x.DisplayOrder = 100;
                })
            .Alter(90, x =>
                {
                    x.Name = "Solomon Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(706, x =>
                {
                    x.Name = "Somalia";
                    x.DisplayOrder = 100;
                })
            .Alter(239, x =>
                {
                    x.Name = "South Georgia, South Sandwich Isl.";
                    x.DisplayOrder = 100;
                })
            .Alter(144, x =>
                {
                    x.Name = "Sri Lanka";
                    x.DisplayOrder = 100;
                })
            .Alter(654, x =>
                {
                    x.Name = "St. Helena";
                    x.DisplayOrder = 100;
                })
            .Alter(666, x =>
                {
                    x.Name = "St. Pierre und Miquelon";
                    x.DisplayOrder = 100;
                })
            .Alter(736, x =>
                {
                    x.Name = "Sudan";
                    x.DisplayOrder = 100;
                })
            .Alter(740, x =>
                {
                    x.Name = "Surinam";
                    x.DisplayOrder = 100;
                })
            .Alter(744, x =>
                {
                    x.Name = "Svalbard und Jan Mayen Islands";
                    x.DisplayOrder = 100;
                })
            .Alter(748, x =>
                {
                    x.Name = "Swasiland";
                    x.DisplayOrder = 100;
                })
            .Alter(760, x =>
                {
                    x.Name = "Syrien";
                    x.DisplayOrder = 100;
                })
            .Alter(762, x =>
                {
                    x.Name = "Tadschikistan";
                    x.DisplayOrder = 100;
                })
            .Alter(834, x =>
                {
                    x.Name = "Tansania";
                    x.DisplayOrder = 100;
                })
            .Alter(768, x =>
                {
                    x.Name = "Togo";
                    x.DisplayOrder = 100;
                })
            .Alter(772, x =>
                {
                    x.Name = "Tokelau";
                    x.DisplayOrder = 100;
                })
            .Alter(776, x =>
                {
                    x.Name = "Tonga";
                    x.DisplayOrder = 100;
                })
            .Alter(780, x =>
                {
                    x.Name = "Trinidad Tobago";
                    x.DisplayOrder = 100;
                })
            .Alter(788, x =>
                {
                    x.Name = "Tunesien";
                    x.DisplayOrder = 100;
                })
            .Alter(795, x =>
                {
                    x.Name = "Turkmenistan";
                    x.DisplayOrder = 100;
                })
            .Alter(796, x =>
                {
                    x.Name = "Turks und Kaikos Inseln";
                    x.DisplayOrder = 100;
                })
            .Alter(798, x =>
                {
                    x.Name = "Tuvalu";
                    x.DisplayOrder = 100;
                })
            .Alter(800, x =>
                {
                    x.Name = "Uganda";
                    x.DisplayOrder = 100;
                })
            .Alter(548, x =>
                {
                    x.Name = "Vanuatu";
                    x.DisplayOrder = 100;
                })
            .Alter(336, x =>
                {
                    x.Name = "Vatikan";
                    x.DisplayOrder = 100;
                })
            .Alter(704, x =>
                {
                    x.Name = "Vietnam";
                    x.DisplayOrder = 100;
                })
            .Alter(92, x =>
                {
                    x.Name = "Virgin Island (Brit.)";
                    x.DisplayOrder = 100;
                })
            .Alter(850, x =>
                {
                    x.Name = "Virgin Island (USA)";
                    x.DisplayOrder = 100;
                })
            .Alter(876, x =>
                {
                    x.Name = "Wallis et Futuna";
                    x.DisplayOrder = 100;
                })
            .Alter(732, x =>
                {
                    x.Name = "Westsahara";
                    x.DisplayOrder = 100;
                })
            .Alter(887, x =>
                {
                    x.Name = "Jemen";
                    x.DisplayOrder = 100;
                })
            .Alter(894, x =>
                {
                    x.Name = "Sambia";
                    x.DisplayOrder = 100;
                })
            .Alter(716, x =>
                {
                    x.Name = "Zimbabwe";
                    x.DisplayOrder = 100;
                });
        }

        protected override void Alter(IList<Topic> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemName)
                .Alter("AboutUs", x =>
                {
                    x.Title = "Über uns";
                    x.Body = "<p>Fügen Sie hier Informationen über den Shop ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("CheckoutAsGuestOrRegister", x =>
                {
                    x.Title = "";
                    x.Body = "<p><strong>Anmelden und Zeit sparen!</strong><br />Melden Sie sich an und geniessen Sie folgende Vorteile:</p><ul><li>Schnell und einfach einkaufen</li><li>Jederzeit Zugriff auf Bestellstatus und Bestellhistorie</li></ul>";
                })
                .Alter("ConditionsOfUse", x =>
                {
                    x.Title = "AGB";
                    x.Body = "<p>Fügen Sie Ihre AGB hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("ContactUs", x =>
                {
                    x.Title = "Kontakt";
                    x.Body = "<p>Fügen Sie Ihre Kontaktdaten hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("ForumWelcomeMessage", x =>
                {
                    x.Title = "Foren";
                    x.Body = "<p>Fügen Sie eine Willkommens-Nachricht für das Forum hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("HomePageText", x =>
                {
                    x.Title = "Herzlich Willkommen";
                    x.Body = "<p>Fügen Sie eine Willkommens-Nachricht für den Online-Shop hier ein. Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("LoginRegistrationInfo", x =>
                {
                    x.Title = "Anmeldung/Registrierung";
                    x.Body = "<p>Fügen Sie Informationen zur Anmeldung hier ein.</p><p>Diesen Text können Sie auch im Administrations-Bereich editieren.</p>";
                })
                .Alter("PrivacyInfo", x =>
                {
                    x.Title = "Datenschutzerklärung";
                    x.Body = "<p>Legen Sie Ihrer Datenschutzerkl&#228;rung hier fest. Sie können dies in der Admin-Seite zu bearbeiten.</p>";
                })
                .Alter("ShippingInfo", x =>
                {
                    x.Title = "Versand und Rücksendungen";
                    x.Body = "<p>Informationen zu Versand und Rücksendungen. Sie können diese in der Admin-Seite zu bearbeiten.</p>";
                })
                .Alter("Imprint", x =>
                {
                    x.Title = "Impressum";
                    x.Body = @"<p>
                            <div>http://www.[mein-shop].de ist ein kommerzielles Angebot der</div>
                            <div>
                                MusterFirma<br>
                                Musterstr. 123<br>
                                44135 Dortmund<br>
                            </div>
                            <div>
                                Geschäftsführer: Max Mustermann<br>
                                Verantwortlich für den Inhalt der Website: Max Mustermann
                            </div>
                            <div>
                                Telefon: 0231/123 456<br>
                                Fax: 0231/123789<br>
                                E-Mail: info@[mein-shop].de<br>
                            </div>
                            <div>
                                SteuerNr.: 1234567890<br>
                                USt.-IdNr.: DE1234567890<br>
                            </div>
                            <div>
                                <Name und Anschrift des Auslandsvertreters>
                            </div>
                            <div>
                                <Zuständige Aufsichtsbehörde>
                            </div>
                            <div>
                                <Kammer>
                            </div>
                            <div>
                                <Gesetzliche Berufsbezeichnung>
                            </div>
                            <div>
                                <Verweis auf die berufsrechtlichen Regelungen>
                            </div>
                            </p>";
                })
                
                .Alter("Disclaimer", x =>
                {
                    x.Title = "Widerrufsrecht";
                    x.Body = "<p>Fügen Sie Ihr Widerrufsrecht hier ein. Sie können diese in der Admin-Seite zu bearbeiten.</p>";
                })
                .Alter("PaymentInfo", x =>
                {
                    x. Title = "Zahlungsarten";
                    x.Body = "<p><p>Fügen Sie Informationen zu Zahlungsarten hier ein. Sie können diese in der Admin-Seite zu bearbeiten.</p>";
                });

        }

        protected override void Alter(IList<ISettings> settings)
        {
            base.Alter(settings);

            settings
                .Alter<MeasureSettings>(x =>
                {
                    x.BaseDimensionId = base.DbContext.Set<MeasureDimension>().Where(m => m.SystemKeyword == "m").Single().Id;
                    x.BaseWeightId = base.DbContext.Set<MeasureWeight>().Where(m => m.SystemKeyword == "kg").Single().Id;
                })

                .Alter<SeoSettings>(x =>
                {
                    x.DefaultTitle = "Mein Shop";
                })

                .Alter<OrderSettings>(x =>
                {
                    x.ReturnRequestActions = "Reparatur,Ersatz,Gutschein";
                    x.ReturnRequestReasons = "Falschen Artikel erhalten,Falsch bestellt,Ware fehlerhaft bzw. defekt";
                    x.NumberOfDaysReturnRequestAvailable = 14;
                })

                .Alter<ShippingSettings>(x =>
                {
                    x.EstimateShippingEnabled = false;
                })
                .Alter<TaxSettings>(x =>
                {
                    x.TaxBasedOn = TaxBasedOn.ShippingAddress;
                    x.TaxDisplayType = TaxDisplayType.IncludingTax;
                    x.DisplayTaxSuffix = true;
                    x.ShippingIsTaxable = true;
                    x.ShippingPriceIncludesTax = false;
                    x.ShippingTaxClassId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Normal").Single().Id;
                    x.EuVatEnabled = true;
                    x.EuVatShopCountryId = base.DbContext.Set<Country>().Where(c => c.TwoLetterIsoCode == "DE").Single().Id;
                    x.EuVatAllowVatExemption = true;
                    x.EuVatUseWebService = false;
                    x.EuVatEmailAdminWhenNewVatSubmitted = true;
                })

                #region ContentSliderSettings
                .Alter<ContentSliderSettings>(x =>
                {
					var slidePics = base.DbContext.Set<Picture>().ToList();

					var slide1PicId = slidePics.Where(p => p.SeoFilename == base.GetSeName("slide-1")).First().Id;
					var slide2PicId = slidePics.Where(p => p.SeoFilename == base.GetSeName("slide-2")).First().Id;
					var slide3PicId = slidePics.Where(p => p.SeoFilename == base.GetSeName("slide-3")).First().Id;

                    //slide 1
                    x.Slides.Add(new ContentSliderSlideSettings
                    {
                        DisplayOrder = 1,
                        //LanguageName = "Deutsch",
                        Title = "Das Größte, was dem iPhone passieren konnte.",
                        Text = @"<ul>
                                    <li>Dünneres, leichteres Design</li>
                                    <li>4"" Retina Display.</li>
                                    <li>Ultraschnelle mobile Daten.</li>                       
                                </ul>",
                        Published = true,
                        LanguageCulture = "de-DE",
                        PictureId = slide1PicId,
                        //PictureUrl = _pictureService.GetPictureUrl(slide1PicId),
                        Button1 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "mehr...",
                            Type = "btn-primary",
                            Url = "~/apple-iphone-6",
                           
                        },
                        Button2 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "Jetzt Kaufen",
                            Type = "btn-danger",
                            Url = "~/apple-iphone-6"
                        },
                        Button3 = new ContentSliderButtonSettings
                        {
                            Published = false
                        }
                    });
                    //slide 2
                    x.Slides.Add(new ContentSliderSlideSettings
                    {
                        DisplayOrder = 2,
                        //LanguageName = "Deutsch",
                        Title = "Musik online kaufen!",
                        Text = @"<p>Hier kaufen & sofort herunterladen.</p>
                                 <p>Beste MP3-Qualität mit 320 kbit/s.</p>
                                 <p>Hörprobe und Sofortdownload mit Lichtgeschwindigkeit.</p>",
                        Published = true,
                        LanguageCulture = "de-DE",
                        PictureId = slide2PicId,
                        //PictureUrl = _pictureService.GetPictureUrl(slide2PicId),
                        Button1 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "mehr...",
                            Type = "btn-warning",
                            Url = "~/musik-kaufen-sofort-herunterladen"
                        },
                        Button2 = new ContentSliderButtonSettings
                        {
                            Published = false
                        },
                        Button3 = new ContentSliderButtonSettings
                        {
                            Published = false
                        }
                    });
                    //slide 3
                    x.Slides.Add(new ContentSliderSlideSettings
                    {
                        DisplayOrder = 3,
                        //LanguageName = "Deutsch",
                        Title = "Bereit für die Revolution?",
                        Text = @"<p>SmartStore.NET ist die neue dynamische E-Commerce Lösung von SmartStore.</p>
                                 <ul>
                                     <li>Auftrags-, Kunden- und Lagerverwaltung.</li>
                                     <li>SEO-optimiert | 100% Mobile-optimiert.</li>
                                     <li>Reviews &amp; Ratings | SmartStore.biz Import.</li>
                                 </ul>",
                        Published = true,
                        LanguageCulture = "de-DE",
                        PictureId = slide3PicId,
                        //PictureUrl = _pictureService.GetPictureUrl(slide3PicId),
                        Button1 = new ContentSliderButtonSettings
                        {
                            Published = true,
                            Text = "und vieles mehr...",
                            Type = "btn-success",
                            Url = "http://net.smartstore.com"
                        },
                        Button2 = new ContentSliderButtonSettings
                        {
                            Published = false
                        },
                        Button3 = new ContentSliderButtonSettings
                        {
                            Published = false
                        }
                    });
                });
            #endregion ContentSliderSettings



        }

        protected override void Alter(IList<ActivityLogType> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.SystemKeyword)
                .Alter("AddNewCategory", x =>
                    {
                        x.Name = "Erstellen einer Warengruppe";
                    })
                .Alter("AddNewCheckoutAttribute", x =>
                    {
                        x.Name = "Neues Checkout Attribut hinzugefügt";
                    })
                .Alter("AddNewCustomer", x =>
                    {
                        x.Name = "Neuen Kunden hinzugefügt";
                    })
                .Alter("AddNewCustomerRole", x =>
                    {
                        x.Name = "Neue Kundengruppe hinzugefügt";
                    })
                .Alter("AddNewDiscount", x =>
                    {
                        x.Name = "Neuer Rabatt hinzugefügt";
                    })
                .Alter("AddNewGiftCard", x =>
                    {
                        x.Name = "Neuer Geschenkgutschein hinzugefügt";
                    })
                .Alter("AddNewManufacturer", x =>
                    {
                        x.Name = "Neuer Hersteller hinzugefügt";
                    })
                .Alter("AddNewProduct", x =>
                    {
                        x.Name = "Neues Produkt hinzugefügt";
                    })
                .Alter("AddNewProductAttribute", x =>
                    {
                        x.Name = "Neues Produktattribut hinzugefügt";
                    })
                .Alter("AddNewSetting", x =>
                    {
                        x.Name = "Neue Einstellung hinzugefügt";
                    })
                .Alter("AddNewSpecAttribute", x =>
                    {
                        x.Name = "Neues Spezifikationsattribut hinzugefügt";
                    })
                .Alter("AddNewWidget", x =>
                    {
                        x.Name = "Neues Widget hinzugefügt";
                    })
                .Alter("DeleteCategory", x =>
                    {
                        x.Name = "Warengruppe gelöscht";
                    })
                .Alter("DeleteCheckoutAttribute", x =>
                    {
                        x.Name = "Checkout-Attribut gelöscht";
                    })
                .Alter("DeleteCustomer", x =>
                    {
                        x.Name = "Kunde gelöscht";
                    })
                .Alter("DeleteCustomerRole", x =>
                    {
                        x.Name = "Kundengruppe gelöscht";
                    })
                .Alter("DeleteDiscount", x =>
                    {
                        x.Name = "Rabatt gelöscht";
                    })
                .Alter("DeleteGiftCard", x =>
                    {
                        x.Name = "Geschenkgutschein gelöscht";
                    })
                .Alter("DeleteManufacturer", x =>
                    {
                        x.Name = "Hersteller gelöscht";
                    })
                .Alter("DeleteProduct", x =>
                    {
                        x.Name = "Produkt gelöscht";
                    })
                .Alter("DeleteProductAttribute", x =>
                    {
                        x.Name = "Produktattribut gelöscht";
                    })
                .Alter("DeleteReturnRequest", x =>
                    {
                        x.Name = "Rücksendeanforderung gelöscht";
                    })
                .Alter("DeleteSetting", x =>
                    {
                        x.Name = "Einstellung gelöscht";
                    })
                .Alter("DeleteSpecAttribute", x =>
                    {
                        x.Name = "Spezifikationsattribut gelöscht";
                    })
                .Alter("DeleteWidget", x =>
                    {
                        x.Name = "Widget gelöscht";
                    })
                .Alter("EditCategory", x =>
                    {
                        x.Name = "Warengruppe bearbeitet";
                    })
                .Alter("EditCheckoutAttribute", x =>
                    {
                        x.Name = "Checkout-Attribut bearbeitet";
                    })
                .Alter("EditCustomer", x =>
                    {
                        x.Name = "Kunde bearbeitet";
                    })
                .Alter("EditCustomerRole", x =>
                    {
                        x.Name = "Kundengruppe bearbeitet";
                    })
                .Alter("EditDiscount", x =>
                    {
                        x.Name = "Rabatt bearbeitet";
                    })
                .Alter("EditGiftCard", x =>
                    {
                        x.Name = "Geschenkgutschein bearbeitet";
                    })
                .Alter("EditManufacturer", x =>
                    {
                        x.Name = "Hersteller bearbeitet";
                    })
                .Alter("EditProduct", x =>
                    {
                        x.Name = "Produkt bearbeitet";
                    })
                .Alter("EditProductAttribute", x =>
                    {
                        x.Name = "Produktattribut bearbeitet";
                    })
                .Alter("EditPromotionProviders", x =>
                    {
                        x.Name = "Edit promotion providers";
                    })
                .Alter("EditReturnRequest", x =>
                    {
                        x.Name = "Rücksendewunsch bearbeitet";
                    })
                .Alter("EditSettings", x =>
                    {
                        x.Name = "Einstellungen bearbeitet";
                    })
                .Alter("EditSpecAttribute", x =>
                    {
                        x.Name = "Spezifikationsattribut bearbeitet";
                    })
                .Alter("EditWidget", x =>
                    {
                        x.Name = "Widget bearbeitet";
                    })
                .Alter("PublicStore.ViewCategory", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat eine Warengruppen-Detailseite angesehen";
                    })
                .Alter("PublicStore.ViewManufacturer", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat eine Hersteller-Detailseite angesehen";
                    })
                .Alter("PublicStore.ViewProduct", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat eine Produkt-Detailseite angesehen";
                    })
                .Alter("PublicStore.PlaceOrder", x =>
                    {
                        x.Name = "Öffentlicher Shop. Hat einen neuen Auftrag erteilt";
                    })
                .Alter("PublicStore.SendPM", x =>
                    {
                        x.Name = "Öffentlicher Shop. PN an Kunden geschickt";
                    })
                .Alter("PublicStore.ContactUs", x =>
                    {
                        x.Name = "Öffentlicher Shop. Kontaktformular benutzt";
                    })
                .Alter("PublicStore.AddToCompareList", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produkt zur Vergleichsliste hinzugefügt";
                    })
                .Alter("PublicStore.AddToShoppingCart", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produkt in den Warenkorb gelegt";
                    })
                .Alter("PublicStore.AddToWishlist", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produkt zur Wunschliste hinzugefügt";
                    })
                .Alter("PublicStore.Login", x =>
                    {
                        x.Name = "Öffentlicher Shop. Anmeldung";
                    })
                .Alter("PublicStore.Logout", x =>
                    {
                        x.Name = "Öffentlicher Shop. Abmeldung";
                    })
                .Alter("PublicStore.AddProductReview", x =>
                    {
                        x.Name = "Öffentlicher Shop. Produktrezension hinzugefügt";
                    })
                .Alter("PublicStore.AddNewsComment", x =>
                    {
                        x.Name = "Öffentlicher Shop. News-Kommentar hinzugefügt";
                    })
                .Alter("PublicStore.AddBlogComment", x =>
                    {
                        x.Name = "Öffentlicher Shop. Blogeintrag hinzugefügt";
                    })
                .Alter("PublicStore.AddForumTopic", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Thema erstellt";
                    })
                .Alter("PublicStore.EditForumTopic", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Thema bearbeitet";
                    })
                .Alter("PublicStore.DeleteForumTopic", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Thema gelöscht";
                    })
                .Alter("PublicStore.AddForumPost", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Beitrag erstellt";
                    })
                .Alter("PublicStore.EditForumPost", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Beitrag bearbeitet";
                    })
                .Alter("PublicStore.DeleteForumPost", x =>
                    {
                        x.Name = "Öffentlicher Shop. Foren-Beitrag gelöscht";
                    })
                .Alter("EditThemeVars", x =>
                {
                    x.Name = "Theme-Variablen geändert";
                })
                .Alter("ResetThemeVars", x =>
                {
                    x.Name = "Theme-Variablen zurückgesetzt";
                })
                .Alter("ImportThemeVars", x =>
                {
                    x.Name = "Theme Variablen importiert";
                })
                .Alter("ExportThemeVars", x =>
                {
                    x.Name = "Theme Variablen exportiert";
                });
                
        }

        protected override void Alter(IList<ScheduleTask> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Type)
				.Alter("SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services", x =>
                    {
                        x.Name = "E-Mail senden";
                    })
				.Alter("SmartStore.Services.Messages.QueuedMessagesClearTask, SmartStore.Services", x =>
					{
						x.Name = "E-Mail Queue bereinigen";
					})
				.Alter("SmartStore.Services.Media.TransientMediaClearTask, SmartStore.Services", x =>
				{
					x.Name = "Temporäre Uploads bereinigen";
				})
				.Alter("SmartStore.Services.Customers.DeleteGuestsTask, SmartStore.Services", x =>
                    {
                        x.Name = "Gastbenutzer löschen";
                    })
				.Alter("SmartStore.Services.Caching.ClearCacheTask, SmartStore.Services", x =>
                    {
                        x.Name = "Cache bereinigen";
                    })
				.Alter("SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services", x =>
                    {
                        x.Name = "E-Mail senden";
                    })
				.Alter("SmartStore.Services.Directory.UpdateExchangeRateTask, SmartStore.Services", x =>
                    {
                        x.Name = "Wechselkurse aktualisieren";
                    })
				.Alter("SmartStore.Services.Common.TempFileCleanupTask, SmartStore.Services", x =>
                    {
                        x.Name = "Temporäre Dateien bereinigen";
                    });
        }

        protected override void Alter(IList<SpecificationAttribute> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
            #region Cpu-Hersteller
.Alter(1, x =>
                    {
                        x.Name = "CPU-Hersteller";
                        //var attributeOptionNames = x.SpecificationAttributeOptions.OrderBy(y => y.DisplayOrder).Select(y => y.Name).ToList();
                        //foreach (var name in attributeOptionNames)
                        //{
                        //    name = 
                        //}
                    })
            #endregion

            #region Farbe
.Alter(2, x =>
                    {
                        x.Name = "Farbe";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "weiss";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "schwarz";

                        var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                        attribOption3.First().Name = "beige";

                        var attribOption4 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4);
                        attribOption4.First().Name = "rot";

                        var attribOption5 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 5);
                        attribOption5.First().Name = "blau";

                        var attribOption6 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 6);
                        attribOption6.First().Name = "grün";

                        var attribOption7 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 7);
                        attribOption7.First().Name = "gelb";

                    })
            #endregion

            #region Festplatten-Kapazität
.Alter(3, x =>
                    {
                        x.Name = "Festplatten-Kapazität";
                    })

            #endregion

            #region Arbeitsspeicher
.Alter(4, x =>
                    {
                        x.Name = "Arbeitsspeicher";
                    })
            #endregion

            #region OS
.Alter(5, x =>
                    {
                        x.Name = "Betriebssystem";
                    })
            #endregion

            #region Anschluss
.Alter(6, x =>
                    {
                        x.Name = "Anschluss";
                    })
            #endregion

            #region Geschlecht
.Alter(7, x =>
                    {
                        x.Name = "Geschlecht";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "Herren";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "Damen";
                    })
            #endregion

            #region Material
.Alter(8, x =>
                    {
                        x.Name = "Material";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "Edelstahl";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "Titan";

                        var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                        attribOption2.First().Name = "Kunststoff";

                        var attribOption4 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                        attribOption2.First().Name = "Aluminium";
                    })
            #endregion

            #region Technische Ausführung
            .Alter(9, x =>
                    {
                        x.Name = "Technische Ausführung";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "Automatik, selbstaufziehend";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "Automatik";

                        var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                        attribOption3.First().Name = "Quarz, batteriebetrieben";
                    })
            #endregion

            #region Verschluss
.Alter(10, x =>
                    {
                        x.Name = "Verschluss";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "Faltschließe";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "Sicherheitsfaltschließe";

                        var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                        attribOption2.First().Name = "Dornschließe";
                    })
            #endregion

            #region Glas
.Alter(11, x =>
                    {
                        x.Name = "Glas";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "Mineral";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "Saphir";
                    })
            #endregion

            #region Sprache
.Alter(12, x =>
                    {
                        x.Name = "Sprache";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "deutsch";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "englisch";

                        var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                        attribOption2.First().Name = "französisch";

                        var attribOption4 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4);
                        attribOption2.First().Name = "italienisch";
                    })
            #endregion

            #region Ausgabe
            .Alter(13, x =>
                    {
                        x.Name = "Ausgabe";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "gebunden";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "Taschenbuch";
                    })
            #endregion

            #region Kategorie
            .Alter(14, x =>
                    {
                        x.Name = "Genre";
                        var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                        attribOption1.First().Name = "Abenteuer";

                        var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                        attribOption2.First().Name = "Science-Fiction";

                        var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                        attribOption2.First().Name = "Geschichte";

                        var attribOption4 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4);
                        attribOption2.First().Name = "Internet & Computer";

                        var attribOption5 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 5);
                        attribOption2.First().Name = "Krimi";

                        var attribOption6 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 6);
                        attribOption2.First().Name = "Autos";

                        var attribOption7 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 7);
                        attribOption2.First().Name = "Roman";

                        var attribOption8 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 8);
                        attribOption2.First().Name = "Kochen & Backen";

                        var attribOption9 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 9);
                        attribOption2.First().Name = "Sachbuch";
                    })

            #endregion

            #region Computer-Typ
            .Alter(15, x =>
            {
                x.Name = "Computer-Typ";
                var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                attribOption1.First().Name = "Desktop";

                var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                attribOption2.First().Name = "All-in-One";

                var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                attribOption2.First().Name = "Laptop";

                var attribOption4 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4);
                attribOption2.First().Name = "Tablet";
            })

            #endregion

            #region Massenspeicher-Typ
.Alter(16, x =>
            {
                x.Name = "Massenspeicher-Typ";
            })

            #endregion

            #region Computer-Typ
.Alter(17, x =>
            {
                x.Name = "Größe (externe HDD)";
            })

            #endregion

            #region MP3-Qualität
.Alter(18, x =>
            {
                x.Name = "MP3-Qualität";
            })

            #endregion

            #region Musik-Genre
.Alter(19, x =>
            {
                x.Name = "Genre";
                var attribOption1 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 1);
                attribOption1.First().Name = "Blues";

                var attribOption2 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 2);
                attribOption2.First().Name = "Jazz";

                var attribOption3 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 3);
                attribOption2.First().Name = "Disko";

                var attribOption4 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 4);
                attribOption2.First().Name = "pop";

                var attribOption5 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 5);
                attribOption2.First().Name = "Funk";

                var attribOption6 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 6);
                attribOption2.First().Name = "Klassik";

                var attribOption7 = x.SpecificationAttributeOptions.Where(y => y.DisplayOrder == 7);
                attribOption2.First().Name = "R&B";
            })
            #endregion

            #region Hersteller
.Alter(19, x =>
            {
                x.Name = "Hersteller";
            })
            #endregion

            ;


            #region old code
            //entities.Clear();

            //var sa1 = new SpecificationAttribute
            //{
            //    Name = "CPU-Hersteller",
            //    DisplayOrder = 1,
            //};
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "AMD''",
            //    DisplayOrder = 1,
            //});
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "Intel''",
            //    DisplayOrder = 2,
            //});
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "ARM''",
            //    DisplayOrder = 3,
            //});
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "Samsung''",
            //    DisplayOrder = 4,
            //});
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "Apple''",
            //    DisplayOrder = 5,
            //});


            // add de-DE specific Tasks
            //entities = new List<SpecificationAttribute>
            //{
            //    sa1,
            //};
            #endregion
        }

        protected override void Alter(IList<ProductAttribute> entities)
        {
            base.Alter(entities);

            //entities.Clear();

            entities.WithKey(x => x.Alias)
                .Alter("Color", x =>
                {
                    x.Name = "Farbe";
                })
                .Alter("Custom Text", x =>
                {
                    x.Name = "eigener Text";
                })
                .Alter("HDD", x =>
                {
                    x.Name = "HDD";
                })
                .Alter("OS", x =>
                {
                    x.Name = "Betriebssystem";
                })
                .Alter("Processor", x =>
                {
                    x.Name = "Prozessor";
                })
                .Alter("RAM", x =>
                {
                    x.Name = "Arbeitsspeicher";
                })
                .Alter("Size", x =>
                {
                    x.Name = "Größe";
                })
                .Alter("Software", x =>
                {
                    x.Name = "Software";
                })
				.Alter("Game", x =>
				{
					x.Name = "Spiel";
				})
				.Alter("iPhone color", x =>
				{
					x.Name = "Farbe";
				})
				.Alter("Memory capacity", x =>
				{
					x.Name = "Speicherkapazität";
				});
        }

		protected override void Alter(IList<ProductVariantAttribute> entities)
		{
			base.Alter(entities);
			

			entities.WithKey(x => x.ProductAttribute.Alias)
				.Alter("Color", x =>
					{
						x.ProductVariantAttributeValues.First(v => v.Alias == "black").Name = "Schwarz";
						x.ProductVariantAttributeValues.First(v => v.Alias == "white").Name = "Weiß";
					})
				.Alter("iPhone color", x =>
				{
					x.ProductVariantAttributeValues.First(v => v.Alias == "silver").Name = "Silber";
					x.ProductVariantAttributeValues.First(v => v.Alias == "spacegray").Name = "Space-Grau";
				})
				.Alter("Game", x =>
					{
						x.ProductVariantAttributeValues.First(v => v.Alias == "Prince of Persia \"The Forgotten Sands\"").Name = "Prince of Persia \"Die vergessene Zeit\"";
					});
		}

        protected override void Alter(IList<ProductTemplate> entities)
        {
            base.Alter(entities);

			entities.WithKey(x => x.ViewPath)
				.Alter("ProductTemplate.Simple", x =>
				{
					x.Name = "Simple product";
				})
				.Alter("ProductTemplate.Grouped", x =>
				{
					x.Name = "Grouped product";
				});
        }

        protected override void Alter(IList<CategoryTemplate> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.ViewPath)
                .Alter("CategoryTemplate.ProductsInGridOrLines", x =>
                {
                    x.Name = "Products in Grid or Lines";
                });
        }

        protected override void Alter(IList<ManufacturerTemplate> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.ViewPath)
                .Alter("ManufacturerTemplate.ProductsInGridOrLines", x =>
                {
                    x.Name = "Products in Grid or Lines";
                });
        }

        protected override void Alter(IList<Category> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.MetaTitle)
            .Alter("Books", x =>
            {
                x.Name = "Bücher";
            })
            .Alter("Cook and enjoy", x =>
            {
                x.Name = "Kochen und Genießen";
            })
            .Alter("Computers", x =>
                {
                    x.Name = "Computer";
                })
            .Alter("Desktops", x =>
            {
                x.Name = "Desktop Computer";
            })
            .Alter("Notebooks", x =>
            {
                x.Name = "Notebook";
            })
            .Alter("Software", x =>
            {
                x.Name = "Software";
            })
            .Alter("Cell phones", x =>
            {
                x.Name = "Smartphones";
            })
            .Alter("Instant music", x =>
            {
                x.Name = "Musik kaufen & sofort herunterladen";
            })
            .Alter("Gift cards", x =>
            {
                x.Name = "Geschenkgutscheine";
            })
            .Alter("Watches", x =>
            {
                x.Name = "Uhren";
            })
			.Alter("Gaming Accessories", x =>
			{
				x.Name = "Zubehör";
			})
			.Alter("Games", x =>
			{
				x.Name = "Spiele";
			});
        }

        protected override void Alter(IList<Product> entities)
        {
            base.Alter(entities);

            try
            {
				#region variables

				string ps3FullDescription = "<table cellspacing=\"0\" cellpadding=\"1\"><tbody><tr><td>Prozessortyp&nbsp;</td><td>Cell Processor&nbsp;</td></tr><tr><td>Arbeitsspeicher (RAM)nbsp;</td><td>256 MB&nbsp;</td></tr><tr><td>Grafikchipsatz&nbsp;</td><td>nVidia RSX&nbsp;</td></tr><tr><td>Taktfrequenz&nbsp;</td><td>3.200 MHz&nbsp;</td></tr><tr><td>Abmessungen&nbsp;</td><td>290 x 60 x 230 mm&nbsp;</td></tr><tr><td>Gewicht&nbsp;</td><td>2.100 g&nbsp;</td></tr><tr><td>Speichermedium&nbsp;</td><td>Blu-ray&nbsp;</td></tr><tr><td>Stromverbrauch in Betrieb&nbsp;</td><td>190 Watt&nbsp;</td></tr><tr><td>Plattform&nbsp;</td><td>Playstation 3 (PS3)&nbsp;</td></tr><tr><td>Akku-Laufzeit&nbsp;</td><td>0 h&nbsp;</td></tr><tr><td>Anschlüsse&nbsp;</td><td>2x USB 2.0, AV-Ausgang, digitaler optischer Ausgang (SPDIF), HDMI&nbsp;</td></tr><tr><td>Soundmodi&nbsp;</td><td>AAC, Dolby igital, Dolby Digital Plus, Dolby TrueHD, DTS, DTS-HD, LPCM 7.1-Kanal&nbsp;</td></tr><tr><td>Unterstützte Auflösungen&nbsp;</td><td>576i, 576p, 720p, 1080i, 1080p Full HD&nbsp;</td></tr><tr><td>Serie&nbsp;</td><td>Sony Playstation 3&nbsp;</td></tr><tr><td>Veröffentlichungsjahr&nbsp;</td><td>2012&nbsp;</td></tr><tr><td>Mitgelieferte Hardware&nbsp;</td><td>Dual Shock 3-Controller&nbsp;</td></tr><tr><td>Farbe&nbsp;</td><td>schwarz&nbsp;</td></tr><tr><td>USK-Freigabe&nbsp;</td><td>0 Jahre&nbsp;</td></tr><tr><td>PEGI-Freigabe&nbsp;</td><td>3 Jahre&nbsp;</td></tr><tr><td>RAM-Typ&nbsp;</td><td>XDR-DRAM&nbsp;</td></tr><tr><td>Controller-Akku-Laufzeit&nbsp;</td><td>30 h&nbsp;</td></tr><tr><td>WLAN-Standard&nbsp;</td><td>IEEE 802.11 b/g&nbsp;</td></tr><tr><td>LAN-Standard&nbsp;</td><td>Gigabit Ethernet (10/100/1000 Mbit/s)&nbsp;</td></tr><tr><td>Daten-Kommunikation&nbsp;</td><td>Bluetooth 2.0 + EDR, Netzwerk (Ethernet), WLAN (Wi-Fi)&nbsp;</td></tr><tr><td>Controller-Eigenschaften&nbsp;</td><td>Beschleunigungssensor, Lagesensor (Gyrosensor), Headset-nschluss, Vibration&nbsp;</td></tr><tr><td>Spielsteuerungen&nbsp;</td><td>Bewegungssteuerung, Controller&nbsp;</td></tr><tr><td>Spielfunktionen&nbsp;</td><td>Community, Kindersicherung, Plattformübergreifendes Spielen, Remote Gaming, Sony PlayStation Network, Sony PlayStation Plus, Streaming (DLNA), Streaming (PlayStation Now/Gaikai)&nbsp;</td></tr><tr><td>Marketplace&nbsp;</td><td>Sony PlayStation Store&nbsp;</td></tr><tr><td>Internetfunktionen&nbsp;</td><td>Chat, Video Chat, Voice Chat, Webbrowser&nbsp;</td></tr><tr><td>Multimedia-Funktionen&nbsp;</td><td>Audio-CD-Wiedergabe, Blu-ray-Wiedergabe, DVD-Wiedergabe, Internet-Radio, Video-Wiedergabe&nbsp;</td></tr><tr><td>Streaming-ienste&nbsp;</td><td>Animax, Lovefilm, Maxdome, Mubi, Music on Demand, Sony Music Unlimited, Sony Video Unlimited, TuneIn, VidZone, Video on Demand, Watchever, YouTube&nbsp;</td></tr><tr><td>Ausstattung</td><td>onlinefähig/eingebautes Netzteil/3D-Ready</td></tr><tr><td>Sonstiges</td><td>bis zu 7 kabellose lageempfindliche Controller (Bluetooth) / PSP-Connectivity / keine Abwärtskompatibilität zu PlayStation 2-Spielen / Herunterladen von Filmen von Hollywood Studios aus dem Playstation Network, übertragbar auf PSP / Toploader-Laufwerk / Cross-Plattform-Funktionen (PS3 und PS Vita): Remote Play (Zugriff auf kompatible Inhalte auf PS3), Cross Buy (Spiel für anderes System kostenlos oder günstiger (online) dazukaufen), Cross-Goods (In-Game-Objekte für beide Systeme), Cross-Save (gespeichertes Spiel auf anderem System weiterspielen), Cross-Controller (PS Vita als Controller), Cross-Play (PSV vs. PS3), PlayStation Network-Konto erforderlich / 256 MB GDDR3 Grafikspeicher&nbsp;</td></tr></tbody></table>";
				string ps4FullDescription = "<ul><li>PlayStation 4, die neueste Generation des Entertainment Systems, definiert reichhaltiges und beeindruckendes Gameplay, völlig neu.</li><li>Den Kern der PS4 bilden ein leistungsstarker, eigens entwickelter Chip mit acht x86-Kernen (64 bit) sowie ein hochmoderner optimierter Grafikprozessor.</li><li>Ein neuer, hochsensibler SIXAXIS-Sensor ermöglicht mit dem DualShock 4 Wireless Controller eine erstklassige Bewegungssteuerung.</li><li>Der DualShock 4 bietet als Neuerungen ein Touchpad, eine Share-Taste, einen eingebauten Lautsprecher und einen Headset-Anschluss.</li><li>PS4 integriert Zweitbildschirme, darunter PS Vita, Smartphones und Tablets, damit Spieler ihre Lieblingsinhalte überall hin mitnehmen können.</li></ul>";

				#endregion variables

				entities.WithKey(x => x.MetaTitle)

				# region category Gift Cards
                .Alter("$5 Virtual Gift Card", x =>
				{
					x.Name = "5 € Geschenkgutschein";
					x.ShortDescription = "5 € Geschenkgutschein. Eine ideale Geschenkidee.";
					x.FullDescription = "<p>Wenn in letzter Minute mal wieder ein Geschenk fehlt oder man nicht weiß, was man schenken soll, dann bietet sich der Kauf eines Geschenkgutscheins an.</p>";
				})

				.Alter("$25 Virtual Gift Card", x =>
				{
					x.Name = "25 € Geschenkgutschein";
					x.ShortDescription = "25 € Geschenkgutschein. Eine ideale Geschenkidee.";
					x.FullDescription = "<p>Wenn in letzter Minute mal wieder ein Geschenk fehlt oder man nicht weiß, was man schenken soll, dann bietet sich der Kauf eines Geschenkgutscheins an.</p>";
				})

				.Alter("$50 Virtual Gift Card", x =>
				{
					x.Name = "50 € Geschenkgutschein";
					x.ShortDescription = "50 € Geschenkgutschein. Eine ideale Geschenkidee.";
					x.FullDescription = "<p>Wenn in letzter Minute mal wieder ein Geschenk fehlt oder man nicht weiß, was man schenken soll, dann bietet sich der Kauf eines Geschenkgutscheins an.</p>";
				})

				#endregion

				#region Bücher

				#region SPIEGEL-Bestseller
                .Alter("Überman: The novel", x =>
				{
					x.Name = "Überman: Der Roman";
					x.ShortDescription = "Gebundene Ausgabe";
					x.FullDescription = "<p> Nach Der Schatten des Windes und Das Spiel des Engels der neue große Barcelona-Roman von Carlos Ruiz Zafón. - Barcelona, Weihnachten 1957. Der Buchhändler Daniel Sempere und sein Freund Fermín werden erneut in ein großes Abenteuer hineingezogen. In der Fortführung seiner Welterfolge nimmt Carlos Ruiz Zafón den Leser mit auf eine fesselnde Reise in sein Barcelona. Unheimlich und spannend, mit unglaublicher Sogkraft und viel Humor schildert der Roman die Geschichte von Fermín, der 'von den Toten auferstanden ist und den Schlüssel zur Zukunft hat'. Fermíns Lebensgeschichte verknüpft die Fäden von Der Schatten des Windes mit denen aus Das Spiel des Engels. Ein meisterliches Vexierspiel, das die Leser rund um die Welt in Bann hält. </p> <p> Produktinformation<br> Gebundene Ausgabe: 416 Seiten<br> Verlag: S. Fischer Verlag; Auflage: 1 (25. Oktober 2012)<br> Sprache: Deutsch<br> ISBN-10: 3100954025<br> ISBN-13: 978-3100954022<br> Originaltitel: El prisionero del cielo<br> Größe und/oder Gewicht: 21,4 x 13,6 x 4,4 cm<br> </p>";
					x.Price = 16.99M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == TaxNameBooks).Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				#endregion SPIEGEL-Bestseller

				#region Kochen & Genießen

                .Alter("Best Grilling Recipes", x =>
				{
					x.ShortDescription = "Mehr als 100 regionale Favoriten Grill-Rezepte getestet und und für den Freiluft-Koch perfektioniert";
					x.Price = 16.99M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == TaxNameBooks).Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				.Alter("Cooking for Two", x =>
				{
					//x.Name = "Überman: Der Roman";
					//x.ShortDescription = "Mehr als 100 regionale Favoriten Grill-Rezepte getestet und und für den Freiluft-Koch perfektioniert";
					//x.FullDescription = "<p> Nach Der Schatten des Windes und Das Spiel des Engels der neue große Barcelona-Roman von Carlos Ruiz Zafón. - Barcelona, Weihnachten 1957. Der Buchhändler Daniel Sempere und sein Freund Fermín werden erneut in ein großes Abenteuer hineingezogen. In der Fortführung seiner Welterfolge nimmt Carlos Ruiz Zafón den Leser mit auf eine fesselnde Reise in sein Barcelona. Unheimlich und spannend, mit unglaublicher Sogkraft und viel Humor schildert der Roman die Geschichte von Fermín, der 'von den Toten auferstanden ist und den Schlüssel zur Zukunft hat'. Fermíns Lebensgeschichte verknüpft die Fäden von Der Schatten des Windes mit denen aus Das Spiel des Engels. Ein meisterliches Vexierspiel, das die Leser rund um die Welt in Bann hält. </p> <p> Produktinformation<br> Gebundene Ausgabe: 416 Seiten<br> Verlag: S. Fischer Verlag; Auflage: 1 (25. Oktober 2012)<br> Sprache: Deutsch<br> ISBN-10: 3100954025<br> ISBN-13: 978-3100954022<br> Originaltitel: El prisionero del cielo<br> Größe und/oder Gewicht: 21,4 x 13,6 x 4,4 cm<br> </p>";

					x.Price = 27.00M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == TaxNameBooks).Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				#endregion Kochen & Genießen

				#region Books : cars and motorcycles
                .Alter("Car of superlatives", x =>
				{
					x.Name = "Autos der Superlative: Die Stärksten, die Ersten, die Schönsten, Die Schnellsten";
					x.ShortDescription = "Gebundene Ausgabe";
					x.FullDescription = "<p> Für manche ist das Auto nur ein nützliches Fortbewegungsmittel.<br> Für alle anderen gibt es 'Autos - Das ultimative Handbuch' des Technik-Kenners Michael Dörflinger. Mit authentischen Bildern, allen wichtigen Daten und jeder Menge Infos präsentiert es die schnellsten, die innovativsten, die stärksten, die ungewöhnlichsten und die erfolgreichsten Exemplare der Automobilgeschichte. Ein umfassendes Handbuch zum gezielten Nachschlagen und ausgiebigen Schmökern. </p>";
					x.Price = 14.95M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == TaxNameBooks).Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				.Alter("Picture Atlas Motorcycles", x =>
				{
					x.Name = "Bildatlas Motorräder: Mit mehr als 350 brillanten Abbildungen";
					x.ShortDescription = "Gebundene Ausgabe";
					x.FullDescription = "<p> Motorräder stehen wie kein anderes Fortbewegungsmittel für den großen Traum von Freiheit und Abenteuer. Dieser reich illustrierte Bildatlas porträtiert in brillanten Farbfotografien und informativen Texten die berühmtesten Zweiräder der Motorradgeschichte weltweit. Von der urtümlichen Dampfmaschine unter dem Fahrradsattel des ausgehenden 19. Jahrhunderts bis hin zu den kraftstrotzenden, mit modernster Elektronik und Computertechnik ausgestatteten Superbikes unserer Tage zeichnet er ein eindrucksvolles Bild der Entwicklung und Fabrikation edler und rasanter Motorräder. Dem Mythos des motorisierten Zweirads wird dabei ebenso nachgegangen wie dem Motorrad als modernem Lifestyle-Produkt unserer Zeit. Länderspezifische Besonderheiten, firmenhistorische Hintergrundinformationen sowie spannende Geschichten und Geschichtliches über die Menschen, die eine der wegweisendsten Erfindungen der letzten Jahrhunderte vorantrieben und weiterentwickelten, machen diesen umfangreichen Bildband zu einem unvergleichlichen Nachschlagewerk für jeden Motorradliebhaber und Technikbegeisterten. </p> <p> • Umfassende Geschichte der legendärsten Modelle aller bedeutenden Motorradhersteller weltweit<br> • Mit mehr als 350 brillanten Farbaufnahmen und fesselnden Hintergrundtexten<br> • Mit informativen Zeichnungen, beeindruckenden Detailaufnahmen und erläuternden Info-Kästen<br> </p> <p> Inhalt • 1817 1913: Die Anfänge einer Erfolgsgeschichte<br> • 1914 1945: Massenmobilität<br> • 1946 1990: Kampf um den Weltmarkt<br> • Ab 1991: Das moderne Motorrad<br> • Kultobjekt Motorrad: Von der Fortbewegung zum Lifestyle<br> </p>";
					x.Price = 14.99M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == TaxNameBooks).Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				.Alter("The Car Book", x =>
				{
					x.Name = "Das Auto-Buch. Die große Chronik mit über 1200 Modellen";
					x.ShortDescription = "Gebundene Ausgabe";
					x.FullDescription = "<p> Marken, Modelle, Meilensteine<br> Das Auto - für manche ein Gebrauchsgegenstand, für andere Ausdruck des Lebensstils, Kultobjekt und große Leidenschaft. Nur wenige Erfindungen haben das Leben so verändert wie die des Automobils vor gut 125 Jahren - ein Grund mehr für diese umfangreiche Chronik. Das Auto-Buch lässt die Geschichte des Automobils lebendig werden. Es stellt über 1200 wichtige Modelle vor - von Karl Benz' Motorwagen über legendäre Kultautos bis zu modernsten Hybridfahrzeugen. Es erklärt die Meilensteine der Motortechnik und porträtiert die großen Marken und ihre Konstrukteure. Steckbriefe vom Kleinwagen bis zur Limousine und schicken Rennwagen jeder Epoche laden zum Stöbern und Entdecken ein. Der umfassendste und bestbebildert Bildband auf dem Markt - darüber freut sich jeder Autoliebhaber! </p> <p> Gebundene Ausgabe: 360 Seiten<br> Verlag: Dorling Kindersley Verlag (27. September 2012)<br> Sprache: Deutsch<br> ISBN-10: 3831022062<br> ISBN-13: 978-3831022069<br> Größe und/oder Gewicht: 30,6 x 25,8 x 2,8 cm<br> </p>";
					x.Price = 29.95M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Ermäßigt").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				.Alter("Fast Cars", x =>
				{
					x.Name = "Fast Cars, Bildkalender 2013";
					x.ShortDescription = "Spiralbindung";
					x.FullDescription = "<p> Großformat: 48,5 x 34 cm.<br> Dieser imposante Bildkalender mit silberner Ringbindung begeistert mit eindrucksvollen Aufnahmen von exklusiven Sportwagen. Wer Autos nicht nur als reine Nutzfahrzeuge begreift, findet hier die begehrtesten Statussymbole überhaupt: Die schnellen Fahrzeuge sind wirkungsvoll auf den gestochen scharfen, farbintensiven Fotos in Szene gesetzt und vermitteln Freiheit, Geschwindigkeit, Stärke und höchste technische Vollkommenheit. </p> <p> Angefangen vom 450 PS-starken Maserati GranTurismo MC Stradale über den stilvoll-luxuriösen Aston Martin Virage Volante bis zu dem nur in geringen Stückzahlen produzierten Mosler MT900S Photon begleiten die schnellen Flitzer mit Stil und Eleganz durch die Monate. Neben dem Kalendarium lenkt ein weiteres Foto den Blick auf sehenswerte Details. Dazu gibt es die wesentlichen Informationen zu jedem Sportwagen in englischer Sprache. Nach Ablauf des Jahres sind die hochwertigen Fotos eingerahmt ein absoluter Blickfang an der Wand eines jeden Liebhabers schneller Autos. Auch als Geschenk ist dieser schöne Jahresbegleiter wunderbar geeignet. 12 Kalenderblätter, neutrales und dezent gehaltenes Kalendarium. Gedruckt auf Papier aus nachhaltiger Forstwirtschaft. </p> <p> Für Freunde von luxuriösen Oldtimern ebenfalls bei ALPHA EDITION erhältlich: der großformatige Classic Cars Bildkalender 2013: ISBN 9783840733376. </p> <p> Produktinformation<br> Spiralbindung: 14 Seiten<br> Verlag: Alpha Edition (1. Juni 2012)<br> Sprache: Deutsch<br> ISBN-10: 3840733383<br> ISBN-13: 978-3840733383<br> Größe und/oder Gewicht: 48,8 x 34,2 x 0,6 cm<br> </p>";
					x.Price = 16.95M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Ermäßigt").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				.Alter("Motorcycle Adventures", x =>
				{
					x.Name = "Motorrad-Abenteuer: Fahrtechnik für Reise-Enduros";
					x.ShortDescription = "Gebundene Ausgabe";
					x.FullDescription = "<p> Moderne Reise-Enduros sind ideale Motorräder für eine Abenteuerreise. Ihre Technik ist jedoch komplex, ihr Gewicht beträchtlich. Das Fahrverhalten verändert sich je nach Zuladung und Strecke. Bevor die Reise losgeht, sollte man unbedingt ein Fahrtraining absolvieren. <br> Dieses hervorragend illustrierte Praxisbuch zeigt anhand vieler aussagekräftiger Serienfotos das richtige Fahren im Gelände in Sand und Schlamm, auf Schotter und Fels mit Gepäck und ohne. Neben dem Fahrtraining werden zahlreiche Informationen und Tipps zur Auswahl des richtigen Motorrades, zur Reiseplanung und zu praktischen Fragen unterwegs gegeben. </p>";
					x.Price = 44.90M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Ermäßigt").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})

				#endregion Books : cars and motorcycles

				#endregion Bücher

				#region computer

				#region computer-desktops

				#region Dell Inspiron One 23
                .Alter("Dell Inspiron One 23", x =>
				{
					x.ShortDescription = "Dieser 58 cm (23'')-All-in-One-PC mit Full HD, Windows 8 und leistungsstarken Intel® Core™ Prozessoren der dritten Generation ermöglicht eine praktische Interaktion mit einem Touchscreen.";
					x.FullDescription = "<p>Extrem leistungsstarker All-in-One PC mit Windows 8, Intel® Core™ i7 Prozessor, riesiger 2TB Festplatte und Blu-Ray Laufwerk.  </p>  <p>  Intel® Core™ i7-3770S Prozessor ( 3,1 GHz, 6 MB Cache) Windows 8 64bit , Deutsch<br> 8 GB1 DDR3 SDRAM bei 1600 MHz<br> 2 TB-Serial ATA-Festplatte (7.200 U/min)<br> 1GB AMD Radeon HD 7650<br> </p>";
					x.Price = 589.00M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Normal").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})
				#endregion Dell Inspiron One 23

				#region Dell Optiplex 3010 DT Base
                .Alter("Dell Optiplex 3010 DT Base", x =>
				{
					x.ShortDescription = "SONDERANGEBOT: Zusätzliche 50 € Rabatt auf alle Dell OptiPlex Desktops ab einem Wert von 549 €. Online-Coupon: W8DWQ0ZRKTM1, gültig bis 4.12.2013";
					x.FullDescription = "<p>Ebenfalls im Lieferumfang dieses Systems enthalten</p> <p> 1 Jahr Basis-Service - Vor-Ort-Service am nächsten Arbeitstag - kein Upgrade ausgewählt Keine Asset-Tag erforderlich</p> <p> Die folgenden Optionen sind in Ihren Auftrag aufgenommene Standardauswahlen.<br> German (QWERTZ) Dell KB212-B QuietKey USB Keyboard Black<br> X11301001<br> WINDOWS LIVE<br> OptiPlex™ Bestellung - Deutschland<br> OptiPlex™ Intel® Core™ i3 Aufkleber<br> Optische Software nicht erforderlich, Betriebssystemsoftware ausreichend<br> </p>";
					x.Price = 419.00M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Normal").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})
				#endregion Dell Optiplex 3010 DT Base

				#endregion computer-desktops

				#region Notebooks
				#region Acer Aspire One 8.9
                .Alter("Acer Aspire One 8.9", x =>
				{
					x.Name = "Acer Aspire One 8.9\" Mini-Notebook Case - (Schwarz)";
					x.ShortDescription = "Acer definiert mit dem Aspire One mobile Konnektivität neu, dem revolutionären Spaß und Power Netbook in der zierlichen 8.9\" Größe. ";
					x.FullDescription = "<p> Von der Betätigung des Powerknopfes an, ist das Aspire One in nur wenigen Sekunden betriebsbereit. Sobald an, ist die Arbeit sehr einfach: ein Heimarbeitsplatz der die heute benötigten vier Bereiche abdeckt, verbunden bleiben, arbeiten, spielen und Ihr Leben unterwegs organisieren. Und der Aspire One ist etwas Besonderes, Sie können alles so individualisieren das es für Sie das Richtige ist. Schnell, einfach und unbeschreiblich schick. Ihr Style ist Ihre Unterschrift. Es ist Ihre Identität, Ihre Persönlichkeit und Ihre Visitenkarte. Ihr Style zeigt Ihrer Umwelt wie Sie sind und wie Sie Ihr Leben leben, online und offline. Das alles benötigen Sie, um Sie selbst zu sein. Ihr Style kommt in verschiedenen Farben, jede mit einem individuellen Charakter. Kleiner als ein durchschnittliches Tagebuch, das Aspire One bringt Freiheit in Ihre Hände. </p> <p> Allgemein<br> Betriebssystem: Microsoft Windows XP Home Edition, Linux Linpus Lite <br> Herstellergarantie: 1 Jahr Garantie<br> Systemtyp: Netbook<br> MPN: LU.S080B.069, LU.S050B.081, LU.S040B.079, LU.S090B.079, LU.S040B.198, LU.S040A.048, LU.S050A.050, LU.S050B.080, LU.S040B.078, 099915639, LU.S050A.074, LU.S360A.203, LU.S450B.030, LU.S050B.159<br> Speicher<br> RAM: 1 GB ( 1 x 512 MB + 512 MB (integriert) ), 1 GB<br> Max. unterstützter RAM-Speicher: 1.5 GB<br> Technologie: DDR2 SDRAM<br> Geschwindigkeit: 533 MHz   <br> Formfaktor: SO DIMM 200-polig  <br> Anz. Steckplätze: 1<br> Leere Steckplätze: 0, 1<br> Display<br> Typ: 22.6 cm ( 8.9\" )<br> Auflösung: 1024 x 600 ( WSVGA )<br> Breitwand: Ja<br> LCD-Hintergrundbeleuchtung: LED-Hintergrundbeleuchtung     <br> Farbunterstützung: 262.144 Farben, 24 Bit (16,7 Millionen Farben)<br> Besonderheiten: CrystalBrite<br> Batterie<br> Betriebszeit: Bis zu 7 Stunden, Bis zu 3 Stunden<br> Kapazität: 2600 mAh, 2200 mAh<br> Technologie: 6 Zellen Lithium-Ionen, 3 Zellen Lithium-Ionen, Lithium-Ionen<br> Herstellergarantie<br> Service & Support:<br> Reisegarantie - 1 Jahr, Begrenzte Garantie - 1 Jahr, Internationale Garantie - 1 Jahr<br> Begrenzte Garantie - 1 Jahr, Reisegarantie - 1 Jahr<br> Begrenzte Garantie - 1 Jahr, Begrenzte Garantie - 1 Jahr<br> Reisegarantie - 1 Jahr<br> Navigation<br>Empfänger: GPS<br></p>";
					x.Price = 210.60M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Normal").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
				})
				#endregion Acer Aspire One 8.9

				#endregion Notebooks

				#endregion computer

				#region SmartPhones
				
				#region Apple iPhone
				
				.Alter("Apple iPhone 6", x =>
				{
					x.ShortDescription = "Apple iPhone 6 Simlock frei Neu";
					x.FullDescription = "<p>Das iPhone 6 ist nicht einfach nur größer. Es ist wirklich in allem besser. Es ist länger und breiter, aber deutlich dünner. Leistungsstärker, aber unglaublich energieeffizient. Seine glatte Oberfläche aus Metall schließt nahtlos an das neue Retina HD Display an. So entsteht eine durchgehende Form, in der Hardware und Software in perfekter Einheit zusammenarbeiten – für eine neue iPhone Generation, die nach allen Maßstäben besser ist.</p><p>Das iPhone 6 kommt mit einem A8 Chip auf Basis einer 64 Bit Desktoparchitektur der zweiten Generation. Seine unglaubliche Leistung wird durch einen M8 Motion Coprozessor erweitert, der Aktivität mit fortschrittlichen Sensoren ermittelt, darunter auch ein neues Barometer.</p><p>Mit keiner anderen Kamera werden von so vielen Menschen so viele Fotos gemacht wie mit dem iPhone. Jetzt hat die iSight Kamera einen neuen Sensor mit Focus Pixels und neue Videofunktionen wie 1080p HD Videos mit 60 Bildern pro Sekunde, Zeitlupenvideos mit 240 Bildern pro Sekunde und Zeitraffervideo.</p><p><ul><li>Abmessungen (HxBxT)	138.1 x 67 x 6.9 Millimeter</li><li>Gewicht	129 Gramm</li><li>Strahlungswert (SAR-Wert)	unbekannt</li><li>Erscheinungstermin	09/2014</li><li>Betriebssystem	Apple iOS</li><li>Systemversion	8.0</li><li>Netzstandards	GSM1800 GSM1900 GSM850 GSM900W-CDMA (UMTS)</li><li>Flugzeugmodus	ja</li><li>Akku-Typ	Li-Ionen</li><li>Akkuleistung	1810 mAh</li><li>max. Bereitschaftszeit	250 Stunden</li><li>max. Sprechzeit	840 Minuten</li></ul></p>";
					x.Price = 579.00M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Normal").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
					x.ProductReviews.Clear();
				})
				#endregion Apple iPhone

				#endregion SmartPhones

				#region Instant Downloads
				
                #region Antonio Vivildi: then spring
                
                .Alter("Antonio Vivaldi: spring", x =>
				{
					x.Name = "Antonio Vivaldi: Der Frühling";
					x.ShortDescription = "MP3, 320 kbit/s";
					x.FullDescription = "<p>Antonio Vivaldi: Der Fr&uuml;hling</p> <p><b>Antonio Lucio Vivaldi</b><span>&nbsp;(*&nbsp;</span>4. M&auml;rz<span>&nbsp;</span>1678<span>&nbsp;in&nbsp;</span>Venedig<span>; &dagger;&nbsp;</span>28. Juli<span>&nbsp;</span>1741<span>&nbsp;in&nbsp;</span>Wien<span>) war ein venezianischer&nbsp;</span>Komponist<span>&nbsp;und&nbsp;</span>Violinist<span>&nbsp;im&nbsp;</span>Barock<span>.</span></p> <p><b>Die vier Jahreszeiten</b>&nbsp;(italienisch&nbsp;<span lang=\"it\" class=\"lang\"><i>Le quattro stagioni</i></span>) hei&szlig;t das wohl bekannteste Werk&nbsp;Antonio Vivaldis. Es handelt sich um vier&nbsp;Violinkonzerte&nbsp;mit au&szlig;ermusikalischen&nbsp;Programmen; jedes Konzert portr&auml;tiert eine&nbsp;Jahreszeit. Dazu ist den einzelnen Konzerten jeweils ein &ndash; vermutlich von Vivaldi selbst geschriebenes &ndash;&nbsp;Sonett&nbsp;vorangestellt; fortlaufende Buchstaben vor den einzelnen Zeilen und an den entsprechenden Stellen in der&nbsp;Partitur&nbsp;ordnen die verbale Beschreibung der Musik zu.</p> <p>Vivaldi hatte bereits zuvor immer wieder mit au&szlig;ermusikalischen Programmen experimentiert, die sich h&auml;ufig in seinen Titeln niederschlagen; die genaue Ausdeutung von Einzelstellen der Partitur ist aber f&uuml;r ihn ungew&ouml;hnlich. Seine Erfahrung als virtuoser Geiger erlaubte ihm den Zugriff auf besonders wirkungsvolle Spieltechniken; als Opernkomponist hatte er einen starken Sinn f&uuml;r Effekte entwickelt; beides kam ihm hier zugute.</p> <p>Wie der Titel bereits nahelegt, werden vor allem Naturerscheinungen imitiert &ndash; sanfte Winde, heftige St&uuml;rme und Gewitter sind Elemente, die in allen vier Konzerten auftreten. Hinzu kommen verschiedene Vogelstimmen und sogar ein Hund, weiter menschliche Bet&auml;tigungen wie etwa die Jagd, ein Bauerntanz, das Schlittschuhlaufen einschlie&szlig;lich Stolpern und Hinfallen bis hin zum schweren Schlaf eines Betrunkenen.</p> <p>Das Werk stammt aus dem Jahre 1725 und ist in zwei Druckausgaben erhalten, die offenbar mehr oder weniger zeitgleich in Amsterdam und Paris erschienen.</p> <p><span><br /></span></p>";
					x.ProductReviews.Clear();
				})

				#endregion Antonio Vivildi: then spring

				#region Beethoven für Elise
                .Alter("Ludwig van Beethoven: Für Elise", x =>
				{
					x.Name = "Ludwig van Beethoven: Für Elise";
					x.ShortDescription = "Ludwig van Beethoven: Für Elise. Eine von Beethovens populärsten Kompositionen.";
					x.FullDescription = "<p> Die früheste, 1973 bekannt gewordene Fassung der „Kernmelodie“[5] notierte Beethoven 1808 in ein Skizzenbuch zur Pastorale. Einige aus dem Skizzenbuch herausgelöste Seiten bilden heute das Autograph Mus. ms. autograph. Beethoven Landsberg 10 der Staatsbibliothek Preußischer Kulturbesitz in Berlin. Die Melodie, die eindeutig als Kern des Klavierstückes WoO 59 zu erkennen ist,[2] befindet sich in den Zeilen 6 und 7 der Seite 149. Es handelt sich um eine einstimmige, sechzehntaktige Melodie, die sich besonders bei den Auftakten des Mittelteiles und bei den Schlusswendungen der Takte 7 und 15 sowie durch das Fehlen des zweitaktigen Orgelpunktes auf E von späteren Fassungen unterscheidet.[2] Diese Melodie nahm Beethoven 1810 wieder auf, modifizierte sie und fügte ihr weitere Teile hinzu. Das geschah in Beethovens Handschrift BH 116[6] und vermutlich auch in dem Autograph, das zu Babette Bredl gelangte und von Ludwig Nohl abgeschrieben und 1867 erstmals veröffentlicht wurde.[7][8] </p> <p> In BH 116 lassen sich drei Arbeitsphasen erkennen: eine erste Niederschrift im Jahre 1810, Korrekturen daran von 1810 und eine Bearbeitung aus dem Jahre 1822. Die Bearbeitung von 1822 hatte das Ziel, das Klavierstück in eine für eine Veröffentlichung taugliche Fassung zu bringen. Es sollte als No 12 den Schluss eines Zyklus von Bagatellen bilden. Dieser Plan wurde allerdings nicht ausgeführt.[9] 1822 überschrieb Beethoven das Klavierstück mit „molto grazioso“. Er verschob die Begleitfiguren des A-Teils in der linken Hand um ein Sechzehntel nach rechts und entlastete dabei den Taktanfang. Außerdem führte er die Begleitfigur teilweise in eine tiefere Lage und weitete damit den Klang aus.[10] Im Teil B kehrte Beethoven zu einer melodisch und rhythmisch komplizierteren, 1810 verworfenen Fassung zurück. Den vermutlichen Gesamtaufbau des Klavierstückes ließ er nicht völlig unangetastet und fügte vier bisher ungenutzte Takte als Überleitung zum Teil B ein. Vier 1822 notierte Einleitungstakte, die zum A-Teil passen, strich er dagegen wieder.[11] Bei der Anweisung für die Reprise des letztmals wiederkehrenden Teiles A schrieb er „una corda“ vor, was sich auf diesen Teil selbst beziehen kann oder nur auf den neu entworfenen, dreitaktigen, wahrscheinlich akkordisch gedachten, aber nur einstimmig notierten Schluss.[12] Eine vollständige Fassung als Resultat der Bearbeitung von 1822 stellte Beethoven nicht her.[13][14] </p>";
					x.ProductReviews.Clear();
				})

				#endregion Beethoven für Elise

				#endregion Instant Downloads

				#region watches

				#region Certina DS Podium Big Size
                .Alter("Certina DS Podium Big Size", x =>
				{
					x.Name = "Certina DS Podium Big Size Herrenchronograph";
					x.ShortDescription = "C001.617.26.037.00";
					x.FullDescription = "<p><strong>Produktbeschreibung</strong></p> <ul> <li>Artikelnr.: 3528 C001.617.26.037.00</li> <li>Certina DS Podium Big Size Herrenchronograph</li> <li>Schweizer ETA Werk</li> <li>Silberfarbenes Edelstahlgeh&auml;use mit schwarzer L&uuml;nette</li> <li>Wei&szlig;es Zifferblatt mit silberfarbenen Ziffern und Indizes</li> <li>Schwarzes Lederarmband mit Faltschliesse</li> <li>Kratzfestes Saphirglas</li> <li>Datumsanzeige</li> <li>Tachymeterskala</li> <li>Chronograph mit Stoppfunktion</li> <li>Durchmesser: 42 mm</li> <li>Wasserdichtigkeits -Klassifizierung 10 Bar (nach ISO 2281): Perfekt zum Schwimmen und Schnorcheln</li> <li>100 Tage Niedrigpreisgarantie, bei uhrzeit.org kaufen Sie ohne Preisrisiko!</li> </ul>";
					x.Price = 479.00M;
					x.DeliveryTime = base.DbContext.Set<DeliveryTime>().Where(dt => dt.DisplayOrder == 0).Single();
					x.TaxCategoryId = base.DbContext.Set<TaxCategory>().Where(tc => tc.Name == "Normal").Single().Id;
					x.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
					x.OrderMinimumQuantity = 1;
					x.OrderMaximumQuantity = 10000;
					x.StockQuantity = 10000;
					x.NotifyAdminForQuantityBelow = 1;
					x.AllowBackInStockSubscriptions = false;
					x.Published = true;
					x.CreatedOnUtc = DateTime.UtcNow;
					x.UpdatedOnUtc = DateTime.UtcNow;
					x.IsShipEnabled = true;
					x.ProductReviews.Clear();
				})

				#endregion Certina DS Podium Big Size
				
                #endregion watches

				#region gaming

				.Alter("Playstation 3 Super Slim", x =>
				{
					x.ShortDescription = "Die Sony PlayStation 3 ist die Multi-Media-Console für die nächste Generation digitalem Home-Entertainment. Mit der Blu-Ray-Technologie genießen Sie Filme in HD.";
					x.FullDescription = ps3FullDescription;
				})
				.Alter("DUALSHOCK 3 Wireless Controller", x =>
				{
					x.ShortDescription = "Ausgestattet mit SIXAXIS™ Motion-sensing-Technologie und Drucksensoren für jede Aktionsschaltfläche, bietet die DUALSHOCK®3 wireless Controller für die PlayStation ® 3 die intuitivste Gameplay-Erfahrung.";
					x.FullDescription = "<ul><li><h4>Gewicht und Maße</h4><ul><li>Größe und Gewicht (ca.) : 27 x 23,5 x 4 cm ; 191 g</li></ul></li></ul>";
				})
				.Alter("Assassin's Creed III", x =>
				{
					x.ShortDescription = "Eliminieren Sie Ihre Gegner mit einem erweiterten Waffenarsenal, darunter Bögen, Pistolen, Tomahawk und die charakteristische Klinge des Assassinenordens. Erkunden Sie dicht bevölkerte Städte entlang der ausgedehnten und gefährlichen Grenze zur Wildnis, wo es von wilden Tieren nur so wimmelt. Eine ganz neue Spielengine zeigt die Brutalität und Schönheit einer Nation während ihres epischen Kampfes um Unabhängigkeit.";
					x.FullDescription = "<p>Vor dem Hintergrund der Amerikanischen Revolution im späten 18. Jahrhundert präsentiert Assassin’s Creed III einen neuen Helden: Ratohnhaké:ton, der teils uramerikanischer, teils englischer Abstammung ist. Er nennt sich selbst Connor und wird die neue Stimme der Gerechtigkeit im uralten Krieg zwischen Assassinen und Templern. Der Spieler wird zum Assassinen im Krieg um Freiheit und gegen Tyrannei in der aufwändigsten und flüssigsten Kampferfahrung der Reihe. Assassin’s Creed III umfasst die Amerikanische Revolution und nimmt den Spieler mit auf eine Reise durch das lebhafte, ungezähmte Grenzland, vorbei an geschäftigen Kolonialstädten, bis hin zu den erbittert umkämpften und chaotischen Schlachtfeldern, auf denen George Washingtons Kontinentalarmee mit der eindrucksvollen Britischen Armee zusammenstieß.</p><p>Das 18. Jahrhundert in Nordamerika. Nach mehr als 20 Jahren voller Konflikte stehen die 13 amerikanischen Kolonien und die Britische Krone am Rande eines handfesten Krieges. Die Schlachtlinien werden vorbereitet. Blutvergießen ist unvermeidbar. Aus der Asche dieses brennenden Dorfes wird ein neuer Assassine auferstehen. Als Sohn mohikanischer und britischer Vorfahren wird sein Kampf für Freiheit und Gerechtigkeit in den Wirren der Revolution Gestalt annehmen.</p>";
				})
				.Alter("PlayStation 3 Assassin's Creed III Bundle", x =>
				{
					x.ShortDescription = "500GB PlayStation®3 Console, 2 × DUALSHOCK®3 wireless controller und Assassin's Creed® III.";
					x.FullDescription = ps3FullDescription;
                    x.BundleTitleText = "Produktset besteht aus";
				})
				.Alter("PlayStation 4", x =>
				{
					x.ShortDescription = "In Zusammenarbeit mit einigen der kreativsten Köpfe der Industrie entstanden, bietet die PlayStation® 4 atemberaubende und einzigartige Gaming-Erlebnis.";
					x.FullDescription = ps4FullDescription;
				})
				.Alter("DUALSHOCK 4 Wireless Controller", x =>
				{
					x.ShortDescription = "Durch Kombination klassischer Steuerelemente mit innovativen neuen Möglichkeiten des Spielens, ist der Wireless Controller DUALSHOCK® 4 der evolutionäre Controller für eine neue Ära des Gaming.";
					x.FullDescription = "<div><div><p>Der DualShock 4 Controller bietet einige neue Features, die völlig neue Wege des Spielens ermöglichen und wohlüberlegt mit Unterstützung aus der Entwickler-Community zusammengestellt wurden. Die “Share”-Taste erlaubt es Ihnen ganz einfach, Gameplay in Echtzeit über Streaming-Seiten wie Ustream zu veröffentlichen. Dort können andere Gamer Spiele kommentieren oder sogar direkt beitreten und aushelfen. Daneben können Sie über die “Share”-Taste Bilder oder Videos zu Facebook hochladen. Auf der Vorderseite des DualShock 4 befindet sich eine LED-Leuchte, die in unterschiedlichen Farben erstrahlen kann, um die Farbe des Charakters im Spiel abzubilden und einen Spieler so leicht zu identifizieren. Die Farben können dem Spieler auch nützliche Informationen liefern, zum Beispiel wenn der Charakter im Spiel Schaden nimmt.</p> <p>Der DualShock 4 wurde zusammen mit einem zweiten Peripherie-Gerät entwickelt, einer Kamera (nicht im Lieferumfang enthalten), die die Tiefe der Umgebung vor ihr wahrnehmen kann und die mithilfe der LED-Leuchte die Position des Controllers im dreidimensionalen Raum bestimmen kann.</p> <p>Der DualShock 4 bietet auf seiner Vorderseite ein Touchpad und somit eine neue Input-Methode. Zusätzlich gibt es einen eingebauten Lautsprecher und einen Headset-Anschluss, um hochklassige Soundeffekte aus den Spielen zu übertragen. Mithilfe eines Headsets (nicht im Lieferumfang enthalten) können Sie während des Online-Gamings mit Ihren Freunden chatten und Soundeffekte aus dem Controller hören können. Der DualShock 4 adaptiert die bekannte Form des kabellosen DualShock 3 Controllers und bietet einige entscheidende Verbesserungen:</p> <ul> <li>Ein neuer, hochsensibler SIXAXIS-Sensor ermöglicht eine erstklassige Bewegungssteuerung.</li> <li>Die Dual-Analogsticks wurden verbessert und bieten eine größere Präzision, ein besseres Material auf den Oberflächen sowie eine verbesserte Form, um eine noch genauere Steuerung zu ermöglichen.</li> <li>Die L2/R2-Tasten oben auf dem Controller wurden abgerundet und sind jetzt einfacher und flüssiger zu bedienen.</li> <li>Eine neue “Options”-Taste kombiniert die Funktionen der “Select”- und “Start”-Tasten auf dem DualShock 3 zur Steuerung der Ingame-Menüs.</li> </ul> <h4>Technische Spezifikationen</h4> <ul> <li>Außenabmessungen Ca. 162 × 52 × 98 mm (B × H × T) (vorläufig) </li><li>Gewicht Ca. 210 g (vorläufig) </li><li><b>Tasten / Schalter:</b> PS-Taste, SHARE-Taste, OPTIONS-Taste, Richtungstasten (oben/unten/links/rechts), Aktionstasten (Dreieck, Kreis, Kreuz, Quadrat), R1/L1/R2/L2-Taste, linker Stick / L3-Taste, rechter Stick / R3-Taste, Pad-Taste </li><li><b>Touchpad:</b> 2-Punkt-Touchpad, Klick-Mechanismus, kapazitiv</li> <li><b>Bewegungssensor:</b> Sechsachsiges Motion-Sensing-System (dreiachsiges Gyroskop, dreiachsiger Beschleunigungssensor) </li><li><b>Sonstige Funktionen:</b> Lichtbalken, Vibration, integrierter MonoLautsprecher</li> <li><b>Anschluss:</b> USB (Micro B), Erweiterungs-Port, Stereo-Kopfhörerbuchse </li><li><b>Wireless-Kommunikation:</b> Bluetooth 2.1+EDR</li> <li><b>Batterie:</b> Typ Eingebauter Lithium-Ionen-Akku</li> <li><b>Spannung:</b> 3,7 V Gleichspannung (vorläufig)</li> <li><b>Kapazität:</b> 1000 mAh (vorläufig)</li> <p></p> <p> <i>Kurzfristige Änderungen des Herstellers vorbehalten.</i> </p> </ul></div><div></div></div>";
				})
				.Alter("PlayStation 4 Camera", x =>
				{
					x.Name = "PlayStation 4 Kamera";
					x.ShortDescription = "Eine Kamera, die die Tiefe der Umgebung vor ihr wahrnehmen kann und die mithilfe der LED-Leuchte die Position des Controllers im dreidimensionalen Raum bestimmen kann.";
					x.FullDescription = "<p>Die neue Kamera besitzt vier Mikrofone mit der eine präzise Geräusch-Erkennung und -Ortung möglich ist und wird den PlayStation Move-Motion-Controller (nicht im Lieferumfang enthalten) mit einer größeren Präzision als je zuvor unterstützen.</p><p><ul><li><b>Farbe:</b> Jet Black</li> <li><b>Außenabmessungen:</b> Ca. 186 × 27 × 27 mm (B × H × T) (vorläufig)</li> <li><b>Gewicht:</b> Ca. 183 g (vorläufig)</li> <li><b>Videopixel:</b> (Maximum) 2 × 1280 × 800 Pixel</li> <li><b>Videobildrate:</b> 1280×800 Pixel bei 60 fps, 640×400 Pixel bei 120 fps, 320×192 Pixel bei 240 fps</li> <li><b>Videoformat:</b> RAW, YUV (unkomprimiert)</li> <li><b>Objektiv:</b> Zwei Objektive, F-Wert/F2.0 Fixfokus</li> <li><b>Erfassungsbereich</b> 30 cm ～ ∞</li> <li><b>Sichtfeld</b> 85°</li> <li><b>Mikrofon:</b> 4-Kanal Mikrofon-Array</li> <li><b>Verbindungsart:</b> Spezieller PS4-Stecker (AUX-Stecker)</li> <li><b>Kabellänge:</b> Ca. 2 m (vorläufig)</li> <p></p><p> <i>Kurzfristige Änderungen des Herstellers vorbehalten.</i> </p></ul></p>";
				})
				.Alter("PlayStation 4 Bundle", x =>
				{
					x.ShortDescription = "PlayStation®4 Console, DUALSHOCK®4 wireless controller und PS4 Kamera.";
					x.FullDescription = ps4FullDescription;
                    x.BundleTitleText = "Produktset besteht aus";
				})
				.Alter("Accessories for unlimited gaming experience", x =>
				{
					x.Name = "Zubehör für unbegrenzte Gaming-Erlebnis";
					x.ShortDescription = "Die Zukunft des Gaming ist jetzt mit dynamischen, verbundenen Spiele, starke Grafikleistung und Geschwindigkeit, intelligente Personalisierung, integrierter sozialer Fähigkeiten und innovative Second-Screen-Funktionen. Der geniale Höhepunkt der kreativsten Köpfe in der Industrie, bietet PlayStation® 4 eine einzigartige Spielumgebung, die Ihnen den Atem rauben wird.";
					x.FullDescription = "<ul><li>Tauchen Sie ein in eine neue Welt des Spielens mit starker Grafikleistung und Geschwindigkeit.</li><li>Beseitigen Sie längere Ladezeiten der gespeicherte Spiele mit Suspend-Modus.</li><li>Sofortiges spielen ohne zu warten dank Herunterladen und Aktialisierung im Hintergrund.</li><li>Teilen Sie sofort Bilder und Videos Ihrer Lieblings-Gaming-Momente auf Facebook mit dem DUALSHOCK® 4 Controller.</li><li>Ausgestrahlt, während Sie Echtzeit über Ustream spielen.</li></ul>";
				})
				.Alter("Watch Dogs", x =>
				{
					x.ShortDescription = "Hacken Sie die Stadt und machen Sie sie zu Ihrer Waffe. Bewegen Sie sich dynamisch durch die Stadt, nutzen Sie Abkürzungen durch Gebäude, klettern Sie auf Dächer und über Hindernisse";
					x.FullDescription = "<p>Es braucht nur eine Fingerbewegung und wir sind mit unseren Freunden verbunden. Wir kaufen die aktuellsten Gadgets und Ausrüstungen. Wir finden heraus, was in der Welt passiert. Aber mit der gleichen Bewegung entsteht ein digitaler Schatten, der sich zunehmend vergrößert. Mit jeder Verbindung hinterlassen wir eine digitale Spur, die jede Bewegung von uns aufzeichnet: unsere Vorlieben sowie unsere Abneigungen. Und es sind nicht nur die Menschen – jede Großstadt ist heutzutage vernetzt. Die städtische Infrastruktur wird von komplexen Systemen überwacht und kontrolliert.</p><p>In Watch Dogs heißt dieses System Central Operating System (ctOS). Es kontrolliert den Großteil der Technologie der Stadt und verfügt über wichtige Informationen über jeden Einwohner. Du spielst Aiden Pearce, einen brillanten Hacker und ehemaligen Gangster, dessen kriminelle Vergangenheit zu einer blutigen Familientragödie führte. Jetzt bist du auf der Jagd nach den Leuten, die deiner Familie Leid zugefügt haben, und du hast die Möglichkeit jeden in deinem Umfeld zu überwachen und zu hacken, indem du alles manipulierst, was mit dem Netzwerk der Stadt verbunden ist. Greife auf die allgegenwärtigen Überwachungskameras zu, lade persönliche Informationen herunter, um eine Zielperson zu finden, kontrolliere Ampeln und öffentliche Verkehrsmittel… und vieles mehr. Nutze die Stadt Chicago als deine ultimative Waffe und nimm auf deine persönliche Art Rache.</p><p><ul> <li><strong>Hacke die Stadt</strong> - <i>Watch Dogs</i> spielt in einer vollständig simulierten Stadt, in der du in Echtzeit die öffentlichen Verkehrsmittel mit deinem Smartphone kontrollieren kannst. Alles, was mit dem ctOS der Stadt verbunden ist, kann zu deiner Waffe werden.</li> <li><strong>Gerechtigkeit der Straße</strong> - In einer Stadt, in der auf Gewalt am besten mit Gewalt geantwortet wird, hast du die Fähigkeiten, um den Kampf auf die Straßen zu bringen. Insgesamt wirst du Zugriff auf ein Arsenal von über 30 traditionellen Waffen haben.</li> <li><strong>Volle Power</strong> - Setze dich hinter das Lenkrad von mehr als 65 Fahrzeugen, jedes ausgestattet mit Handling und Fahrphysik auf dem neusten Stand der Technik, und erkunde die riesige Stadt während du Missionen erfüllst.</li> <li> <strong>Alles unter Kontrolle</strong> - Die komplett neue Disrupt Engine, die eigenes für <i>Watch Dogs</i> programmiert wurde, nutzt fortschrittliche Technologien, um ein beeindruckende Grafik und eine unglaublich realistische Spielerfahrung zu bieten. </li> <li> <strong>Dynamische Navigation</strong> - <i>Watch Dogs</i> bietet dir nicht nur die Möglichkeit das ctOS zu deinem Vorteil zu nutzen, sondern auch deine Umgebung. Nimm Abkürzungen durch Gebäude oder klettere über die Dächer, um dein Ziel in einem realistischen Chicago zu erreichen. </li> </ul></p>";
				})
				.Alter("Prince of Persia", x =>
				{
					x.Name = "Prince of Persia \"Die vergessene Zeit\"";
					x.ShortDescription = "Mit Prince of Persia: Die Vergessene Zeit erscheint eine völlig neue Geschichte aus dem Prince of Persia-Universum. Auf den Spieler warten bisher unbekannte Charaktere und neuartige Kräfte, mit denen die Natur und die Zeit kontrolliert werden können. Die Handlung des Spiels ist zwischen Prince of Persia The Sands of Time und Prince of Persia Warrior Within angesiedelt.";
					x.FullDescription = "<p>Prince of Persia: Die Vergessene Zeit stellt das nächste Kapitel aus dem von Fans geliebten Sands of Time-Universum dar. Nach seinem Abenteuer in Azad reist der Prinz in das Königreich seines Bruders. Doch eine mächtige Armee belagert den königlichen Palast und droht diesen zu vernichten. Um das Königreich vor dem sicheren Untergang zu bewahren, entschließt sich der Prinz, die uralten Kräfte des Sands nutzen – der Beginn eines epischen Abenteuers. Der Prinz muss sich als Anführer beweisen und lernen, dass große Macht oft einen hohen Preis fordert.</p><p>Klassisches Gameplay mit neuen Elementen Prince of Persia: Die Vergessene Zeit handelt von den Ereignissen zwischen Prince of Persia The Sands of Time und Prince of Persia: Warrior Within und eröffnet den Fans ein völlig neues Kapitel der Prince of Persia-Reihe. Dadurch verleiht das Spiel dem Sands of Time-Universum noch mehr Tiefe. Den Prinzen erwarten epische Kämpfe gegen mehrere Feinde gleichzeitig sowie spektakuläre Akrobatikeinlagen in schwindelerregender Höhe und gewaltigen Spielumgebungen. Glücklicherweise verfügt er in über besondere Kräfte, durch die er die Natur und die Zeit kontrollieren kann, denn Prince of Persia: Die Vergessene Zeit wird seine außerordentlichen Talente fordern wie niemals zuvor.</p><p>Prince of Persia: Die Vergessene Zeit entführt den Spieler zu unvergesslichen Schauplätzen, die durch die fortschrittliche und preisgekrönte Anvil-Engine zum Leben erweckt werden. Während vor den Toren des Königreichs ein schrecklicher Krieg tobt, werden die Fähigkeiten des Prinzen wie noch nie zuvor auf die Probe gestellt. Das epische Abenteuer des Prinzen hält etliche atemberaubende Momente bereit, die dem Spieler während des gesamten Abenteuers die Sprache verschlagen werden.</p><p>Herrschaft über die Natur. Da der Prinz die Natur und die Zeit beeinflussen kann, hat er stets die volle Kontrolle über seine Umgebung und einen gewaltigen Vorteil gegenüber seinen Widersachern. Er wird feststellen, dass seine Macht über die Natur in Kombination mit der Fähigkeit, die Zeit zurückzudrehen, ganz besonders durchschlagende Effekte haben kann. Somit eröffnen sich neue Wege und Ansätze Gebiete zu überwinden oder Rätsel zu lösen. Abwechslung und Spannung erwartet den Spieler dabei während des gesamten Abenteuers.</p><p><ul><li><b>Klassisches Gameplay mit neuen Elementen:</b> Den Prinzen erwarten epische Kämpfe gegen mehrere Feinde gleichzeitig sowie spektakuläre Akrobatikeinlagen in schwindelerregender Höhe und gewaltigen Spielumgebungen.</li><li><b>Eine kinoreife Spielerfahrung:</b> Prince of Persia: Die Vergessene Zeit entführt den Spieler zu unvergesslichen Schauplätzen, die durch die fortschrittliche und preisgekrönte Anvil-Engine zum Leben erweckt werden.</li><li><b>Herrschaft über die Natur:</b> Da der Prinz die Natur und die Zeit beeinflussen kann, hat er stets die volle Kontrolle über seine Umgebung und einen gewaltigen Vorteil gegenüber seinen Widersachern.</li><li><b>Rückkehr der beliebten Serie:</b> Prince of Persia: Die Vergessene Zeit handelt von den Ereignissen zwischen Prince of Persia The Sands of Time und Prince of Persia: Warrior Within und eröffnet den Fans ein völlig neues Kapitel der Prince of Persia-Reihe.</li></ul></p>";
				})
				.Alter("Driver San Francisco", x =>
				{
					x.ShortDescription = "Gangsterboss Charles Jericho ist wieder auf freiem Fuß und wird zu einer enormen Bedrohung für ganz San Francisco. Nur ein Mann vermag ihn jetzt noch aufzuhalten. Er hat die Straßen hunderter Städte befahren und sein ganzes Leben damit verbracht, Verbrecher hinter Schloss und Riegel zu bringen. Nun gibt es kein Zurück mehr! Um Jericho zur Strecke zu bringen muss er alles riskieren und er weiß, dass es sich dabei um seine vielleicht letzte Fahrt handeln könnte. Sein Name ist John Tanner. Er ist der Driver.";
					x.FullDescription = "<p>Mit Ubisoft Reflections kehren die Entwickler des Originals zurück und erschaffen mit Driver San Francisco einen brandneuen Ableger der weltweit über 14 Millionen Mal verkauften Driver-Reihe. Der Spieler schlüpft in die Rolle von Detective John Tanner und wird in eine unerbittliche Jagd in den Straßen der weltberühmten Stadt an der Bucht verwickelt. Dank eines revolutionären Features wird es nun möglich, nahtlos zwischen hunderten lizensierten Wagen hin und her zu wechseln. Der Spieler bleibt so immer mitten in der Action. Mir seiner zeitlose Atmosphäre, einzigartigem Fahrverhalten und einer grunderneuerten Spielbarkeit wird Driver San Francisco zu einer klassischen und kinoreifen Verfolgungsjagd in einer offenen Spielwelt.</p><p><ul> <li><strong>Verfolgungsjagd pur:</strong><br>Zurück zu den Ursprüngen des cinematischen Fahrerlebnis von DRIVER: Weiche Radaufhängungen, lange Drifts, enge Kurven und High-Speed Verfolgungen im dichten Verkehr. Über 120 lizensierte Fahrzeuge stehen in der intensiven Hatz durch die Straßen San Franciscos zur Verfügung.<br><br></li> <li><strong>Eine unerbitterliche Hetzjagd:</strong><br>Auf Tanners von Rache erfüllter Jagd nach Jericho enthüllt der Spieler eine spannende Geschichte. Er verfolgt Tanners PS geladenen Überlebenskampf durch San Francisco bis zu dem Punkt, an dem es kein Zurück mehr gibt. <br><br></li> <li><strong>Shift:</strong><br>Während Tanner sich von der Folgen eines dramatischen Unfalls erholt, wird ihm bewusst, dass er eine neue Fähigkeit erlang hat: Shift. Sie ermöglicht es ihm nahtlos zwischen Fahrzeugen hin und her zu wechseln um deren Kontrolle zu übernehmen. Dies hat eine beispiellose Intensität, Vielfältigkeit und Freiheit zur Folge: Es wird möglich, in schnellere Vehikel zu wechseln, mit zivilen Fahrzeugen seine Kontrahenten auszuschalten oder in den Wagen des Gegner zu schlüpfen, um ihn ins Verderben zu stürzen.<br><br></li> <li><strong>Ein Spielplatz für Verfolgungsjagden:</strong><br>Ein mehr als 200km umfassendes Straßen-Netzwerk reicht bis über die Golden Gate Bridge und ist gespickt mit vielen Sehenswürdigkeiten von San Francisco. Der Spieler wechselt von einem zum nächsten Fahrzeug und taucht ab in das Leben verschiedenster Stadtbewohner. Eine schwindelerregende Vielzahl an Charakteren, alle mit einem ganz eigenen Blick auf eine Stadt im Ausnahmezustand.<br><br></li> <li><strong>Wahnwitziger Mehrspieler-Action:</strong><br>Neun mitreißende Online-Modi, lassen die Spieler dank der SHIFT-Funktion nahezu zu jeder Zeit an jedem beliebigen Ort auftauchen.&nbsp; Sowohl offline im Splitscreen als auch online setzt man zum Rammen, Abhängen und Überholen seines Freundes an.<br><br> </li> <li><strong>Und vieles mehr:</strong><br>Wie ein echter Regisseur kann der Spieler seine besten Stunts in packenden Filmen festhalten, bearbeiten und mit seinen Freunden teilen. Wer sein fahrerisches Können unter Beweis stellen will, kann sich in 20 Rennen und bei 80 Herausforderungen austoben. Über 60 bekannte Songs und natürlich das originale Driver-Thema sorgen während der Action auf heißem Asphalt für ebenso heiße Ohren.</li> </ul></p>";
				})
				.Alter("PlayStation 3 plus game cheaper", x =>
				{
					x.Name = "PlayStation 3 plus Spiel günstiger";
					x.ShortDescription = "Unser besonderes Angebot: PlayStation 3 plus ein Spiel Ihrer Wahl günstiger.";
					x.FullDescription = ps3FullDescription;
				});

				#endregion gaming

			}
            catch (Exception ex)
            {
				throw new SeedDataException("AlterProduct", ex);
            }
        }

        protected override void Alter(IList<ForumGroup> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
            .Alter("General", x =>
            {
                x.Name = "Allgemein";
            });
        }

        protected override void Alter(IList<Forum> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
            .Alter("New Products", x =>
            {
                x.Name = "Neue Produkte";
                x.Description = "Diskutieren Sie aktuelle oder neue Produkte";
            })
            .Alter("Packaging & Shipping", x =>
            {
                x.Name = "Verpackung & Versand";
                x.Description = "Haben Sie Fragen oder Anregungen zu Verpackung & Versand?";
            });
        }

        protected override void Alter(IList<Discount> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("Sample discount with coupon code", x =>
                {
                    x.Name = "Beispiel Rabatt mit Coupon-Code";
                })
                .Alter("20% order total' discount", x =>
                {
                    x.Name = "20% vom Gesamteinkauf";
                });
        }

        protected override void Alter(IList<DeliveryTime> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(0, x =>
                {
                    x.Name = "sofort lieferbar";
                })
                .Alter(1, x =>
                {
                    x.Name = "2-5 Werktage";
                })
                .Alter(2, x =>
                {
                    x.Name = "7 Werktage";
                });
        }

        protected override void Alter(IList<QuantityUnit> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
                .Alter(0, x =>
                {
                    x.Name = "Stück";
                    x.Description = "Stück";
                })
                .Alter(1, x =>
                {
                    x.Name = "Schachtel";
                    x.Description = "Schachtel";
                })
                .Alter(2, x =>
                {
                    x.Name = "Paket";
                    x.Description = "Paket";
                })
                .Alter(3, x =>
                {
                    x.Name = "Palette";
                    x.Description = "Palette";
                });
        }

		protected override void Alter(IList<Store> entities)
		{
			base.Alter(entities);

			entities.WithKey(x => x.DisplayOrder)
				.Alter(1, x =>
				{
					x.Name = "Mein Shop-Name";
					x.Url = "http://www.mein-shop.de/";
					x.Hosts = "mein-shop.de,www.mein-shop.de";
				});
		}

        protected override void Alter(IList<ProductTag> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Name)
                .Alter("gift", x =>
                {
                    x.Name = "Geschenk";
                })
                .Alter("compact", x =>
                {
                    x.Name = "kompakt";
                })
                .Alter("cooking", x =>
                {
                    x.Name = "Kochen";
                })
                .Alter("cars", x =>
                {
                    x.Name = "Autos";
                })
                .Alter("motorbikes", x =>
                {
                    x.Name = "Motorräder";
                })
                .Alter("computer", x =>
                {
                    x.Name = "Computer";
                })
                .Alter("notebook", x =>
                {
                    x.Name = "Notebook";
                })
                .Alter("download", x =>
                {
                    x.Name = "Download";
                })
                .Alter("watches", x =>
                {
                    x.Name = "Uhren";
                })
                .Alter("book", x =>
                {
                    x.Name = "Buch";
                });
        }

        protected override void Alter(IList<EmailAccount> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.DisplayName)
            .Alter("General contact", x =>
            {
                x.DisplayName = "Kontakt";
                x.Email = "kontakt@meineshopurl.de";
                x.Host = "localhost";
            })
            .Alter("Sales representative", x =>
            {
                x.DisplayName = "Vertrieb";
                x.Email = "vertrieb@meineshopurl.de";
                x.Host = "localhost";
            })
            .Alter("Customer support", x =>
            {
                x.DisplayName = "Kundendienst / Support";
                x.Email = "kundendienst@meineshopurl.de";
                x.Host = "localhost";
            })
            ;
        }

        protected override void Alter(IList<BlogPost> entities)
        {
            base.Alter(entities);

            entities.WithKey(x => x.Title)
            .Alter("Online Discount Coupons", x =>
            {
                x.Title = "Online Rabatt Coupon";
                x.Body = "<p><p>Sparen Sie mit unseren Online-Coupons bares Geld!</p></p>";
                x.Tags = "Geld, Rabatt, Coupon";
                x.Language = base.DbContext.Set<Language>().FirstOrDefault();
            })

            .Alter("Customer Service - Client Service", x =>
            {
                x.Title = "Kundendienst - Unser Service";
                x.Body = "<p>Bei uns wird Service GROSS geschrieben! Auch nach Ihrem Einkauf bei uns können Sie mit uns Rechnen!<br></p>";
                x.Tags = "Shopsystem, SmartStore.NET, asp.net, sample tag, Service";
				x.Language = base.DbContext.Set<Language>().FirstOrDefault();
            });
            

        }

        protected override void Alter(IList<NewsItem> entities)
        {
			var defaultLanguage = base.DbContext.Set<Language>().FirstOrDefault();
            base.Alter(entities);

            entities.WithKey(x => x.MetaTitle)
            .Alter("SmartStore.NET new release!", x =>
            {
                x.Title = "SmartStore.NET - das clevere Shopsystem!";
                x.Short = "SmartStore.NET ist die neue dynamische E-Commerce Lösung von SmartStore. SmartStore.NET bietet alle Funktionen und Möglichkeiten, um schnell und einfach einen leistungsfähigen und funktional kompletten Online-Shop zu erstellen.";
                x.Full = "<p>Mit SmartStore.NET haben Sie alles im Griff. Verwalten Sie Ihren Lagerbestand, Ihre Aufträge und alle kundenspezifischen Funktionen, wie kundenindividuelle Rabatte, Gutscheine oder Zugriffsrechte für spezielle Kundengruppen.  </p>  <p>  Durch sprechende URL's und eine durchdachte HTML-Struktur ist SmartStore.NET perfekt für Suchmaschinen optimiert.  </p>  <p>  SmartStore.NET erkennt automatisch, ob Ihre Shopbesucher mit einem mobilen Endgerät auf Ihren Shop zugreifen und zeigt den Shop in einer Ansicht, die für geringe Auflösungen optimiert ist.  </p>  <p>  Steigern sie Ihren Umsatz und animieren Sie mehr Kunden zum Kauf mit Produkt-Rezensionen und -Bewertungen.  </p>  <p>  SmartStore.NET wird bereits mit Sprachpaketen für Deutsch und Englisch ausgeliefert und unterstützt die Verwaltung unendlich vieler weiterer Sprachen.  </p>  <p>  Starten Sie sofort durch!<br>  Importieren Sie Ihren SmartStore.biz Shop mit nur einem Klick in SmartStore.NET.</p>";
                x.Language = defaultLanguage;
            })

            .Alter("New online store is open!", x =>
            {
                x.Title = "Kundendienst - Service";
                x.Short = "Hier finden Sie Antworten auf Fragen zu unserem Onlineshop";
                x.Full = "<p> Warum ein Benutzerkonto?<br> Wenn Sie in unserem Online-Shop bestellen wollen, können Sie gerne als Gast, ohne ein Benutzerkonto zu erstellen, oder als registrierter Kunde mmit einem Benutzerkonto bestellen. Mit dem Benutzerkonto gestaltet sich der Einkauf im Online-Shop bequemer und zusätzlich haben Sie eine Übersicht der hinterlegten Daten und Ihrer Bestellungen. </p> <p> Welche Funnktionen beinhaltet das Benutzerkonto?<br> In Ihrem Benutzerkonto haben Sie Zugriff auf alle wichtigen, persönlichen Daten und können diese bei Bedarf entsprechend bearbeiten: Bearbeiten der Rechnungsadresse Bearbeiten, Löschen und Neuanlage von Lieferadressen Ändern des Passwortes Aktuelle Bestellung und Übersicht über Ihre Bestellhistorie Merkliste <br> Darüberhinaus, können Sie im Forum teilnehmen, unsere Blogs und News kommentieren. </p>";
                x.Language = defaultLanguage;
            });
        }

        protected override void Alter(IList<Poll> entities)
        {
			var defaultLanguage = base.DbContext.Set<Language>().FirstOrDefault();
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
            .Alter(1, x =>
            {
                x.Language = defaultLanguage;
                x.Name = "Wie gefällt Ihnen der Shop?";
                x.PollAnswers.Clear();
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Ausgezeichnet",
                    DisplayOrder = 1,
                });
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Gut",
                    DisplayOrder = 2,
                });
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Geht so",
                    DisplayOrder = 3,
                });
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Schlecht",
                    DisplayOrder = 4,
                });

            })

            .Alter(2, x =>
            {
                x.Language = defaultLanguage;
                x.Name = "Wie oft kaufen Sie Online ein?";
                x.PollAnswers.Clear();
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Täglich",
                    DisplayOrder = 1,
                });
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Wöchentlich",
                    DisplayOrder = 2,
                });
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Alle zwei Wochen",
                    DisplayOrder = 3,
                });
                x.PollAnswers.Add(new PollAnswer()
                {
                    Name = "Einmal im Monat",
                    DisplayOrder = 4,
                });
            });
        }

        protected override void Alter(IList<PollAnswer> entities)
        {
            var defaultLanguage = base.DbContext.Set<DeliveryTime>().FirstOrDefault();
            base.Alter(entities);

            entities.WithKey(x => x.DisplayOrder)
            .Alter(1, x =>
            {
                x.Name = "Ausgezeichnet";
            })
            .Alter(2, x =>
            {
                x.Name = "Gut";
            })
            .Alter(3, x =>
            {
                x.Name = "Geht so";
            })
            .Alter(4, x =>
            {
                x.Name = "Schlecht";
            })
            .Alter(5, x =>
            {
                x.Name = "Täglich";
            })
            .Alter(6, x =>
            {
                x.Name = "Wöchentlich";
            })
            .Alter(7, x =>
            {
                x.Name = "Alle zwei Wochen";
            })
            .Alter(8, x =>
            {
                x.Name = "Einmal im Monat";
            });
        }
    }
}
