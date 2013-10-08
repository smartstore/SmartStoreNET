--upgrade scripts for smartstore.net (only specific parts)

--new locale resources
DECLARE @resources xml
--a resource will be deleted if its value is empty   
SET @resources='
<Language>
  <LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowConfirmOrderLegalHint">
    <Value>Show legal hints in order summary on the confirm order page</Value>
	<Value lang="de">Rechtliche Hinweise in der Warenkorbübersicht auf der Bestellabschlußseite anzeigen</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.ShoppingCart.ShowConfirmOrderLegalHint.Hint">
    <Value>Determines whether to show hints in order summary on the confirm order page. This text can be altered in the language resources.</Value>
	<Value lang="de">Bestimmt, ob rechtliche Hinweise in der Warenkorbübersicht auf der Bestellabschlußseite angezeigt werden. Dieser Text kann in den Sprachresourcen geändert werden.</Value>
  </LocaleResource>

  <LocaleResource Name="OrderSummary.ConfirmOrderLegalHint">
    <Value>For deliveries to a non-EU state additional costs in regard to customs, fees and taxes can arise.</Value>
	<Value lang="de">Bei Lieferungen in das Nicht-EU-Ausland können zusätzlich Zölle, Steuern und Gebühren anfallen.</Value>
  </LocaleResource>

  <LocaleResource Name="Common.Submit">
    <Value>Submit</Value>
	<Value lang="de">Absenden</Value>
  </LocaleResource>
  
  <LocaleResource Name="Common.Send">
    <Value>Send</Value>
	<Value lang="de">Senden</Value>
  </LocaleResource>
  
  <LocaleResource Name="Common.Question">
    <Value>Question</Value>
	<Value lang="de">Frage</Value>
  </LocaleResource>
  
  <LocaleResource Name="Common.Error.SendMail">
    <Value>Error while sending the email. Please try again later.</Value>
	<Value lang="de">Fehler beim Versenden der Email. Bitte versuchen Sie es später erneut.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Account.Fields.FullName">
    <Value>Name</Value>
	<Value lang="de">Name</Value>
  </LocaleResource>
  
  <LocaleResource Name="Account.Fields.FullName.Required">
    <Value>Name is required</Value>
	<Value lang="de">Name wird benötigt</Value>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion">
    <Value>Question about product?</Value>
	<Value lang="de">Fragen zum Artikel?</Value>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Title">
    <Value>Question about product</Value>
	<Value lang="de">Frage zum Artikel</Value>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Question.Required">
    <Value>Question is required</Value>
	<Value lang="de">Frage ist erforderlich</Value>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Question.Text">
    <Value>I have following questions concerning the product {0}:</Value>
	<Value lang="de">Ich habe folgende Fragen zum Artikel {0}:</Value>
  </LocaleResource>
  
  <LocaleResource Name="Products.AskQuestion.Sent">
    <Value>Thank you. Your inquiry has been sent successfully.</Value>
	<Value lang="de">Vielen Dank. Ihre Anfage wurde erfolgreich gesendet.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.AskQuestionEnabled">
    <Value>''Ask question'' enabled</Value>
	<Value lang="de">''Produktanfragen'' ermöglichen</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.AskQuestionEnabled.Hint">
    <Value>Check to allow customers to send an inquiry concerning a product</Value>
	<Value lang="de">Legt fest, ob Kunden eine Anfrage zu einem Produkt stellen können</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnAskQuestionPage">
    <Value>Show on ''ask question'' page</Value>
	<Value lang="de">Auf der Seite ''Produktanfrage'' zeigen</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnAskQuestionPage.Hint">
    <Value>Check to show CAPTCHA on ''ask question'' page</Value>
	<Value lang="de">Legt fest, ob ein CAPTCHA auf der ''Produktanfrage''-Seite angezeigt werden soll.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.DefaultViewMode">
    <Value>Default view mode</Value>
	<Value lang="de">Standard Listendarstellung</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.DefaultViewMode.Hint">
    <Value>Specifies how product lists should be displayed by default. The customer can also change the appearance manually.</Value>
	<Value lang="de">Legt fest, wie Produktlisten standardmäßig dargestellt werden sollen. Der Kunde kann die Darstellung im Shop ändern.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Common.List">
    <Value>List</Value>
	<Value lang="de">Liste</Value>
  </LocaleResource>
  
  <LocaleResource Name="Common.Grid">
    <Value>Grid</Value>
	<Value lang="de">Raster</Value>
  </LocaleResource>
  
  <LocaleResource Name="ThemeVar.Alpha.SliderBgSlide">
    <Value>Background slide behaviour</Value>
	<Value lang="de">Hintergrund slide Verhalten</Value>
  </LocaleResource>

  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowColorSquaresInLists">
    <Value>Show color squares in product lists</Value>
	<Value lang="de">Zeige Farbvarianten in Produktlisten</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowColorSquaresInLists.Hint">
    <Value>Specifies whether the colors of the first color type attribute should be displayed in product lists</Value>
	<Value lang="de">Legt fest, ob die Farben des ersten Farbattributes auch in Produktlisten angezeigt werden sollen</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateSuccess">
    <Value>The language resources has been successfully updated.</Value>
	<Value lang="de">Die Sprachressourcen wurden erfogreich aktualisiert.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateFailure">
    <Value>Failed to update language resources.</Value>
	<Value lang="de">Das Aktualisieren der Sprachressourcen ist fehlgeschlagen.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.Update">
    <Value>Update resources</Value>
	<Value lang="de">Ressourcen aktualisieren</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateConfirm">
    <Value>Do you like to update the language resources for this plugin?</Value>
	<Value lang="de">Möchten Sie die Sprachressourcen für dieses Plugin aktualisieren?</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.Resources.UpdateProgress">
    <Value>Refreshing language resources...</Value>
	<Value lang="de">Aktualisiere Sprachressourcen...</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Common.General">
	<Value>General</Value>
	<Value lang="de">Allgemein</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Stores">
	<Value>Stores</Value>
	<Value lang="de">Shops</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Info">
	<Value>Info</Value>
	<Value lang="de">Info</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.StoresAll">
	<Value>All stores</Value>
	<Value lang="de">Alle Shops</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Common.Store.SearchFor">
	<Value>Store</Value>
	<Value lang="de">Shop</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.SearchFor.Hint">
	<Value>Search by a specific store.</Value>
	<Value lang="de">Nach bestimmten Shop suchen.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.LimitedTo">
	<Value>Limited to stores</Value>
	<Value lang="de">Auf Shops begrenzt</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.LimitedTo.Hint">
	<Value>Determines whether the item is available only at certain stores.</Value>
	<Value lang="de">Legt fest, ob der Eintrag nur für bestimmte Shops verfügbar ist.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.AvailableFor">
	<Value>Stores</Value>
	<Value lang="de">Shops</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.Store.AvailableFor.Hint">
	<Value>Select stores for which the item will be shown.</Value>
	<Value lang="de">Bitte Shops auswählen, für die der Eintrag angezeigt werden soll.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Common.On">
    <Value>On</Value>
	<Value lang="de">An</Value>
  </LocaleResource>
  <LocaleResource Name="Common.Off">
    <Value>Off</Value>
	<Value lang="de">Aus</Value>
  </LocaleResource>
  
  <LocaleResource Name="RewardPoints.Message.RegisteredAsCustomer">
    <Value>Registered as customer</Value>
	<Value lang="de">Als Kunde registriert</Value>
  </LocaleResource>  

  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.SmallQuantityThreshold">
    <Value>Threshold for small quantities</Value>
	<Value lang="de">Mindermenge bis Bestellwert</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.SmallQuantityThreshold.Hint">
    <Value>Subtotal up to which a "small quantity surcharge" should be added. The surcharge will be ignored if no shipping fee is applied. Use "0" if no fee will be charged.</Value>
	<Value lang="de">Warenwert, bis zu dem ein Mindermengenzuschlag erhoben werden soll. Der Zuschlag wird ignoriert, wenn keine Versandkosten anfallen. Verwenden Sie "0", wenn kein Zuschlag erhoben werden soll.</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.SmallQuantitySurcharge">
    <Value>Surcharge for small quantities</Value>
	<Value lang="de">Mindermengenzuschlag</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.BaseCharge">
    <Value>Base fee</Value>
	<Value lang="de">Basisgebühr</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.MaxCharge">
    <Value>Max. fee</Value>
	<Value lang="de">Max. Gebühr</Value>
  </LocaleResource>
  <LocaleResource Name="Plugins.Shipping.ByTotal.Fields.MaxCharge.Hint">
    <Value>An amount that the calculated shipping costs may not exceed.</Value>
	<Value lang="de">Ein Betrag, den die berechneten Versandkosten nicht übersteigen dürfen.</Value>
  </LocaleResource>   
  
  <LocaleResource Name="ErrorPage.Title">
    <Value>We''re sorry, an internal error occurred that prevents the request to complete.</Value>
	<Value lang="de">Leider ist ein interner Fehler aufgetreten.</Value>
  </LocaleResource>
  <LocaleResource Name="ErrorPage.Body">
    <Value>Our supporting staff has been notified with this error and will address this issue shortly. We profusely apologize for the <strong>inconvenience</strong> and for any damage this may cause. You might want to try the same action at later time.</Value>
	<Value lang="de">Unser Support-Team wurde über diesen Fehler informiert und wird sich in Kürze um die Behebung kümmern. Wir entschuldigen uns für diese Unannehmlichkeit! Bitte probieren Sie den Vorgang zu einem späteren Zeitpunkt erneut.</Value>
  </LocaleResource>
  
  <LocaleResource Name="AddProductToCompareList.CouldNotBeAdded">
    <Value>Product could not be added.</Value>
	<Value lang="de">Produkt konnte nicht hinzugefügt werden.</Value>
  </LocaleResource>
  <LocaleResource Name="AddProductToCompareList.ProductWasAdded">
    <Value>The product ''{0}'' was added to the compare list.</Value>
	<Value lang="de">Das Produkt ''{0}'' wurde der Vergleichsliste hinzugefügt.</Value>
  </LocaleResource>
  <LocaleResource Name="AddProductToCompareList.CouldNotBeRemoved">
    <Value>Product could not be removed.</Value>
	<Value lang="de">Produkt konnte nicht entfernt werden.</Value>
  </LocaleResource>
  <LocaleResource Name="AddProductToCompareList.ProductWasDeleted">
    <Value>The product ''{0}'' was removed from the compare list.</Value>
	<Value lang="de">Das Produkt ''{0}'' wurde von der Vergleichsliste entfernt.</Value>
  </LocaleResource>

  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductDetail">
    <Value>Show delivery times</Value>
	<Value lang="de">Zeige Lieferzeiten</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductDetail.Hint">
    <Value>Determines whether delivery times should be display on product detail page.</Value>
	<Value lang="de">Bestimmt ob Lieferzeitinformationen auf der Produktdetailseite angezeigt werden.</Value>
  </LocaleResource>
  

  <LocaleResource Name="Jquery.Validate.Email">
    <Value>Please enter a valid email address.</Value>
	<Value lang="de">Bitte geben Sie eine gültige E-Mail-Adresse ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Required">
    <Value>This field is required.</Value>
	<Value lang="de">Diese Angabe ist erforderlich.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Remote">
    <Value>Please fix this field.</Value>
	<Value lang="de">Bitte korrigieren Sie dieses Feld.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Url">
    <Value>Please enter a valid URL.</Value>
	<Value lang="de">Bitte geben Sie eine gültige URL ein.</Value>
  </LocaleResource>
 <LocaleResource Name="Jquery.Validate.Date">
    <Value>Please enter a valid date.</Value>
	<Value lang="de">Bitte geben Sie ein gültiges Datum ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.DateISO">
    <Value>Please enter a valid date (ISO).</Value>
	<Value lang="de">Bitte geben Sie ein gültiges Datum (nach ISO) ein.</Value>
  </LocaleResource>
 <LocaleResource Name="Jquery.Validate.Number">
    <Value>Please enter a valid number.</Value>
	<Value lang="de">Bitte geben Sie eine gültige Nummer ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Digits">
    <Value>Please enter only digits.</Value>
	<Value lang="de">Bitte geben Sie nur Ziffern ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Creditcard">
    <Value>Please enter a valid credit card number.</Value>
	<Value lang="de">Bitte geben Sie eine gültige Kreditkartennummer ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.EqualTo">
    <Value>Please enter the same value again.</Value>
	<Value lang="de">Wiederholen Sie bitte die Eingabe.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Maxlength">
    <Value>Please enter no more than {0} characters.</Value>
	<Value lang="de">Bitte geben Sie nicht mehr als {0} Zeichen ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Minlength">
    <Value>Please enter at least {0} characters.</Value>
	<Value lang="de">Bitte geben Sie mindestens {0} Zeichen ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Rangelength">
    <Value>Please enter a value between {0} and {1} characters long.</Value>
	<Value lang="de">Die Länge der Eingabe darf minimal {0} und maximal {1} Zeichen lang sein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Range">
    <Value>Please enter a value between {0} and {1}.</Value>
	<Value lang="de">Bitte geben Sie einen Wert zwischen {0} und {1} ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Max">
    <Value>Please enter a value less than or equal to {0}.</Value>
	<Value lang="de">Bitte geben Sie einen Wert kleiner oder gleich {0} ein.</Value>
  </LocaleResource>
  <LocaleResource Name="Jquery.Validate.Min">
    <Value>Please enter a value greater than or equal to {0}.</Value>
	<Value lang="de">Bitte geben Sie einen Wert größer oder gleich {0} ein.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.RenderAsWidget">
    <Value>Render as HTML widget</Value>
	<Value lang="de">Als HTML Widget darstellen</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.RenderAsWidget.Hint">
    <Value>Specifies whether the content should be displayed as an HTML widget.</Value>
	<Value lang="de">Legt fest, ob der Content inline als HTML Widget dargestellt werden soll</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.WidgetZone">
    <Value>Widget zone</Value>
	<Value lang="de">Widget Zone</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.WidgetZone.Hint">
    <Value>One or more widget zones in which the widget should be rendered. Note: a theme defines clearly more zones as offered here. Search the view files for "@Html.Widget(...)" to locate all zones and determine their corresponding names.</Value>
	<Value lang="de">Ein oder mehrere Widget Zonen, in denen der Content dargestellt werden soll. Hinweis: ein Theme definiert deutlich mehr Zonen als hier angeboten. Suchen Sie die View-Dateien nach "@Html.Widget(...)" ab, um alle verfügbaren Zonen-Namen zu ermitteln.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.WidgetShowTitle">
    <Value>Show title</Value>
	<Value lang="de">Titel anzeigen</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.WidgetShowTitle.Hint">
    <Value>Specifies whether the title should be displayed as the widget header.</Value>
	<Value lang="de">Legt fest, ob der Titel als Überschrift dargestellt werden soll.</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.WidgetBordered">
    <Value>Render bordered</Value>
	<Value lang="de">Widget umrahmen</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.Priority">
    <Value>Priority</Value>
	<Value lang="de">Sortierung</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement.Topics.Fields.Priority.Hint">
    <Value>Specifies the sort order of a widget within a zone.</Value>
	<Value lang="de">Legt die Sortierreihenfolge des Widgets innerhalb einer Zone fest.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Catalog.Categories.Fields.Alias">
    <Value>Alias</Value>
	<Value lang="de">Alias</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Categories.Fields.Alias.Hint">
    <Value>An optional, language-neutral reference name for internal use</Value>
	<Value lang="de">Ein optionaler, sprachneutraler Referenzwert für interne Zwecke</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Categories.List.SearchAlias">
    <Value>Alias</Value>
	<Value lang="de">Alias</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Categories.List.SearchAlias.Hint">
    <Value>The alias to be filtered</Value>
	<Value lang="de">Der Alias, nach dem gefiltert werden soll</Value>
  </LocaleResource>  
  
  <LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MUBase">
    <Value>Basic unit</Value>
	<Value lang="de">Grundeinheit (PAngV)</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Catalog.Products.Variants.Fields.MUAmount">
    <Value>Amount</Value>
	<Value lang="de">Menge</Value>
  </LocaleResource>

  <LocaleResource Name="Admin.Orders.List.OrderGuid">
	<Value lang="de"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.List.OrderGuid">
	<Value lang="de">Auftrags-GUID</Value>
  </LocaleResource>

  <LocaleResource Name="Admin.Orders.List.OrderNumber">
    <Value>Order Number</Value>
	<Value lang="de">Auftragsnummer</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.List.OrderNumber.Hint">
    <Value>Search by order number or part of order number. Leave empty to load all orders.</Value>
	<Value lang="de">Suche über die Auftragsnummer oder Teile davon. Freilassen, um alle Aufträge zu laden.</Value>
  </LocaleResource>
    <LocaleResource Name="Admin.Orders.Fields.OrderNumber">
    <Value>Order Number</Value>
	<Value lang="de">Auftragsnummer</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Orders.Fields.OrderNumber.Hint">
    <Value>The (formatted) order number</Value>
	<Value lang="de">Die formattierte Auftragsnummer.</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.ContentManagement">
    <Value lang="en"></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.ContentManagement">
    <Value lang="en">CMS</Value>
  </LocaleResource>

  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.DetectBrowserUserLanguage">
    <Value>Detect browser user language</Value>
	<Value lang="de">Browsersprache erkennen</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.DetectBrowserUserLanguage.Hint">
    <Value>Specifies whether the browser language of the visitor should be detected and assigned on his first visit (when inactive, the default store language will be assigned)</Value>
	<Value lang="de">Legt fest, ob beim Erstbesuch die Browsersprache des Besuchers automatisch erkannt und zugewiesen werden soll (wenn inaktiv, wird die Standardsprache des Stores zugewiesen)</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.DefaultLanguageRedirectBehaviour">
    <Value>Default language redirect behaviour</Value>
	<Value lang="de">Verhalten bei Standardsprache</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.DefaultLanguageRedirectBehaviour.Hint">
    <Value>Specifies the redirect behavior when a page is requested in the default language (the default language is the first active store language)</Value>
	<Value lang="de">Legt das Redirect-Verhalten fest, wenn eine Seite in der Standardsprache angefordert wird (die Standardsprache ist die erste aktive Sprache eines Stores).</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.InvalidLanguageRedirectBehaviour">
    <Value>Invalid language redirect behaviour</Value>
	<Value lang="de">Verhalten bei ungültiger Sprache</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Settings.GeneralCommon.InvalidLanguageRedirectBehaviour.Hint">
    <Value>Specifies the redirect behavior when the given SEO code is invalid or the corresponding language does not exist or is unpublished.</Value>
	<Value lang="de">Legt das Redirect-Verhalten fest, wenn eine Seite mit einem ungültigen bzw. inaktiven SEO code (Sprachkürzel) angefordert wird.</Value>
  </LocaleResource>

  <LocaleResource Name="Enums.SmartStore.Core.Domain.Localization.DefaultLanguageRedirectBehaviour.PrependSeoCodeAndRedirect">
    <Value>Prepend SEO code to url and redirect</Value>
	<Value lang="de">Der URL SEO Code voranstellen und weiterleiten</Value>
  </LocaleResource>
  <LocaleResource Name="Enums.SmartStore.Core.Domain.Localization.DefaultLanguageRedirectBehaviour.DoNoRedirect">
    <Value>Do not redirect</Value>
	<Value lang="de">Nicht weiterleiten</Value>
  </LocaleResource>
  <LocaleResource Name="Enums.SmartStore.Core.Domain.Localization.DefaultLanguageRedirectBehaviour.StripSeoCode">
    <Value>Strip SEO code if specified (recommended)</Value>
	<Value lang="de">SEO Code entfernen wenn angegeben (empfohlen)</Value>
  </LocaleResource>
  <LocaleResource Name="Enums.SmartStore.Core.Domain.Localization.InvalidLanguageRedirectBehaviour.Tolerate">
    <Value>Tolerate</Value>
	<Value lang="de">Tolerieren</Value>
  </LocaleResource>
  <LocaleResource Name="Enums.SmartStore.Core.Domain.Localization.InvalidLanguageRedirectBehaviour.FallbackToWorkingLanguage">
    <Value>Fallback to working language</Value>
	<Value lang="de">Zur aktiven Sprache bzw. Standardsprache umleiten</Value>
  </LocaleResource>
  <LocaleResource Name="Enums.SmartStore.Core.Domain.Localization.InvalidLanguageRedirectBehaviour.ReturnHttp404">
    <Value>Return HTTP 404 (page not found) (recommended)</Value>
	<Value lang="de">HTTP 404 zurückgeben (Seite nicht gefunden) (empfohlen)</Value>
  </LocaleResource>

  <LocaleResource Name="Plugins.KnownGroup.Developer">
    <Value>Developer</Value>
	<Value lang="de">Entwickler</Value>
  </LocaleResource>

  <LocaleResource Name="Admin.Common.GenericAttributes">
    <Value>Attributes</Value>
	<Value lang="de">Attribute</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.GenericAttributes.NameAlreadyExists">
    <Value>An attribute with the name "{0}" already exists</Value>
	<Value lang="de">Ein Attribut mit dem Namen "{0}" existiert bereits</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.GenericAttributes.Fields.Name">
    <Value>Name</Value>
	<Value lang="de">Name</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.GenericAttributes.Fields.Name.Required">
    <Value>Please provide an attribute name</Value>
	<Value lang="de">Bitte geben Sie einen Attribut-Namen an</Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Common.GenericAttributes.Fields.Value">
    <Value>Value</Value>
	<Value lang="de">Wert</Value>
  </LocaleResource>
  
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.CurrencyExchange">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.DiscountRequirement">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.ExternalAuth">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.Import">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.Misc">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.Mobile">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.Payment">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.PromotionFeed">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.Shipping">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.Tax">
    <Value></Value>
  </LocaleResource>
  <LocaleResource Name="Admin.Configuration.Plugins.KnownGroup.Widget">
    <Value></Value>
  </LocaleResource>
  
	<LocaleResource Name="Plugins.KnownGroup.Admin">
		<Value>Administration</Value>
		<Value lang="de">Administration</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Analytics">
		<Value>Analytics &amp; Stats</Value>
		<Value lang="de">Analyse &amp; Statistiken</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Api">
		<Value>API</Value>
		<Value lang="de">API</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.CMS">
		<Value>Content Management</Value>
		<Value lang="de">Content Management</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.CurrencyExchange">
		<Value>Exchange rate providers</Value>
		<Value lang="de">Wechselkursdienst</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.DiscountRequirement">
		<Value>Discount requirements</Value>
		<Value lang="de">Rabattkonditionen</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.ExternalAuth">
		<Value>Authentication</Value>
		<Value lang="de">Authentifizierung</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Import">
		<Value>Import</Value>
		<Value lang="de">Import</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Marketing">
		<Value>Marketing</Value>
		<Value lang="de">Marketing</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Misc">
		<Value>Miscellaneous</Value>
		<Value lang="de">Sonstige</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Mobile">
		<Value>Mobile</Value>
		<Value lang="de">Mobile</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Payment">
		<Value>Payment &amp; Gateways</Value>
		<Value lang="de">Zahlungsschnittstellen</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.PromotionFeed">
		<Value>Promotion Feeds</Value>
		<Value lang="de">Promotion Feeds</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Security">
		<Value>Security</Value>
		<Value lang="de">Sicherheit</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Shipping">
		<Value>Shipping &amp; Logistics</Value>
		<Value lang="de">Versand &amp; Logistik</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.SEO">
		<Value>SEO</Value>
		<Value lang="de">SEO</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Social">
		<Value>Social</Value>
		<Value lang="de">Social</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Tax">
		<Value>Tax providers</Value>
		<Value lang="de">Steuern</Value>
	</LocaleResource>
	<LocaleResource Name="Plugins.KnownGroup.Widget">
		<Value>Widgets</Value>
		<Value lang="de">Widgets</Value>
	</LocaleResource>

