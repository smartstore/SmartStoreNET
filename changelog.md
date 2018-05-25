# Release Notes

## SmartStore.NET 3.1.5

### Highlights
* Compliance with EU-GDPR requirements
* Search engine friendly topic URLs
* "Honeypot" bot detection for registration and contact forms.

### New Features
* #1429 Search engine friendly topic URLs
* Implemented cookie consent according to EU-GDPR
* Added checkboxes for data processing consent in all relevant forms
* Implemented "Honeypot" bot detection for registration and contact forms.
* Trusted Shops: Added consent checkbox to confirm order page for submission of customer email address to Trusted Shops if review widget is active
* #1226 Shop-Connector: Added exchange of tier prices and delivery times
* #1439 Debitoor: Option whether to display the payment method and SKU on invoices

### Improvements
* Added double opt-in feature for newsletter subscriptions during checkout (confirm order)
* Allow forward slash in product tag URL slug
* Theming: throttle AJAX cart refresh after spin up/down click
* Moved StoreLastIpAddress & DisplayPrivacyAgreementOnContactUs from customer settings to privacy settings tab
* #1450 Show the regular price only if it's higher than the final price
* #1450 Do not ignore discounts with a negative amount
* (Soft) deleted customers can be edited now
* Customer IP addresses will be anonymized on (soft) deletion
* Set catalogsettings.showsharebutton to false as its not compliant with GDPR
* Made form fields for first & last name in customer registration optional
* Implemented settings to make form fields for first & last name required again
* Made form field for full name in contact us & product request optional
* Implemented settings to make form field for full name in contact us & product request required again
* #1453 Import: Use [IGNORE] to ignore a field value on record level
* #1455 More detail on packing slip when bundled item
* Display category alias as badge in grids and dropdowns

### Bugfixes
* Migration: take all same-named message templates into account
* Messaging: OrderPlaced e-mail templates show main product image even when an attribute combination with a custom image has been selected
* Theming: fix broken product review voting
* Theming: added missing bottom space to .html-editor-content
* Theming: Language switcher is not displayed if no currency options are available
* No bundle item thumbnail displayed in bundle summary if item is not individually visible
* Tracking number in shipment details was not saved
* Assigning or removing product tags did not invalidate model cache
* Reward points weren't displayed in message templates
* Dashboard: link for not yet shipped orders loads list with all orders
* Topic search button had no effect
* #1442 Message factory should not throw an exception if a template has been deactivated
* Fixes script error "$(...).tab is not a function" on product detail page
* Title attribute for the product name in product lists was sometimes truncated
* Relativizing font sizes should cast to double, not int
* Fixes category list on product edit page shows empty category name
* #1438 Debitoor: The country is displayed twice
* MegaSearch: Fixes indexing ignores DeliveryTimeIdForEmptyStock setting
* Web API: Fixes "No NavigationLink factory was found for the navigation property 'WalletHistory'"
* #1449 IgnoreCharges of shipping methods is not working if a localized name is specified
* Fixes "The object does not support the property or method 'startsWith'" on product edit page.
* Wallet: Fixes "Child actions are not allowed to perform redirect actions" when there are cart warnings
* Fixes the delivery time in the order notifications may differ from delivery time on the product detail page


## SmartStore.NET 3.1.0

