#Release Notes#

##SmartStore.NET 1.2.1.0#

###New Features###
* Added option *Limit to current basket subtotal* to _HadSpentAmount_ discount rule
* Added the ability to set multiple *favicons* depending on stores and/or themes
* Added a field for the HTML body id to store entity
* (Developer) New property 'ExtraData' for DiscountRequirement entity

###Bugfixes###
* #110: PictureService.GetThumbLocalPath doesn't add picture to cache if it doesn't exist (this broke PDFService among others)
* #114: Runtime error occurs after new customer registration in the storefront
* #115: XML exports could fail because of invalid characters
* #121: Categories restricted to specific stores could also show up in other stores.* #125: Widget Trusted Shops Customer Reviews can not be configured
* #127: Redirection to 404 failed with localized urls when shop ran under virtual application paths
* #128: _Switch language_ in store failed when SEO friendly urls were disabled

###Improvements###
* #97: Product numbers of attribute combinations could not be searched
* #120: Excel product import: The same product pictures were imported repeatedly which led to duplicate pictures.
* Info pages for all Trusted Shops widgets 


##SmartStore.NET 1.2.0.0#

###Highlights###
 - Multi-store support
 - "Trusted Shops" plugins
 - Highly improved _SmartStore.biz Importer_ plugin
 - Add custom HTML content to pages
 - Performance optimization

###New Features###
 - **Multi-store-support:** now multiple stores can be managed within a single application instance (e.g. for building different catalogs, brands, landing pages etc.)
 - Added 3 new **Trusted Shops** plugins: Seal, Buyer Protection, Store Reviews
 - Added **Display as HTML Widget** to CMS Topics (store owner now can add arbitrary HTML content to any page region without modifying view files)
 - **Color attributes** are now displayed on product list pages (as mini squares)
 - Added **ask question form** to product pages
 - _ShippingByTotal_ plugin: added __BaseFee__, __MaxFee__ and __SmallQuantitySurcharge__.
 - _ShippingByTotal_ plugin: added the ability to define __zip ranges__ including __wildcards__.
 - Added 2 DiscountRule plugins: __HasPaymentMethod__ and __HasShippingOption__
 - Better handling of localized SEO URLs (new settings: __DetectBrowserUserLanguage__, __DefaultLanguageRedirectBehaviour__ and __InvalidLanguageRedirectBehaviour__) 
 - Plugin language resources are now updateable via backend
 - Added __Attributes__ tab to Admin > Order Detail
 - Added new (hidden) setting __ExtraRobotsDisallows__ (enables store owner to add new Disallow lines to the dynamically generated robots.txt)
 - (Developer) Added new plugin __Glimpse for SmartStore.NET__
 - (Developer) **Localizable views:** the view engine now is able to resolve localized (physical) view files (by appending the language seo code to a view file name in the same folder, e.g. 'en' or 'de'). The engine first tries to detect a view file with the matching language suffix, then falls back to the default one.
 - (Developer) Added new interface __IPreApplicationStart__ allowing plugins to register HttpModules very early in the app bootstrap stage