</Language>
'

DECLARE @forceResUpdate bit
SET @forceResUpdate = 0

CREATE TABLE #ResTmp
(
	[Name] [nvarchar](200) NOT NULL, [Lang] [nvarchar](2) NULL DEFAULT N'', [Value] [nvarchar](max) NOT NULL
)

--flatten the 'Value' nodes into temp table
INSERT INTO #ResTmp (Name, Lang, Value)
SELECT
	R.rref.value('@Name', 'nvarchar(200)'),
	COALESCE(V.vref.value('@lang', 'nvarchar(2)'), ''),
	COALESCE(V.vref.value('text()[1]', 'nvarchar(MAX)'), '')
FROM
	@resources.nodes('//Language/LocaleResource') AS R(rref)
CROSS APPLY
	R.rref.nodes('Value') AS V(vref)


--do it for each existing language
DECLARE @ExistingLanguageID int
DECLARE @ExistingSeoCode nvarchar(2)
DECLARE cur_existinglanguage CURSOR FOR
SELECT [ID], [UniqueSeoCode] AS Lang FROM [Language]
OPEN cur_existinglanguage
FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID, @ExistingSeoCode
WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE @Name nvarchar(200)
	DECLARE @Lang nvarchar(2)
	DECLARE @Value nvarchar(MAX)
	DECLARE cur_localeresource CURSOR FOR
	SELECT Name, Lang, Value FROM #ResTmp WHERE Lang = @ExistingSeoCode OR Lang = '' ORDER BY Lang, Name
	OPEN cur_localeresource
	FETCH NEXT FROM cur_localeresource INTO @Name, @Lang, @Value
	WHILE @@FETCH_STATUS = 0
	BEGIN

		IF (EXISTS (SELECT 1 FROM [LocaleStringResource] WHERE LanguageID=@ExistingLanguageID AND ResourceName=@Name))
		BEGIN
			UPDATE [LocaleStringResource]
			SET [ResourceValue]=@Value
			WHERE LanguageID=@ExistingLanguageID AND ResourceName=@Name AND (@forceResUpdate=1 OR (IsTouched is null OR IsTouched = 0))
		END
		ELSE 
		BEGIN
			INSERT INTO [LocaleStringResource] (LanguageId, ResourceName, ResourceValue) VALUES (@ExistingLanguageID, @Name, @Value)
		END
		
		IF (@Value is null or @Value = '')
		BEGIN
			DELETE [LocaleStringResource] WHERE LanguageID=@ExistingLanguageID AND ResourceName=@Name
		END
	
		FETCH NEXT FROM cur_localeresource INTO @Name, @Lang, @Value
	END
	CLOSE cur_localeresource
	DEALLOCATE cur_localeresource


	--fetch next language identifier
	FETCH NEXT FROM cur_existinglanguage INTO @ExistingLanguageID, @ExistingSeoCode