### Highlights
* **Wallet**: Enables full or partial order payment via credit account. Includes REST-Api. (commercial plugin)
* **[Liquid](https://github.com/Shopify/liquid/wiki/Liquid-for-Designers) template engine**: very flexible templating for e-mails and campaigns with autocompletion and syntax highlighting.
* **Cash Rounding**: define money rounding rules on currency and payment method level.
* **Modern, responsive backend**: migrated backend to Bootstrap 4, overhauled and improved the user interface.
* **Enhanced MegaMenu**: virtual dropdowns for surplus top-level categories and brands (commercial plugin exclusively bundled with Pro Edition).
* **RTL**: comprehensive RTL (Right-to-left) and bidi(rectional) support.
* **Amazon Pay**:
	* Supports merchants registered in the USA and Japan
	* External authentication via *Login with Amazon* button in shop frontend
	* Several improvements through the new *Login and pay with Amazon* services
* (Perf) **Faster image processing**: new processing and caching strategy! Thumbnails are not created synchronously during the main request anymore, instead a new middleware route defers processing until an image is requested by any client.
* **TinyImage**: scores ultra-high image compression rates (up to 80 %!) and enables WebP support (commercial plugin exclusively bundled with Premium Edition).
* **UrlRewriter**: define URL redirection rules in the backend using *mod_rewrite* notation. (commercial plugin)
* **Address formatting** templates by country
* **Language packs**: downloader & auto-importer for packages available online.

### Breaking changes
* Message template customizations are lost due to the new template engine. You have to customize the templates again. No automatic migration, sorry :-(
* Amazon Pay: The plugin has been changed to new *Login and pay with Amazon* services. The client ID has been added, which has to be created in Amazon Seller Central and saved in the payment method configuration.
* (Dev) Calls to cache methods `Keys()` and `RemoveByPattern()` require glob chars to be present now (supported glob-styles see [https://redis.io/commands/keys](https://redis.io/commands/keys)). Previously these methods appended `*` to the passed pattern, which made pattern matching rather unflexible.
* (Dev) Hook framework now passes `IHookedEntity` interface instead of `HookedEntity` class
* (Dev) Completely removed all `EntityInserted<T>`, `EntityUpdated<T>` and `EntityDeleted<T>` legacy events. We were using DbSaveHooks anyway, which provides a much more powerful and way faster pub-sub mechanism for database operations.

### New Features
* 1203 MegaMenu shrinker and *Brands* virtual menu item
* [Summernote](https://summernote.org/) is now the primary HTML editor
* #431 Added option to randomize the display order for slides on each request
* #1258 Add option to filter shipping and payment methods by a specific customer role
* #1247 Allow to import non system customer roles in customer import
* #1117 Added an option to display a dropdown menu for manufacturers 
* #1203 Added an option to define a maximum number of elements in the main menu for the first hierarchy of the catalog navigation
* GMC: column chooser for edit grid
* #1100 Customer can register in frontend via "Login with Amazon" button
* **Web API**:
	* #1292 Added endpoint to get order in PDF format
	* Added endpoint to complete an order
	* #1364 Added endpoints for MeasureWeight and MeasureDimension
* Added options to include option names of specification and product attributes in the search index
* #441 added option to specify that additional shipping surcharges are considered only once.
* #1295 Sales tracking (tracking pixel) for Billiger.de
* XML and CSV export of shopping cart and wishlist items
* #1363 Make storing of IP addresses optional
* #729 Option for automatic order amount capturing when the shipping status changed to "shipped"
* (Dev) ILocalizationFileResolver: responsible for finding localization files for client scripts
* #998 GMC: Find a way to map attribute combination values to feed export values
* Added Instagram icon to social media icons in footer

### Improvements
* Target .NET Framework changed: 4.5.2 > 4.6.1.
* Lower memory consumption
* #649 Media FileSystem provider: segmenting files in subfolders to increase IO perf with huge amount of files
* #1141 Cleaner backend order list. Added more infos like payment method.
* OuputCache: Simple product changes that affect visibility now correctly invalidate all assigned category and manufacturer pages
* * OuputCache: When MegaSearch is active, invalidation occurs only during indexing and not ad-hoc anymore. 
* #1248 New payment integration guidelines for Sofort\Klarna
* TwitterAuth: better error handling and enhanced admin instruction
* #1181 Debitoor: Add option to display shipping address on invoices
* Moved RoundPricesDuringCalculation setting to currency entity
* #1100 Use new "Login with Amazon" services to initialize an Amazon payment
* #1285 Copy product: Add option to add more than one copy
* (Perf) Many improvements in hooking framework
* #1294 Swiss PostFinance: External payment page too small on mobile devices. Added setting for mobile device template URL, pre-configured with PostFinance template.
* #1143 Make shipping methods suitable for multi-stores
* #1320 Image import: Find out the content type of image URLs by response header rather than file extension (which is sometimes missing)
* #1219 Recently viewed products list should respect setting to hide manufacturer names
* Import and export product quantity step
* Add bundle item information to order messages
* #1031 Enable offline payment methods to have brand icons
* DevTools Plugin: Added example for cached output invalidation 

### Bugfixes
* #1268 Data importer always inserts new pictures and does not detect equal pictures while importing
* OutputCache computes ambiguous cache keys for blog pages
* #1142 Customer import creates role multiple times
* #1244 Variant query model binder cannot handle types text and datepicker
* #1273 Attribute formatter should consider setting CatalogSettings.ShowVariantCombinationPriceAdjustment
* Product entity picker should use the wildcard search to find products
* Hook framework should run hooks with `ImportantAttribute` when hooking was disabled per scope
* #1297 Web API: Parsing the timestamp may fail due to the different accuracy of the milliseconds
* Debitoor: VAT amount could be transmitted as miscellaneous for deliveries abroad.
* Prices with discounts limited to categories and customer groups were shown to all users in product lists
* #1330 MegaSearch: Missing variant facets if the variant value is not unique
* Back-in-stock subscription form was already submitted when opening the popup dialog
* Associated products of a grouped product were displayed in the wrong order
* Payment-Filter: Fixed "The cast to value type 'System.Decimal' failed because the materialized value is null"
* The tax value per tax rate was not updated when adding\removing a product to\from the order.
* The option to send manually was ignored when sending e-mails
* #528 LimitedToStores is required on payment provider rather than plugin level
* #1318 Disabled preselected attribute combination permanently hides the shopping cart button, even if another combination is selected.
* Copy product: Fixes "Cannot insert duplicate key row in object dbo.UrlRecord with unique index IX_UrlRecord_Slug"
* Fixed export publishing via email

## SmartStore.NET 3.0.3
### Bugfixes
* #1232 Scheduled tasks are being 'randomly' set inactive
* #1245 Existing product does not get updated during import under certain circumstances
* #1207 No low stock activity when bulk editing the product quantity
* #1208 Missing inventory tab when switching from bundle to simple product
* #1210 Clear asset cache when a theme setting has changed
* Fixed name of deleted manufacturers displayed in product lists
* Fixed non-system category export profile always exports categories of current store
* Fixed picture assignment for variant attribute combinations of product copies which weren't assigned correctly
* #1218 CopyProductService does not copy picture of ProductVariantAttributeValue
* Fixed hidden checkout button when there are two or more shopping cart items and one gets removed from basket during checkout
* Fixed physical gift cards cannot be moved to cart
* Fixed tax rate wasn't properly formatted on product detail page
* #1228 Text entered in frontend for attributes or checkout attributes are truncated by comma
* Fixed shipping computation method ignoring deactivated PricesIncludeTax setting
* **Debitoor**: Fixed missing tax rates on Debitoor invoice for net prices
* #1224 Notifier wasn't working in plugin controllers
* #1205 Server cannot append header after 
* 
*  headers have been sent
* #1154 Left offcanvas navigation does not open when in checkout progress
* #1212 Export: FTP publishing should consider directory structure
* #1253 Product PDF exporter only exports one picture and ignores the picture number profile setting
* Configuration of some plugins not saved in Firefox browser
  
### Improvements
* Apply img-fluid class to all images within html editor content
* System name in privacy info topic URL should be lowercase 
* If no checkout attribute is required checking out will be possible
* **OpenTrans**: optimizations for Lexware import
* #496 show bonus points in ordersummary on first checkout page
* Discounts of plugins weren't displayed in product lists 

### New Features
* #1076 Add support for percental tier price adjustments
* **Trusted Shops**: added review import
* (Dev) SSL redirection on localhost will be bypassed
* #783 Added option for "Email a friend" which disables the form field "Your email address" so it can't be changed 

## SmartStore.NET 3.0.2
### Breaking changes
* MegaSearch: The index must be re-created because of changes to the price filter.

### New Features
* New setting indicates whether to include or exclude unavailable products in search results by default.
* (Dev) New Asset Cache 

### Improvements
* PayPal Standard: New settings "UsePayPalAddress" and "IsShippingAddressRequired" to avoid payment rejection due to address validation.
* More widget zones for the footer
* #1177 Implement new setting for product search sorting
* More fault tolerant task scheduler

### Bugfixes
* Fixed memory leak in MegaSearch plugin, which caused HTTP 503
* RSS: Item URLs in feeds have ignored SSL settings
* Fixed "The view 'ProfileImportResult' or its master was not found"
* #1187 Search: Do not hide multi-selectable filters of the same group if there are no hits
* Fixed System.ArgumentNullException in ProductVariantQueryFactory
* PayPal PLUS: Fixed HTTP 401 "Unauthorized" when calling PatchShipping
* #1189 MegaSearch: Boosts are ignored in prefix and wildcard queries by default
* MegaSearch: Localized labels of filters were never displayed
* #1195 Exporter: don't send an email if no email account has been selected
* Product lists sometimes show the wrong delivery time
* #1192 Lucene indexing performance decreases the longer it takes
* #1198 MegaSearch: never sort numeric range by label, always by value
* Filter for attributes were always sorted by hit count
* #1200 PayPal PLUS: Invalid request if the order amount is zero
* Fixed null reference exception when copying attribute option set with image(s) (file system storage only)
* Product price sometimes was wrong when entering a numeric value in an attribute text box
* Added missing code for customer privacy agreement 
* Clicking the login link wasn't working correctly in offcanvas my-account menu
* #1158 Currency and language selectors weren't working in OffCanvas menu  
* Fixed "Server cannot append header after HTTP headers have been sent"

## SmartStore.NET 3.0.1
### Improvements
* (Perf) Much faster application startup
* Microsoft Visual C++ 2015 Redistributable no longer required to be installed
* **BeezUP**:
	* Exports up to 9 product images
	* Export stock quantity when stock is managed by attributes
	* Export parent child relation data when exporting attribute combinations as products
	* Flatten and append attribute data when exporting attribute combinations as products
* Instant search should search manufacturer/brand name
* HTTP 301 redirects for legacy media URLs
* (Dev) New 'ApplicationStarted' event
* (Dev) Enabled C# 6 features in plugin views
* (GMC) Added missing database indexes.
* (WebApi) Added endpoints for blog post and blog comment
* Added more sortable columns to backend product grids
* #1160 Append suffix 'Picture (1...n)' to thumbnail's alt-attribute on product detail pages
* (Perf) Faster catalog indexing thanks to new database indexes
* (Perf) Faster dynamic view compilation thanks to *Roslyn* compiler
* Added the widget zone 'productdetails_pictures_bottom'
* Added config setting *sm:PdfEngineBaseUrl*. There are cases where the PDF converter exits with a network error, when it is unable to load automatically resolved URLs.
* (Dev) Added *Retry* utility class
* #1176 Admin > Product Search: It ain't possible to search for parts of a product name
 
### Bugfixes
* #1145: Fixed HTTP 404 after switching language
* Fixed null reference exception in product lists if sorting is not allowed
* The file manager did not work correctly under a virtual folder
* Item row in list style product list should not wrap on small devices
* Item row in list style product list squashed on Android system browser
* Linq search: Filter for manufacturers and categories were not limited by store
* (GMC) Fixes duplicate occurrence of application path in absolute product URL
* Products with stock managed by attributes sometimes not displayed in category lists
* Fixed System.MissingMethodException for SmartStore.Core.Search.Facets.FacetGroup
* Fixed uploaded image paths in HTML fields (Media/Uploaded > Media/<TenantName>/Uploaded)
* (MegaSearch) Product category order, product manufacturer order and product order were not recognized (sorting)
* Standard search: Fixed sorting of recently added products
* Alias for search filters couldn't be set if there's only one language
* #1168 Cart: MinOrderAmount sometimes ignored
* Moving products from editable wishlist to cart was out of function
* Fixed several issues with parallelly executed data export tasks
* Fixed several issues with *PayPal* payment providers
* Facebook login out of function due to Facebook API changes (always returns "Unknown error")
* Fixed 'no picture available' watermark on some product pictures
* #1153 Removing already applied gift card causes error
* (Theming) *Drift* zoom uses static zoomFactor. Made it dynamic.
* (Theming) IE11: simple menu dropdowns weren't positioned correctly
* Removed "trust" element from web.config (causes problems on some shared hosting spaces)
* Fixed ThumbZoomer unproportional rescale issue in product grid
* #1134 JavaScript error (IE 11 in VS debug mode) after removing the last item in shopping cart
* (Theming) Dozens of layout fs for ixemobile devices (especially iOS and Android native browser)
* Better error logging for TaskScheduler
* Fixed several queries which caused problems with SQL CE
* Fixed missing CurrencyCode exception in product exports
* #1179 Error while downloading a downloadable product
* Mega Menu: If a dropdown contains two level hierarchy categories only, they are not wrapped to the next column
* Hide sort dropdown when option is off
* HTTP 301 redirects for legacy media URLs
* Added missing resources of broken migration 'AddressEnhancement' again
* Checkout: Title and salutation were missing when entering new addresses
* #1163 ContentSlider: pictures won't be displayed when placed on top or bottom on IOS 

## SmartStore.NET 3.0

### Highlights
* **Flex**: New mobile-first responsive Theme based on Bootstrap 4
* **Mega Search**: highly professional search framework based on Lucene.NET (commercial plugin exclusively bundled with Pro Edition)
	* Ultra fast search results, even with millions of items
	* Faceted search
	* Synonyms
	* Compound word splitting
* **Mega Menu**: highly customizable catalog menu widgets (commercial plugin exclusively bundled with Pro Edition)
* **Content Slider**: creates eye-catching content to boost sales (commercial plugin exclusively bundled with Pro Edition)
* **Output Cache** with "donut hole caching" for maximum speed and scalability (commercial plugin exclusively bundled with Premium Edition)
* **Microsoft AZURE** provider for media storage (commercial plugin exclusively bundled with Premium Edition)
* Web Farms: **REDIS** providers for Business Cache, Output Cache and Session State (commercial plugin exclusively bundled with Enterprise Edition)
* Product variant **option sets**
* New product specification attribute type: **numeric range**
* Image support for variant attributes

### Breaking changes
* Removed old **Content Slider** module from open source core
* Removed **Web API** plugin from open source core (now exclusively bundled with Premium Edition)


### Other New Features
* Message Bus for inter-process messaging between servers (commercial plugin)
* Configurable media storage path for web farms
* (Dev) log4net integration
* (Dev) proper plugin view debugging
* (Dev) detect changes to static plugin files (css, js, etc.) and immediately apply them to running project
* (Dev) Theming: plugins now support implicitly imported SASS files (Content/[public|admin].scss)
* XML Sitemap optimized for very large catalogs:
	* Partitions for very large sitemaps (> 50.000 nodes or > 10 MB)
	* Generated in a background task. No instant invalidation anymore.
* Added option to skip shipping method selection in checkout process if only one shipping method is active
* Added options to capture salutation and title in addresses and customer info
* Added projection to control the export of individually visible associated products
* #966 Implement new tax calculation logic for shipping and payment fees (calculate with rate of highest cart amount)
* #922 New option to specify whether to include the weight of free shipping products in shipping by weight calculation
* #724 Allow discounts to be applied to manufacturers
* Option to display manufacturers sorted in alphabetical order
* #1093 New product properties required for selling abroad
* Web API:
	* #1002 Add support for addresses and customer roles navigation property of customer entity
	* #1062 Add filter options for user grid on configuration page
	* #1072 Add support for TaxCategory
	* #1073 Settings for maximum pagesize ($top) and maximum expansion depth ($expand)
	* #1074 Extend product image upload to allow updating of images
	* #1064 Deleting all product categories/manufacturers per product in one go
	* #1063 Adding product category/manufacturer ignores any other property like DisplayOrder
	* Added endpoint "Infos" for order and order item entity for additional information like aggregated data.
	* Swagger integration
* Added setting to specify whether the product manufacturer should be displayed in product detail
* #271 Implemented support for attribute images/icons
* #330 Implement 'attribute option sets'
* Two themes Flex Black and Flex Blue which are derived from Flex theme    


### Improvements
* Updated .NET Target Framework from 4.5.1 to 4.5.2
* Added order message token for accepting third party email handover
* ECB currency exchange rate provider now cross calculates rates based on euro rates
* BeezUP: Exports the product weight
* Updated Elmar shop info XML from version 1.1 to 2.0
* (Perf) Application start faster by ~20%
* (Perf) Lower memory consumption
* (Perf) #1098 Reduce number of created guest accounts (more reliable bot detection)
* #1008 Export: Add support for description projection to all product exporting providers
* #1015 Implement Entity Picker in discount requirements
* Debitoor improvements:
	* Company name send as customer name if present
	* Option to append customer email address to invoice address
	* Additional order notes for tax exemption within and outside EU
* #1048 Add meta tags to contact us page
* Order XML export includes more customer data like title and salutation
* #1080 Do not resolve Order.OrderURLForCustomer token for guest customers
* #1051 Display EAN in PDF packaging slip
* #806 User couldn't be created in the admin section without "Manage Customer Roles" permission 
* #491 Implemented option to determine a default delivery time
* #1078 Implemented options for "Add to Cart" quantity input field on product level. Quantity control can now be hidden and can be configured to be increased and decreased in steps.
* #696 Added textual resources for meta title and description for blog per month and by tag pages and enriched them with corresponding month plus year respectivly tag name
* #1025 Added token for customer number to MessageTokenProvider 
* #1016 Implemented choosing of MessageTokens for newsletter campaigns with dropdown menu
* #1107 OpenTrans: Added option to exclude long description of products from export
* BMEcat: Implemented download of pictures according to MimeRoot-Info
* Payone:
	* Replace client API by Payone iFrame solution. Allows credit card payment compliant with PCI DSS, SAQ A.
	* #1123 Add order note with payment info for prepayment and invoice payment.


### Bugfixes
* Currency wasn't displayed for shipping estimation
* SKU, EAN, MPN of last attribute combination were exported for all combinations
* GMC:
	* Id should be unique when exporting attribute combinations as products
	* Attribute price adjustments were ignored when exporting attribute combinations as products
	* Associated products that are not individually visible are not exported anymore. GMC rejects them because the frontend redirects to the grouped product.
* Export:
	* #999 Projected customer id was ignored during price calculation
	* #1104 Language projection was ignored when creating product details URL
	* #1030: Orders didn't include data of attribute combinations
	* Orders didn't include shipping address
* Awarded reward points for a placed order were occasionally calculated wrong
* PayPal PLUS:
	* Changed shipping address/costs were not transmitted to PayPal
	* Skip payment if cart total is zero
	* Do not display payment wall if method is filtered
* PayPal Express: Fixed net price issue.
* Bundle item cannot be deleted if it's in a shopping cart
* Fixed SSL issue for news items in RSS feed
* Filter shows wrong number of products if "Include products from subcategories" is activated
* Guest user cannot be deleted if he has a private message
* #1029 Biz-Importer: Imports fixed tax rates as zero percentage
* #1055 Checkout: Localized shipping method names are not displayed if shipping by total is activated
* #1071 NewsLetterSubscription.ActivationUrl not working if the store if SSL secured
* #1086 Gift cards can be earned in one store and applied in another
* Whishlist: Products with minimum order amount greater then 1 can be added to the whishlist without any warning
* #1102: Discounts not transmitted when the total amount is zero
* #1101 Clickatell: Sending SMS not working anymore

## SmartStore.NET 2.6

### Highlights
* Major improvements in Importer & Exporter: better field mapping, higher performance, bug fixes etc.
* 'PayPal PLUS' payment plugin
* 'paydirekt' payment plugin
* 'Viveum' payment plugin
* 'BeezUP' export provider
* (Dev) Publishing SmartStore.Web from within Visual Studio now deploys the project correctly. No need to execute ClickToBuild.cmd anymore. Just publish directly to any target, including Azure.

### New Features
* #961 Fix "Open Redirection Vulnerability"
* #571 Option to display another checkbox on confirm page to let the customer accept that his email address can be handed over to a third party
* #870 Implement address import for customers (both billing & shipping address)
* #886 Add setting to hide manufacturer images on product detail page and to hide default image for manufacturers
* Import localized SEO names for product and categories
* #477 Implement option to specify the number of exported and imported pictures
* #859 Make checkout attributes suitable for multi-stores
* Product details: Select attribute and gift card values by query string parameters
* #950 make contact form comply with current German law

### Improvements
* Major improvements in Importer: better field mapping, higher performance, bug fixes etc.
* (Dev) Publishing SmartStore.Web from within Visual Studio now deploys the project correctly. No need to execute ClickToBuild.cmd anymore. Just publish directly to any target, including Azure.
* Localization & SEO: language switcher now takes language specific SEO slugs into account when building links
* Smarter import of plugin resource files with graceful fallbacks (de-DE > de > de-* > en-US > en > en-* > *)
* (Perf) Faster language resource file import
* Exports the product detail link including the attribute query string when exporting attribute combinations
* #918 Compare products: Display base price information
* Export email attachments needs to be stored in database because the temp file may not exist anymore when sending the email
* #913 Use HTML5 Input types (tel, email)
* Added paging to frontend order list
* Added paging to backend checkout attribute list
* #977 Show PAngV base/delivery amount also
* Updated LiveEngage LiveChat plugin

### Bugfixes
* TaskScheduler could fail polling when primary store url is an external IP address
* Fixed ajax cache issue when saving payment or shipping restrictions. Internet Explorer showed the old data state (before storage).
* "The provider failed at the Execute method: Member 'CurrentValues' cannot be called for the entity of type 'Product'" when exporting product attribute combinations
* Bundles without selected attributes could throw an exception on product detail page
* GMC feed did not export the product type and Billiger did not export shop_cat (category path)
* The error message of a payment provider when capturing a payment was not displayed
* Adding new shipping method threw an exception
* Attribute Values: Assigning IsPreselected to more than one value causes an error
* BizImporter: fixed redirection bug when default file extension in .biz wasn't .html
* Fixed: Export deployment emails were always send manually
* Manually notifying gift card recipient threw an exception
* Loading shipping by weight grid failed under SQL CE
* #949 Import: ProcessSlugs does not process explicitly specified "SeName", if product name did not change in an update scenario
* Customer import: Creates customer role duplicates for inserted customers
* GMC feed does not generate the sale price if the sale price is set for a future date
* Mobile devices: Fixed "Uncaught Error: Syntax error, unrecognized expression: :nth-child"
* Download nor sample download were removable when editing product
* Copied product must not share sample download of source product. Could produce "The DELETE statement conflicted with the REFERENCE constraint 'FK_dbo.Product_dbo.Download_SampleDownloadId'".
* #921 Specification attribute options with single quotation marks are causing a Javascript error
* #971 Product is added to cart automatically if it has a non-required file upload attribute
* #973 Bundle item upload is nowhere linked
* Base price in product list ignored PriceDisplayType (catalog settings) and possibly displayed the wrong base price info
* Private messages: Fixes "No route in the route table matches the supplied values"
* Payone: Hash string incorrect for frontend API payments where the order has more than 9 products
* Export mail notification: Download link not working if SSL is enabled
* Discount rule has spent amount including sub total option can cause wrong discount calculation if the cart contains a product several times
* #986 File uploads possible through /content/filemanager/index.html


## SmartStore.NET 2.5

### Highlights
 * New import/export framework (profiles, filters, mapping, projections, scheduling, deployment... just everything!)
 * TaskScheduler: Rewritten from scratch to be suitable for Web Farms (including support for cron expressions)
 * Payment and shipping methods by customer roles
 * Restrict payment methods to countries
 * Restrict payment methods to shipping methods
 * Email attachment support for message templates
 * Attach order invoice PDF automatically to order notification emails
 * Overall performance increase
 * Lots of bug fixes

### New Features
* New export and import framework
* Import of customer and category data
* #141 Payment and shipping methods by customer roles
* #67 Restrict payment methods to countries
* #94 Restrict payment methods to shipping methods
* #747 Restrict payment methods by old versus new customer (plugin)
* #584 Email attachment support for message templates
* Attach order invoice PDF automatically to order notification emails
* #526 Min/Max amount option for which the payment method should be offered during checkout
* (Dev) New _SyncMapping_ service: enables easier entity synchronization with external systems
* (Dev) #792 ViewEngine: Enable vbhtml views per configuration
* (Dev) Plugin developers can now render child actions into a dynamically created special tab called 'Plugins' 
* #718 ShopConnector: Import option for "Published" and "Disable buy\wishlist button"
* #702 Facebook and Twitter external authentication suitable for multi-stores
* New scheduled task: Clear e-mail queue
* New scheduled task: Clear uploadeded transient media files
* #704 Make primary store currency suitable for multi-stores
* #727 Web-API: Option to deactivate TimestampOlderThanLastRequest validation
* #731 Web-API: Allow deletion and inserting of product category and manufacturer assignments
* #733 Option to set a display order for homepage products
* #607 HTML capable full description for payment methods displayed in checkout
* #732 Product list: Option to display the pre-selected price instead of the lowest price
* New payment provider for Offline Payment Plugin: Purchase Order Number
* #202 Implement option for product list 'default sort order'
* #360 Import & export product variant combinations
* #722 System > SEO Names: Implement editing of an UrlRecord
* Admin > System > System Info shows used application memory (RAM)
* Added option to make VATIN mandatory during customer registration
* #840 Activity log: Have option to exclude search engine activity
* #841 Activity log for deleting an order
* More settings to control creation of SEO names
* GMC feed: Supporting fields multipack, bundle, adult, energy efficiency class and custom label (0 to 4)
* #760 Setting to set a customer role for new registered users
* #800 Multi-store: Option to display all orders of all stores for customer in frontend
* #457 Added option to hide the default image for categories and products
* #451 Add message token for product shipping surcharge
* #436 Make %Order.Product(s)% token to link the product detail page and a add product thumbnail
* #339 Meta robots setting for page indexing of search engines
* PayPal: Option for API security protocol
* Product filter: Option to sort filter results by their display order rather than by number of matches
* Elmar Shopinfo: Option to export delivery time as availability
* #654 Place user agreement for downloadable files in checkout process
* #398 EU law: add 'revocation' form and revocation waiver for ESD
* #738 Implement download of pictures via URLs in product import
* Web-API: Bridge to import framework: uploading import files to import profile directory
* Setting to round down calculated reward points
* #695 Implement checkbox in checkout to let customers subscribe to newsletters
* #495 Implement option to search product detail description by default

### Improvements
* (Perf) Implemented static caches for URL aliases and localized properties. Increases app startup and request speed by up to 30%.
* (Perf) Significantly reduced number of database reads during product list rendering. Increases request speed by up to 10%.
* (Perf) Implemented 2nd level cache for infrequently changed entities. Increases request speed by up to 10%.
* TaskScheduler: Rewritten from scratch to be suitable for Web Farms
* TaskScheduler: Supports cron expressions to define task execution frequency
* TaskScheduler: Editing tasks does not require app restart anymore
* TaskScheduler: Enhanced UI
* #721 Message Queue: implemented "Delete all"
* #725 Prevent LowestProductPrice being 0
* #709 News feed produced invalid RSS feed. Added content:encoded. Added maximum news age setting for feed export.
* #735 Include SKUs of attribute combinations when filtering the backend product list
* Filter for homepage and published\unpublished products in backend product list
* Reduce database round trips initiated by price calculation
* Google Analytics: added support for mobile devices
* (Dev) TaskScheduler: Tasks can propagate progress info (percentage & message)
* (Dev) TaskScheduler: Cancellation request is sent to tasks on app shutdown
* ShippingByWeight & ShippingByTotal: Support for multiple zip ranges (comma separated)
* Two more options to handle customer numbers: display customer number in frontend & let customers enter their customer number if it's still empty
* #62 free shipping info on product detail page
* Display base price in CompactProductBox
* Automatically redirect to referrer after login
* #826 Image gallery: the viewport height was fixed to 300 px, but now respects MediaSettings > ImageSize.
* #249 Make UI editor for 'SeoSettings.ExtraRobotsDisallows'
* Debitoor: Customer VAT number not transmitted anymore because it appears on the Debitoor invoice.
* #778 Web-API: Increase MaxExpansionDepth for using expand pathes
* #767 Remove assignments to a grouped product if the grouped product is deleted
* #773 Reduce number of guest records created by search engine requests
* #791 Preselected attributes or attribute combinations should always be appended as querystring to product page links
* Simplified handling of SEO names
* URLs are not converted to lower case anymore
* Product grid sortable by name, price and created on
* #26 Display company or name in order list
* Added inline editing of country grid
* #790 Improved language editing
* #843 Implement a product picker
* #850 Use new product picker for selecting required products
* Trusted Shops: badge will be displayed in mobile themes, payment info link replaced compare list link in footer
* Product filter: Specification attributes are sorted by display order rather than alphabetically by name
* #856 Don't route topics which are excluded from sitemap
* #851 Replace reCAPTCHA with "I'm not a robot" CAPTCHA
* #713 Display gift card remaining amount in frontend order details and order messages
* #736 Render PayPal Express Button in minibasket
* PayPal: Support for partial refunds
* Offline credit card payment: Option to exclude credit card types

### Bugfixes
* #523 Redirecting to payment provider performed by core instead of plugin
* Preselected price was wrong for product attributes with multiple preselected values (same on product detail page)
* #749 Visual Studio 2015 compilation error: CS0009: Metadata file. SmartStore.Licensing.dll could not be opened -- Illegal tables in compressed metadata stream
* PayPal Express: fixed capture method
* #770 Resizing browser with product details page causes product image to disappear
* GMC feed: Availability value "available for order" deprecated
* Mobile: Shopping cart warnings weren't displayed to customers
* Tax provider and payment method were automatically activated when there were no active provider\method
* #784 Biz-Importer: Name of delivery time must not be imported empty
* #776 Preview: Manufacturer and Product in Multi Store
* #755 Some methods still loading all products in one go
* #796 Selected specification in product filter mask is displayed with default language (not localized)
* #805 Product filter is reset if 'product sorting' or 'view mode' or 'amount of displayed products per page' is changed
* Hide link to a topic page if it is limited to stores
* #829 Activity log: Searching by customer email out of function
* Product import: Store mappings were not applied when inserting new records
* Faulty permission handling in ajax grid actions (no message, infinite loading icon)
* Grouped products: Display order was not correct
* Deletion of a customer could delete all newsletter subscriptions
* PayPal: Fixed "The request was aborted: Could not create SSL/TLS secure channel."
* PayPal Express: Void and refund out of function ("The transaction id is not valid")
* Customer could not delete his avatar
* Facebook authentication: Email missing in verification
* Attribute with a product linkage throws exception if added to cart
* Number of products per product tag could be incorrect in a multi-store


## SmartStore.NET 2.2.2

### New Features
* SmartStore.NET User Guide
* #210 Implement multi-store support for import/export
* Added zip code to shipping by weight computation method
* Skrill payment plugin (distributed via Marketplace)
* (Dev) DevTool plugin: added option to display all widget zones in public store
* New options for manufacturer display on the homepage
* Added optional customer number field

### Improvements
* (Perf) several minor optimizations for faster app startup and page rendering
* UI: optimized image gallery widget (white background & nicer animations) + enhanced modal dialog fade animations
* (Soft) deletion of SEO slug supporting entities now also deletes the corresponding url records
* License checker now supports IDN mapping for domain names
* #716 Supporting of paged google-product data query for SQL-Server Compact Edition
* #648 Add hint for * at mandatory form fields at address creation
* Added link to imprint and disclaimer to footer in mobile theme 
* #521 Display bonus points in order export
* Updated GMC taxonomy files
* MsieJsEngine now is the default LESS script engine

### Bugfixes
* #694 Product variant attribute in product page should not be preselected implicitly
* Fixed: If currencies are limited to one for a multi-store, this currency should dominate the setting for the primary store currency
* #563 Scheduled Tasks: ensure that 'LastEndUtc' is ALWAYS set
* Topics grid: fixed 'maxJsonLength exceeded' error
* Debitoor: Fixed "The property named 'lines.0.productOrService' should be defined"
* Send currency code of primary store currency (not of working currency) to payment gateway
* #691 Product quantity not added to cart on mobile theme
* #186 Mobile: variant images do not refresh 
* #671 Bundle products: display base price according to applied discount 
* #619 Display base price according to applied tier price
* #726 PAngV: basket displays wrong base price when attribute price adjustment has been set 
* Weight adjustment of attributes weren't applied in shopping cart overview
* Shipping by weight calculates wrong surcharge if attribute combination prices are set
* Don't let database hooks call other hooks.
* There was no payment redirect if only one payment method is available in checkout


## SmartStore.NET 2.2.1

### New Features
* #582 GMC feed: Option to include\exclude a product
* #652 Web API: Support for file upload and multipart mime
* New option to display product thumbnails in instant search (NOTE: the feature can be enabled via _Configuration > Settings > Catalog Settings > Search_)
* #594 Option to prevent a message from being automatically sent
* Category filtering by store for backend category tree and list
* #554 Flag to indicate that a new payment notification (IPN) arrived for an order since last opening\editing of the order
* Setting whether and how to display sub-categories on a category page (hide, above products, bottom of page)
* Debitoor: Extend option "Book invoice if paid" to also (optionally) mail the invoice as a PDF
* #508 Limit country settings to stores
* #205 Add multistore support for forums

### Improvements
* PDF converter: first init is much faster now
* Localized properties for forums and forum groups

### Bugfixes
* PDF converter: fixed _QFont::setPixelSize_ bug
* Fixed System.NullReferenceException at admin/order/pdfpackagingslips
* #660 Don't show shipping cost hint in product detail page if product has free shipping

## SmartStore.NET 2.2.0

### New Features
* New mobile theme: _MobileLight_ (a light variant of the default mobile theme)
* Localization: in a multi-language environment missing language resources fall back to default language's resources (instead of returning the ugly resource key)
* Quantity unit management
* #428 New option to override global category list view type
* #672 New option to automatically create mutual associations for cross sell and related products
* #485 Enable shop admin to change creation date of a blog entry
* #258 Implement email validation in checkout
* New option to specify the maximum number of displayed filter items
* New option to specify whether all filter groups should be displayed expanded
* #459 New field to specify html tag for page titles on widget level
* Added _shrink database_ to backend UI
* #588 Alternative localized category name used as title for category pages
* #592 Add a second category description displayed beyond products on category page
* Promotion feeds now export the preselected price (including price adjustments and overrides)
* Implement 'free shipping threshold' for feed creation
* _Billiger_ feed: Implement new field for shipping costs to austria
* (Developer) Added `BeginTransaction()` and `UseTransaction()`  methods to `IDbContext`

### Improvements
* (Perf) product list rendering up to 10x (!) faster now (depends on page size and view mode)
* The data grid in the backend now preserves client state between requests (page, pageSize, sorting, filtering etc.)
* Excel Import & Export: much lower memory consumption and increased performance
* Moving pictures from DB to FS or vice versa is lightning fast now, consumes much lower memory and is encapsulated in a transaction which ensures reliable rollback after failure. Plus the database gets automatically shrinked after moving to FS.
* Feed plugins: product query now paged to reduce memory payload
* #589 Backend product list: add filter for products with "no category mapping" and "no manufacturer mapping"
* Minor UI tweaks in checkout process
* Payone: CC-Check via client API, not via Server API (requires PCI certification)
* #189 Allow deletion of multiple reviews
* #622 UI: Redesign table in Sales > Orders > Order > Tab Products
* #625 Bundles can be ordered if an attribute combination of a bundle item is not available
* #666 Export addresses in customer export
* New shopping cart setting ShowItemsFromWishlistToCartButton
* XML product export now is paged and more data gets exported
* #560 Return requests: Add fields for last update, last update of requested action and general notes
* #626 Add `IsActive` flag for checkout attributes

### Bugfixes
* Instant search box did not display all results when SQL Fulltext Search was enabled
* Print order as PDF redirected to login although the admin already was logged in 
* #621 PDF Order: does not take overridden attribute combination price into account (in order line)
* _Amazon payments_: Declined authorization IPN did not void the payment status
* Fixed „Payment method couldn't be loaded“ when order amount is zero
* #598 Wrong input parameter name for ReturnRequestSubmit
* #557 Localize MVC validation strings
* Fixed rare bug "The length of the string exceeds the value set on the maxJsonLength property" (Controller: Order, Action: OrderNotesSelect)
* _Debitoor_: Adding order notes can result in infinite order update event loop with thousands of order notes
* Tax rates persisted on order item level to avoid rounding issues (required for Debitoor, Accarda and Payone)
* Hide additional shipping surcharge when display prices permission is not granted
* Fixed "Adding a relationship with an entity which is in the Deleted state is not allowed" when adding bundles to cart
* Fixed price calculation of multiple bundles issue
* Fixed auto add required products for bundle items
* Fixes #641: Protocol in sitemap should be HTTPS when `ForceSslForAllPages` is true
* #640 Do not display shipping infos for grouped products in product lists
* #634 Bundle price in list should not differ from price in details if there is only one bundle item with one attribute
* Do not copy associated product when copying a bundle product
* Null DeliveryTimeId when deleting products. Otherwise deleted products can prevent deletion of delivery times.
* Fixed: Product with inactive attribute combination could be moved to wishlist, copied to cart and be purchased
* Mobile Theme: Show prices according to selected variant value, display of deliverytime
* Bundles: Display base price according to Catalog Setting > Product Detail > Display Base Price

## SmartStore.NET 2.1.1

### New Features
* Html to PDF converter: PDF documents are created from regular HTML templates now, therefore radically simplifying PDF output customization.
* Html widgets: added option to create a wrapper around widget content
* SEO: added new settings `Canonical host name rule`. Enforces permanent redirection to a single domain name for a better page rank (e.g. myshop.com > www.myshop.com or vice versa)
* SEO: added support for `<link hreflang="..." ... />` in multi-language stores. The tags are automatically rendered along with the language switcher.
* (Developer) Implemented new HtmlHelper extension `AddLinkPart`: registers `<link>` tags which should be rendered in page's head section
* (Developer) Implemented new HtmlHelper extension `AddCustomHeadParts`: registers whatever head (meta) tag you wish
* (Developer) Added `SmartUrlRoutingModule`, which can pass static files to `UrlRoutingModule` if desired (e.g. used by MiniProfiler). This way static files can be handled by regular actions or filters, without polluting web.config.
* New payment plugin "Payone"
* Option to set a delivery time for products available for order with stock quantity < 1
* Option to disable product reviews on product detail page
* Option to supress display of sub category picture links

### Improvements
* (Perf) Faster application warmup
* (Perf) Faster product list rendering
* Reworked routing: removed static file security barrier again (caused too much problems)
* #545 Made all (applicable) settings multi-store-enabled
* #579 Make all relative urls absolute prior sending email
* The display order of attribute values are considered in the sorting of backend's attribute combination grid
* Optimized error handling and redesigned error pages
* Removed `PageNotFound` topic. Text is a simple locale resource now.
* PayPal settings are multi-store-enabled
* #555 Product edit: Improve category select box. Add history (last x selected items) above all others.
* #510 Payment plugins: Qualify configuration(s) for multistores
* #556 A negative value should be possible for additional payment fees
* Dashboard: Order items linked with order list
* Security: Missing http-only flag for some cookies set

### Bugfixes
* PayPal Express: corrected basket transmission by eliminating tax transmission
* Fixed password recovery redirection
* #552 Left navbar should stay expanded on product detail pages
* #538 Specification attribute labels in product filter mask are not displayed localized
* #540 Amazon payments: Multistore configuration might be lost if "All stores" are left empty
* #532 Amazon payments: Reflect refunds made at amazon seller central when using data polling
* #577 Exception thrown because of missing TaxRate table when opening tax by region provider configuration
* Added IIS rewrite rule for `/productreviews/{id}` > `/product/reviews/{id}`
* Email a friend redirects to "not found"
* #567 Products marked as 'Featured' should also be included in regular lists
* Fixed some missing company information in order to PDF export
* #583 Fixed "The property rate with the value x is malformed" when creating products
* Fixed ignored discount and tier price when product has attribute combination price
* PayPal Standard provider now using shipping rather than billing address if shipping is required
* Amazon payments: Order wasn't found if the capturing\refunding took place at Amazon Seller Central and the notification came through IPN


## SmartStore.NET 2.1.0

### New Features
* (Developer) *Overhauled plugin architecture*:
	- Plugins are regular MVC areas now
	- No embedded views anymore. Views get copied to the deployment folder
	- No cumbersome return View("Very.Long.View.Identifier") anymore
	- Views in plugin source folders can be edited during debug. The changes are reflected without plugin recompilation.
* (Developer) *Theme inheritance*: create derived child themes with minimum effort by overriding only small parts (static files and even variables).
* *Preview Mode*: virtually test themes and stores more easily
* New payment plugin *Pay with Amazon*
* Support for *hierarchical SEO slugs*, e.g.: samsung/galaxy/s5/16gb/white
* (Developer) Model binding for plugin tab views: models from plugin tabs get automatically materialized and bound to TabbableModel.CustomProperties[MyKey]. Extended the SmartModelBinder for this.
* (Developer) New event _ModelBoundEvent_. Consume this in plugins to persist plugin specific models.
* (Admin) Added _GMC_ tab to product edit view. This is more a coding example for the above stuff rather than a new feature.
* (Developer) Implemented _AdminThemedAttribute_: instructs the view engine to additionally search in the admin area for views. Very useful in larger plugin projects.
* (Developer) Enhanced _IMenuProvider_: menu items can now be injected to the public catalog menu
* (Developer) Implemented _IWidgetProvider_. Allows request scoped registration of action routes to be injectable into widget zones. Perfect for custom action filters.
* (Developer) Simple widgets: the model of the parent action view context now gets passed to a widget.
* (Developer) New IoC method ContainerManager.InjectProperties()
* Implemented support for EU VAT regulation for digital goods (2008/8/EG directive)
* Implemented Media Manager for HTML editor (including file upload)
* Added _CDN_ setting to store configuration. Allows static files to be served through a content delivery network, e.g. cloudfront. (contributed by 'normalc')
* #393 Web API: Implement OData actions for simpler working with product attributes
* #431 Web API: Add support for localized properties
* ShippingByWeight: new settings to configure a small quantity surcharge
* #216 Better return request support
* #90 Directly set order status to completed
* #413 Orders: Add a PDF export\download of selected orders
* #69 Award reward points for product reviews
* #164 Add multistore support for polls
* #170 Multistore support for Newsletters
* #266 Update Pending Order in Admin Panel
* #331 Show CommentBox in checkout (optional) 
* Option to turn off the filter for products in categories
* Export/Import was enabeled to work with localized values for name, short description and long description
* Added two new themes 'Alpha Blue' and 'Alpha Black'   

### Improvements
* New backend design and cleaner frontend theme
* Replaced TinyMCE HTML editor with CKeditor
* Simplified checkout process by combining payment method and info pages
* (Perf) Lower memory consumption
* (Perf) (Developer) Client Dependency updates
	- jQuery 1.8.3 > 2.1.1 (although the backend is still using v1.8.3 because of the Telerik components)
	- FontAwesome 3 > 4.1
	- Modernizr 2.5 > 2.7.2
	- jQuery UI to 1.11
	- SearchBox uses Typeahead now instead of jQuery UI AutoComplete
	- Got rid of obsolete jQuery UI files (will remove this later completely)
* (UI) AJAXified product edit tab: all tabs other than the first one load on demand per AJAX
* (Developer)  Plugins can provide custom tabs more easily (now with on demand AJAX loading)
* Task Scheduler:
	- Can run tasks manually now (async)
	- Better UI
	- Shows last error
	- (Developer) Breaking change: New parameter _TaskExecutionContext_ for _ITask.Execute()_
* UI: TabStrips remember their last selected tab across page requests in an unobtrusive way (removed old selection code)
* Price formatting: the DisplayLocale's FormatProvider was not applied when _CustomFormatting_ was specified for Currency
* Admin: Specification attributes are now sorted by DisplayOrder, THEN BY Name
* Admin: Replaced DatePicker with DateTimePicker control
* (Perf) significantly increased excel import performance... again ;-)
* (Perf) significantly increased excel export performance and optimized memory usage
* (Perf) SEO sitemap is being cached now, resulting in fast reponse times with larger catalogs
* (UI) optimized and reorganized product edit view a bit
* (Developer) MVC filter attributes are now Autofac injectable
* (Developer) Implemented _RunSync_ extension methods for _Func<Task>_ and _Func<Task<T>>_. A reliable way to execute async operations synchronously.
* (Developer) Refactored model creation for category navigation: it now incorporates _TreeNode<MenuItem>_, which enables plugin developers to alter the main menu with the event hook _NavigationModelBuilt_.
* (Developer) Added _user.less_ to Alpha theme for user defined css overrides and tweaks
* (Developer) Moved _PublicControllerBase_ to SmartStore.Web.Framework
* (Developer) Moved 'AdminControllerBase' to SmartStore.Web.Framework
* (Developer) Optimized Bundle handling
	- Html.Add[Script|CssFile]Parts() now can handle already bundled resources correctly (no attempt is made to bundle them, the bundle's virtual url is returned instead)
	- Made extra bundles for frequently used resources (like sequence js, fileupload, image gallery etc.). This way they always come compressed.
* #384 Web API: Inserting sluged recources like products require an URL record
* #382 Promotion feed plugins: Asynchronous feed creation, more options and improvements
* #433 GMC feed: Option to filter config records that have not been edited
* #362 Display 'from {0}' for products with variant attributes
* #239 Categories: Ask merchant if he want a cascading or a non cascading deletion
* HTML text collapser: Make it usable for all long texts
* #375 Implement SKU search for 'related products picker'
* #391 Admin: allow searching/filtering for specification attributes
* Removed _OpenID_ plugin from core
* Specification attribute values that are assigned to a product can be edited 

### Bugfixes
* Twitter Auth: fixed _SecurityTransparent_ error
* Facebook Auth: fixed _SecurityTransparent_ error
* OpenID Auth: fixed _SecurityTransparent_ error
* #376 Product filtering: Category price range filter not working anymore
* Return requests: Products to return won't be listed
* #372 Biz-Importer sometimes shows inactive tier prices
* PayPal Standard: Sending more localized values. Adjustment of net prices to avoid wrong PayPal total amount calculation.
* Globalization fix in plugin data grids: inline editing for decimal values did not take current culture into account
* #391 Show delivery time if out-of-stock orders are allowed by attribute combination
* CustomerRole > TaxDisplayType _Including VAT_ could not be saved
* Product.DisableBuyButton was never updated when the stock quantity has been increased (e.g. as a result of order canceling)
* Shipping.ByTotal: Fixed matching of rates by choosing the more specific over the common rate
* A grouped product only shows up to 12 associated products
* #405 Billiger feed: Wrong base price exported
* #437 Mobile devices: Cannot add a product to the cart when it is grouped
* PayPal Standard: Costs for checkout attributes were double charged
* Paging of return request grid did not work
* #428 Multiline checkout attributes aren't rendered correctly
* #434 Shipping.ByTotal: Make grid pageable
* #419 email account password hidden
* #424 Localize return reasons & return actions
* #479 Product filter: Wrong count of manufacturers if products of sub-categories are included
* #492 Ipayment credit card: Order notes are only created when the order exists
* #493 Postfinance plugin does not work if shopname includes a "umlaut"
* #237 Mobile theme: inactive attribute combinations should not be added to cart
* #178 Mobile theme doesn't display base price
* Ipayment: Capturing did not work because the security was not transmitted
* #405 "Reset Password" Link in Emails is wrong with SSL secured sites 
* #471 Checkout: Redirecting to external payment page could take a while. Clicking "Buy" button again might cancel the redirecting.
* Pricing not considered attribute combination prices for bundles with per item pricing


## SmartStore.NET 2.0.2

### Bugfixes
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

### Improvements
* #250 Implemented validation to theme configuration editing


## SmartStore.NET 2.0.1

### New Features
* #292 Allow specific price for attribute combinations
* Added image upload support to Summernote editor
* (Developer) Added WebApi client test tools to the solution (C# and JavaScript)

### Improvements
* Content slider slides can be filtered by store
* TinyMCE now shows advanced tab for images
* Updated BundleTransformer to the latest version 1.8.25
* Added JavaScriptEngineSwitcher.Msie (disabled by default). Useful in scenarios where target server has problems calling the V8 native libs.
* Updated some 3rd party libraries to their latest versions
* #320 Unavailable attribute combinations: better UI indication

### Bugfixes
* UI notifications get displayed repeatedly
* (Developer) Fixed Razor intellisense for plugin projects (NOTE: switch to 'PluginDev' configuration while editing plugin views, switch back when finished)


## SmartStore.NET 2.0.0

### Highlights
* RESTFul **WebApi**
* Highly optimized and **simplified product management**
* Product **Bundles**
* Considerably **faster app startup** and page processing
* New variant attribute type: **Linked Product**
* **Package upload** for plugins and themes
* Lightning **fast asynchronous Excel import** with progress indicators and detailed reports
* (Developer) Code-based Entity Framework database **migrations**

### New Features
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

### Improvements
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

### Bugfixes
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



## SmartStore.NET 1.2.1.0

### New Features
* Added option *Limit to current basket subtotal* to _HadSpentAmount_ discount rule
* Items in product lists can be labelled as _NEW_ for a configurable period of time
* Product templates can optionally display a _discount sign_ when discounts were applied
* Added the ability to set multiple *favicons* depending on stores and/or themes
* Plugin management: multiple plugins can now be (un)installed in one go
* Added a field for the HTML body id to store entity
* (Developer) New property 'ExtraData' for DiscountRequirement entity

### Bugfixes
* #110: PictureService.GetThumbLocalPath doesn't add picture to cache if it doesn't exist (this broke PDFService among others)
* #114: Runtime error occurs after new customer registration in the storefront
* #115: XML exports could fail because of invalid characters
* #121: Categories restricted to specific stores could also show up in other stores.
* #125: Widget Trusted Shops Customer Reviews can not be configured
* #127: Redirection to 404 failed with localized urls when shop ran under virtual application paths
* #128: _Switch language_ in store failed when SEO friendly urls were disabled
* #134: Fix mobile checkout
* #111: Send wishlist via e-mail doesn't work

### Improvements
* #97: Product numbers of attribute combinations could not be searched
* #120: Excel product import: The same product pictures were imported repeatedly which led to duplicate pictures.
* Updated _FontAwesome_ to version 3.2.1
* Minor CSS fixes
* Info pages for all Trusted Shops widgets 
* Better display and handling when choosing a flag for languages


## SmartStore.NET 1.2.0.0

### Highlights
 - Multi-store support
 - "Trusted Shops" plugins
 - Highly improved _SmartStore.biz Importer_ plugin
 - Add custom HTML content to pages
 - Performance optimization

### New Features
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

### Improvements
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

### Bugfixes
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



## SmartStore.NET 1.0.1.0

### Bug

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

### Improvement
    
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

### New Feature

    * [SMNET-14] - Brutto/Netto Preisanzeige über Kundengruppen steuerbar


