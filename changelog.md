#Release Notes#

##SmartStore.NET 2.0.2#

###Bugfixes###
* IMPORTANT FIX: Memory leak leads to _OutOfMemoryException_ in application after a while
* Installation fix: some varchar(MAX) columns get created as varchar(4000). Added a migration to fix the column specs.
* Installation fix: Setup fails with exception _Value cannot be null. Parameter name: stream_
* Bugfix for stock issue in product variant combinations
* #336 Product bundle: Upper add-to-cart button label shows wrong text
* #338 Serialization exception thrown when session state mode is _StateServer_
* #340 Admin: Header overlays TinyMCE in fullscreen mode
* #341 Orders are not cancellable
* #342 Backend: order total is not editable
* #348 Messaging: OrderPlacedStoreOwnerNotification overwrites email account sender name with the customer's name
* Default value for plugin description not loaded into edit popup window
* Fixed "Controller does not implement IController" (concerning plugin controllers)
* #361 Wrong delivery time in order confirmation

###Improvements###
* #250 Implemented validation to theme configuration editing


##SmartStore.NET 2.0.1##

###New Features###
* #292 Allow specific price for attribute combinations
* Added image upload support to Summernote editor
* (Developer) Added WebApi client test tools to the solution (C# and JavaScript)

###Improvements###
* Content slider slides can be filtered by store
* TinyMCE now shows advanced tab for images
* Updated BundleTransformer to the latest version 1.8.25
* Added JavaScriptEngineSwitcher.Msie (disabled by default). Useful in scenarios where target server has problems calling the V8 native libs.
* Updated some 3rd party libraries to their latest versions
* #320 Unavailable attribute combinations: better UI indication

###Bugfixes###
* UI notifications get displayed repeatedly
* (Developer) Fixed Razor intellisense for plugin projects (NOTE: switch to 'PluginDev' configuration while editing plugin views, switch back when finished)


##SmartStore.NET 2.0.0#

###Highlights###
* RESTFul **WebApi**
* Highly optimized and **simplified product management**
* Product **Bundles**
* Considerably **faster app startup** and page processing
* New variant attribute type: **Linked Product**
* **Package upload** for plugins and themes
* Lightning **fast asynchronous Excel import** with progress indicators and detailed reports
* (Developer) Code-based Entity Framework database **migrations**

###New Features###
* [RESTFul WebApi](https://smartstore.codeplex.com/wikipage?title=Web%20API)
* Product Bundles: create configurable product kits by combining products
* Package upload for plugins and themes
* New variant attribute type: Linked Product
* #137 Shipping method option to ignore additional shipping charges
* #175 IPayment plugin: Implemented deactivation of specific credit card types
* #191 Implemented new scheduled task _Delete logs_
* Added support for _SummerNote_ HTML editor (experimental)
* Enabled fulltext search
* New setting to redirect to order detail page if an order completed
* New setting to suppress the search for SKUs
* Shipment list can be filtered by tracking number
* #238 Working currency in context of request domain
* #295 Display short description for payment plugins
* Setting to skip the payment info page during checkout.
* (Developer) [Entity Framework code-based Migrations](https://smartstore.codeplex.com/wikipage?title=Migrations&referringTitle=Documentation) now fully supported in application core and all relevant plugins (no need to manually run SQL scripts anymore in order to update to newer version).
* (Developer) Admin: Implemented _entity-commit_ event message (for client EventBroker) in order to support custom data persistence in a loosely coupled manner.
* (Developer) New interface _IWidget_ for simplified widget development and multi-widgets per plugin
* (Developer) Outsourced notifications from MVC controller and implemented _INotifier_ for more centralized approach

###Improvements###
* Highly optimized and simplified product management
* Considerably faster app startup and page processing
* Lightning fast asynchronous Excel import with progress indicators and detailed reports
* #171: select2 Dropdown too short in OPC
* Product filtering by price now supports decimal places
* Enhanced Admin UI for _Message Templates_
* _Repliable_ Emails now have customer's email as ReplyTo address
* Fix for EU VAT calculation: resolve NET from GROSS correctly + auto-switch to NET display type when customer is VAT exempt
* Replaced dotLess engine with a native Javascript parser (BundleTransformer > ClearScript.V8)
* #140 Import all plugin resources after adding a language
* #45 Smarter logging. Save same log notifications only once including its frequency.
* Updated jQuery Mobile to version 1.3.2
* Updated TinyMCE html editor to version 4
* Overhauled plugin management UI (plugin search among others)
* Mobile: Only the first product pictures is now displayed in the product-detail-view the others are available by navigation or swiping
* Mobile: Shop logo is now displayed at the top of the mobile page
* Mobile: legal hints are shown in the footer
* #228 Added Youtube to social network settings
* #180 Display delivery time in shopping cart and relevant mails
* #217 GMC Feed Plugin: Make export of expiration_date configurable
* #222 Feed Plugins: Take special price into consideration
* Canceling a PayPal, SU or PostFinance payment now redirects to the order detail page rather than checkout complete
* Added an option to display the short description of products within the order summary
* Added an option to turn off the display of variant price adjustments
* #277 Show BasePrice (PAnGv) in cart also
* GMC feed plugin: Export configurable shipping weight and base price info
* #280 Filter orders by customer name
* #190 App Restart: stay on current page
* Filter orders: Order, payment and shipping status are multi-selectable
* DatePicker control for variant attributes: displayed year range can be specified via _Alias_ property ([BeginYear]-[EndYear], e.g. 1950-2020)
* Significantly faster install process
* * Updated all dependant libraries to their latest versions
* (Developer) Implemented _PaymentMethodBase_ abstract class to simplify payment plugin development

###Bugfixes###
* #150 GTB & disclaimer aren't readable when they become to long
* #151 NewsletterSubscriptionDeactivationUrl-Token doesn't get repleaced
* #153 Admin->Contentslider throws an error when asigned languages aren't available anymore
* #160 Resource on confirm order page isn't escaped
* #152 Copy message template does not work
* SKU search did not work cause of wrong join statement
* #165 TopicWidgets Sorting is not applied
* Google Analytics: with active _Order Number Formatter_ plugin the order number was posted twice (formatted and unformatted)
* #188 Token Order.CustomerFullName doesn't get replaced when ordering as a guest
* #194 Installation fails when installing products
* #196 Samples cannot be downloaded
* Product filter included deleted manufacturers
* Mobile: Paginator does not work
* Product could be overwritten by attribute combination data
* Quantity field was shown though the add to cart button was disabled
* #260 Delivery times translations are not getting applied
* robots.txt: localizable disallow paths did not contain a trailing slash
* #296 Fix price adjustment of product variant combinations
* Resolved shopping cart rounding issues (when prices are gross but displayed net)



##SmartStore.NET 1.2.1.0#

###New Features###
* Added option *Limit to current basket subtotal* to _HadSpentAmount_ discount rule
* Items in product lists can be labelled as _NEW_ for a configurable period of time
* Product templates can optionally display a _discount sign_ when discounts were applied
* Added the ability to set multiple *favicons* depending on stores and/or themes
* Plugin management: multiple plugins can now be (un)installed in one go
* Added a field for the HTML body id to store entity
* (Developer) New property 'ExtraData' for DiscountRequirement entity

###Bugfixes###
* #110: PictureService.GetThumbLocalPath doesn't add picture to cache if it doesn't exist (this broke PDFService among others)
* #114: Runtime error occurs after new customer registration in the storefront
* #115: XML exports could fail because of invalid characters
* #121: Categories restricted to specific stores could also show up in other stores.
* #125: Widget Trusted Shops Customer Reviews can not be configured
* #127: Redirection to 404 failed with localized urls when shop ran under virtual application paths
* #128: _Switch language_ in store failed when SEO friendly urls were disabled
* #134: Fix mobile checkout
* #111: Send wishlist via e-mail doesn't work

###Improvements###
* #97: Product numbers of attribute combinations could not be searched
* #120: Excel product import: The same product pictures were imported repeatedly which led to duplicate pictures.
* Updated _FontAwesome_ to version 3.2.1
* Minor CSS fixes
* Info pages for all Trusted Shops widgets 
* Better display and handling when choosing a flag for languages


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
 - bunch of fixes and improvements for the _SmartStore.biz Importer_ plugin
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