END
CLOSE cur_existinglanguage
DEALLOCATE cur_existinglanguage

DROP TABLE #ResTmp
GO

-- CatalogSettings.AskQuestionEnabled
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'catalogsettings.askquestionenabled')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'catalogsettings.askquestionenabled', N'True', 0)
END
GO

-- CaptchaSettings.ShowOnAskQuestionPage
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'captchasettings.showonaskquestionpage')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'captchasettings.showonaskquestionpage', N'False', 0)
END
GO

-- CatalogSettings.ShowColorSquaresInLists
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'CatalogSettings.ShowColorSquaresInLists')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'CatalogSettings.ShowColorSquaresInLists', N'True', 0)
END
GO

-- CatalogSettings.EnableHtmlTextCollapser
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'CatalogSettings.EnableHtmlTextCollapser')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'CatalogSettings.EnableHtmlTextCollapser', N'False', 0)
END
GO

-- CatalogSettings.HtmlTextCollapsedHeight
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'CatalogSettings.HtmlTextCollapsedHeight')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'CatalogSettings.HtmlTextCollapsedHeight', N'260', 0)
END
GO 

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedPaymentMethods')
BEGIN
	ALTER TABLE [DiscountRequirement] ADD [RestrictedPaymentMethods] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedShippingOptions')