###Improvements###
 - Minor improvements for _SOFORT Überweisung_ plugin
 - ContentSlider: updated 'sequence js' to most recent version and optimized html & css code
 - Content slider: the background slide behaviour is configurable now (NoSlide, Slide, SlideOpposite)
 - Some email templates sent to the store owner now have customer's address in the 'from' field
 - Topic titles are now shown in _Admin > CMS > Topics_ grid
 - Log entry full messages are now displayed prettified in the backend
 - Croatia now has EU-VAT handling
 - Minor theme improvements
 - Updated _FontAwesome_ to the latest version
 - Added 'Admin' Link to the header menu (many users just didn't find it in the 'MyAccount' dropdown ;-))
 - Added new option to disable/enable the display of a more button when the long description is to large
 - Added option to switch back to mobile view when displaying the desktop version on a mobile device
 - (Developer) Added properties for HtmlHelper and Model to 'AdminTabStripCreated' event type. This way external admin UI customization becomes easier and more flexible.
 - (Developer) Implemented minor tweaks and optimizations to the Theming Engine
 - (Developer) Added 'bodyOnly' parameter to TopicBlock ChildAction
 - (Developer) HtmlHelper __SmartLabelFor__ now renders the model property name by default (instead of the _SmartResourceDisplayName_ resource key)

###Bugfixes###
 - A bunch of fixes and improvements for the _SmartStore.biz Importer_ plugin
 - The feed for "Leguide.com" plugin did not work in Germany
 - Fixed minor issues in _shipping-by-weight_ plugin
 - Fixed minor issues in _Google Analytics_ widget
 - Paginator always jumped to the first page when using the product filter
 - Modal window for disclaimer topic was missing the scrollbar
 - Main menu doesn't flicker anymore on page load
 - ThemeVars cache was not cleaned when theme was switched
 - Content slider: container background did not slide in Firefox
 - KeepAlive task minor fix
 - (Developer) Fixed minor issues in build script



##SmartStore.NET 1.0.1.0##

###Bug###

    * [SMNET-1] - Die Anzahl der eingetragenen Mengen bei Varianten wird nicht richtig im Warenkorb übernommen.
    * [SMNET-5] - Fehler beim Hochladen von Bildern im IE
    * [SMNET-6] - Texte in IFrame werden nicht komplett dargestellt
    * [SMNET-7] - Versandart  „Shipping by total“ funktioniert nicht.
    * [SMNET-18] - Megamenu: Expand/collapse schneidet u.U. Submenu ab
    * [SMNET-19] - Im Firefox können keine Inhalte im TinyMCE-Editor per Kontextmenu eingefügt werden.
    * [SMNET-23] - Im HTTPS Modus wird (LESS)-CSS nicht interpretiert
    * [SMNET-25] - Bei Eingabe einer falschen SSL-URL im Admin-Bereich ist kein Einloggen mehr möglich
    * [SMNET-34] - Fehlermeldung nach dem Hochladen einer sbk-Datei 
    * [SMNET-46] - Es können keine NULL_Werte in SpecificationAttribute-Tabelle eingefügt werden
    * [SMNET-58] - Katalog-Einstellungen - Productlist PageSize wird nicht gespeichert
    * [SMNET-59] - ContentSlider: Wenn bei einem Slide kein Titel hinterlegt ist, kommt es zu einem Fehler
    * [SMNET-61] - Wenn man beim Produktnamen in einer anderen Sprache einen Eintrag macht, erfolgt eine Fehlermeldung
    * [SMNET-83] - Theme-Export schlägt mit Runtime-Error fehl
    * [SMNET-142] - Die prozentuale Berechnung des Aufpreises bei der Zahlungsart Kreditkarte funktioniert nicht.
    * [SMNET-150] - Die Angaben aus dem Voraussetzungstyp "Benötigte Kundengruppe" bei der Einrichtung eines Rabatts werden nicht gespeichert.
    * [SMNET-152] - Fehler beim Speichern von Produkten, wenn lokalisierte Felder (aber nicht alle) mit Werten belegt werden.
    * [SMNET-166] - GIF-Dateien werden nach dem Import mit schwarzem Hintergrund übernommen.
    * [SMNET-167] - Produkte sind nach Spezifikations-Attributen filterbar, obwohl dieses Spezifikations-Attribut überhaupt nicht dem Produkt zugeordnet wurde.
    * [SMNET-171] - Import von Newsletter-Adressen scheint bei zu vielen Datensätzen irgendwann "auszusteigen" (Performance-Problem)
    * [SMNET-174] - Bezahloption Lastschrift: Kontodaten des Kunden im Backend nirgendwo einsehbar
    * [SMNET-196] - Preisberechnung für Variant-Kombis fehlerhaft (Rabatte werden nicht berücksichtigt)
    * [SMNET-198] - upgrade.sql für Order.DirectDebit[...] fehlerhaft
    * [SMNET-199] - CategoryNavigationModel: Children von inaktiven Warengruppen müssen in Navigationsleisten ignoriert werden
    * [SMNET-202] - SmartTabSelection mit verschachtelten Tabs fehlerhaft nach Reload einer Seite

###Improvement###
    
    * [SMNET-13] - Attributwerte: der Text "Aufpreis" muss um "Minderpreis" erweitert werden.
    * [SMNET-15] - Umgestaltung der Darstellung der Staffelpreise (Popover ab dem fünften Element)
    * [SMNET-30] - MessageTemplates teilweise auf Englisch
    * [SMNET-31] - Die Bestellbestätigung enthält keine Widerrufsbelehrung.
    * [SMNET-33] - Falsche Beschriftung bei MWST-Befeiung (incl. VAT (0%)) 
    * [SMNET-39] - CSS-Klasse .category-description sollte etwas margin nach unten haben 
    * [SMNET-40] - Beim Löschen von lokalisierte Ressourcen erscheint eine Englische Meldung
    * [SMNET-43] - Bilder und der Langtext von Warengruppen werden beim Import nicht übernommen.
    * [SMNET-47] - Fehlermeldung: Zeichenfolgen- oder Binärdaten würden abgeschnitten [...]
    * [SMNET-48] - Legt man als nicht angemeldeter Kunde ein Produkt in den Warenkorb und geht anschl. in den Checkout, hat man kein Möglichkeit mehr, als Gast zu bestellen.
    * [SMNET-49] - Varianten: Normalpreis > Variantpreis > Rabatt, u.U. inkonsistent und unlogisch
    * [SMNET-56] - Der Hinweis „Preis auf Anfrage“ wird nicht auf der Produktdetailseite angezeigt.
    * [SMNET-69] - Beschreibung für Parameter in den MessageTemplates
    * [SMNET-138] - Währungsformatierung für EUR überdenken (Tausender-Trenner fehlt)
    * [SMNET-139] - Feld 'Alias' in Attribut-Grid aufgenommen
    * [SMNET-140] - ProductAttribute.Description beim Import sollte nicht mehr vom Varianttyp-Namen abgeleitet werden
    * [SMNET-147] - ColorSquares Admin: kleines Farbquadrat im Admin-Grid links neben Beschriftung anzeigen
    * [SMNET-151] - ColorSquares: Active-Zustand besser hervorheben (z.B. dunklerer border)
    * [SMNET-153] - MiniBasket: "Zur Kasse" Button per Default aktivieren
    * [SMNET-160] - Fehlermeldungen beim Anlegen eines Kunden sind nicht lokalisiert.
    * [SMNET-180] - Leichten Border und Verlauf in Lieferzeit-Indikator eingebaut
    * [SMNET-188] - Lokalisierung: IsDirty-Flag und Option "Nur neue anfügen"

###New Feature###

    * [SMNET-14] - Brutto/Netto Preisanzeige über Kundengruppen steuerbar


