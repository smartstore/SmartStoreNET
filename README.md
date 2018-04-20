## Overview

<p align="center">
  <img src="assets/smnet3-devices-sm.png" alt="SmartStore.NET Demoshop" />
</p>

SmartStore.NET is a free, open source, full-featured e-commerce solution for companies of any size. It is web standards compliant and incorporates the newest Microsoft web technology stack.

**SmartStore.NET includes all essential features to create multilingual and multi-currency stores** targeting desktop or mobile devices and enabling SEO optimized rich product catalogs with support for an unlimited number of products and categories, variants, bundles, datasheets, ESD, discounts, coupons and many more.

A comprehensive set of tools for CRM & CMS, sales, marketing, payment & shipping handling, etc. makes SmartStore.NET a powerful all-in-one solution fulfilling all your needs.

**SmartStore.NET delivers a beautiful and configurable shop front-end out-of-the-box**, built with a design approach on the highest level, including components like `Bootstrap 4`, `Sass` and others. The supplied theme _Flex_ is modern, clean and fully responsive, giving buyers the best possible shopping experience on any device. 

The state-of-the-art architecture of SmartStore.NET - with `ASP.NET 4.5` + `MVC 5`, `Entity Framework 6` and Domain Driven Design approach - makes it easy to extend, extremely flexible and essentially fun to work with ;-)

* **Website:** [http://www.smartstore.com/en/net](http://www.smartstore.com/en/net)
* **Forum:** [http://community.smartstore.com](http://community.smartstore.com)
* **Marketplace:** [http://community.smartstore.com/marketplace](http://community.smartstore.com/marketplace)
* **Translations:** [http://translate.smartstore.com/](http://translate.smartstore.com/)
* **Documentation:** [SmartStore.NET Documentation in English](http://docs.smartstore.com/display/SMNET/SmartStore.NET+Documentation+Home)

## Highlights

### Technology & Design

* State of the art architecture thanks to `ASP.NET 4.5`, `ASP.NET MVC 5`, `Entity Framework 6` and Domain Driven Design
* Easy to extend and extremely flexible thanks to modular design
* Highly scalable thanks to full page caching and web farm support 
* A powerful theming engine lets you create themes & skins with minimum effort thanks to theme inheritance
* Point&Click Theme configuration
* Highly professional search framework based on Lucene.NET, delivering ultra fast faceted search results
* Consistent and sophisticated use of modern components such as `jQuery`, `Bootstrap 4`, `Sass` & more in the front and back end.
* Easy shop management thanks to modern and clean UI

### Features

* Multi-Store support
* Unlimited number of products and categories
* Product Bundles
* RESTful WebApi
* Multi-language and RTL support
* Modern, clean, SEO-optimized and fully responsive Theme based on Bootstrap 4
* Ultra fast search framework with faceted search support
* Extremely scalable thanks to output caching, REDIS & Microsoft Azure support.
* Trusted Shops precertification
* 100% compliant with German jurisdiction
* Sales-, Customer- & Inventory-management
* Comprehensive CRM features
* Powerful Discount System
* Powerful layered navigation in the shop
* Numerous Payment and Shipping Providers and options
* Sophisticated Marketing & Promotion capabilities (Gift cards, Reward Points, discounts of any type and more)
* Reviews & Ratings
* CMS (Blog, Forum, custom pages & HTML content etc.)
* and many more...

## Project Status
SmartStore.NET V3.1.0 has been released on April 20, 2018. The highlights are:

* **Wallet \***: Enables full or partial order payment via credit account. Includes REST-Api.
* **[Liquid](https://github.com/Shopify/liquid/wiki/Liquid-for-Designers) template engine**: very flexible templating for e-mails and campaigns with autocompletion and syntax highlighting.
* **Cash Rounding**: define money rounding rules on currency and payment method level.
* **Modern, responsive backend**: migrated backend to Bootstrap 4, overhauled and improved the user interface.
* **Enhanced MegaMenu \***: virtual dropdowns for surplus top-level categories and brands.
* **RTL**: comprehensive RTL (Right-to-left) and bidi(rectional) support.
* **Amazon Pay**:
	* Supports merchants registered in the USA and Japan
	* External authentication via *Login with Amazon* button in shop frontend
	* Several improvements through the new *Login and pay with Amazon* services
* **Image processing**: new processing and caching strategy! Thumbnails are not created synchronously during the main request anymore, instead a new middleware route defers processing until an image is requested by any client.
* **TinyImage \***: scores ultra-high image compression rates (up to 80 %!) and enables WebP support.
* **UrlRewriter \***: define URL redirection rules in the backend using *mod_rewrite* notation.
* **Address formatting** templates by country
* **Language packs**: downloader & auto-importer for packages available online.
* ...and a lot more new features, enhancements and fixes

\* Commercial plugin



## Try it online

We have set up a live online demo for you so you are able to test SmartStore.NET without local installation. Get a first impression and test all available features in the front- and in the backend. Please keep in mind that the backend demo is shared and other testers can modify data at the same time.

* [**Frontend**](http://frontend.smartstore.net/en) (User: demo, PWD: 1234)
* [**Backend**](http://backend.smartstore.net/en/login) (User: demo, PWD: 1234)

## How to install

* Download the latest stable release from the download tab and unzip it to your web folder
* Setup a website in IIS and point the file directory to your unzipped folder
* Fire up your browser and follow the installation instructions
* Enjoy ;-)

NOTE: SmartStore.NET 3 requires [Visual C++ Redistributable für Visual Studio 2015](https://www.microsoft.com/en-US/download/details.aspx?id=52685) which is already pre-installed on most systems. If, nevertheless, it is missing on your web server, just download and execute the installer or ask your hosting provider to do that for you.

### System requirements

* IIS 7+
* ASP.NET 4.5+
* MS SQL Server 2008 Express (or higher) OR MS SQL Server Compact 4
* Visual C++ Redistributable für Visual Studio 2015 ([Download](https://www.microsoft.com/en-US/download/details.aspx?id=52685))
* Full Trust


## License

SmartStore.NET Community Edition is released under the [GPLv3 license](http://www.gnu.org/licenses/gpl-3.0.txt).