BEGIN
	ALTER TABLE [DiscountRequirement] ADD [RestrictedShippingOptions] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[DiscountRequirement]') and NAME='RestrictedToStoreId')
BEGIN
	ALTER TABLE [DiscountRequirement] ADD [RestrictedToStoreId] int NULL
END
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[ShippingByTotal]') and OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	-- ShippingByTotalSettings.SmallQuantityThreshold
	IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'ShippingByTotalSettings.SmallQuantityThreshold')
	BEGIN
		INSERT [Setting] ([Name], [Value], [StoreId])
		VALUES (N'ShippingByTotalSettings.SmallQuantityThreshold', N'0', 0)
	END

	-- ShippingByTotalSettings.SmallQuantityThreshold
	IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [name] = N'ShippingByTotalSettings.SmallQuantitySurcharge')
	BEGIN
		INSERT [Setting] ([Name], [Value], [StoreId])
		VALUES (N'ShippingByTotalSettings.SmallQuantitySurcharge', N'0', 0)
	END

	-- Add ShippingByTotalRecord.BaseCharge
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShippingByTotal]') and NAME='BaseCharge')
	BEGIN
		ALTER TABLE ShippingByTotal ADD [BaseCharge] decimal(18,2) NOT NULL DEFAULT 0
	END

	-- Add ShippingByTotalRecord.MaxCharge
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[ShippingByTotal]') and NAME='MaxCharge')
	BEGIN
		ALTER TABLE ShippingByTotal ADD MaxCharge decimal(18,2) NULL
	END
END
GO


IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Topic]') and NAME='RenderAsWidget')
BEGIN
	ALTER TABLE Topic ADD RenderAsWidget bit NOT NULL DEFAULT 0
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Topic]') and NAME='WidgetZone')
BEGIN
	ALTER TABLE Topic ADD WidgetZone [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Topic]') and NAME='WidgetShowTitle')
BEGIN
	ALTER TABLE Topic ADD WidgetShowTitle bit NOT NULL DEFAULT 1
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Topic]') and NAME='WidgetBordered')
BEGIN
	ALTER TABLE Topic ADD WidgetBordered bit NOT NULL DEFAULT 1
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Topic]') and NAME='Priority')
BEGIN
	ALTER TABLE Topic ADD Priority int NOT NULL DEFAULT 0
END
GO


IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Category]') and NAME='Alias')
BEGIN
	ALTER TABLE [Category] ADD [Alias] nvarchar(100) NULL
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[Order]') and NAME='OrderNumber')
BEGIN
	ALTER TABLE [Order] ADD [OrderNumber] [nvarchar](500) NULL
END
GO

-- SeoSettings.ExtraRobotsDisallows
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [Name] = N'SeoSettings.ExtraRobotsDisallows')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'SeoSettings.ExtraRobotsDisallows', N'', 0)
END


-- LocalizationSettings.DetectBrowserUserLanguage
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [Name] = N'LocalizationSettings.DetectBrowserUserLanguage')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'LocalizationSettings.DetectBrowserUserLanguage', N'False', 0)
END

-- LocalizationSettings.DefaultLanguageRedirectBehaviour
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [Name] = N'LocalizationSettings.DefaultLanguageRedirectBehaviour')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'LocalizationSettings.DefaultLanguageRedirectBehaviour', N'0', 0)
END

-- LocalizationSettings.InvalidLanguageRedirectBehaviour
IF NOT EXISTS (SELECT 1 FROM [Setting] WHERE [Name] = N'LocalizationSettings.InvalidLanguageRedirectBehaviour')
BEGIN
	INSERT [Setting] ([Name], [Value], [StoreId])
	VALUES (N'LocalizationSettings.InvalidLanguageRedirectBehaviour', N'0', 0)
END


-- New MessageTemplate "Product.AskQuestion"
IF NOT EXISTS (
  SELECT 1
  FROM [dbo].[MessageTemplate]
  WHERE [Name] = N'Product.AskQuestion')
BEGIN
	INSERT [dbo].[MessageTemplate] ([Name], [Subject], [IsActive], [EmailAccountId], [LimitedToStores], [Body])
	VALUES (
		N'Product.AskQuestion', 
		N'%Store.Name% - Question concerning "%Product.Name%" from %ProductQuestion.SenderName%', 
		1, 
		0, 
		0, 
		N'<style type="text/css"><!--
address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px; font-family: "Segoe UI", Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:720px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }
--></style>
<center>
<table border="0" cellpadding="0" cellspacing="0" align="center" bgcolor="#ffffff" class="template-body">
<tbody>
<tr>
<td>
<p>%ProductQuestion.Message%</p>
<p><strong>Email:</strong> %ProductQuestion.SenderEmail%<br /><strong>Name:</strong>&nbsp;%ProductQuestion.SenderName%<br /><strong>Phone:</strong>&nbsp;%ProductQuestion.SenderPhone%&nbsp;</p>
</td>
</tr>
</tbody>
</table>
</center>')
END
GO
