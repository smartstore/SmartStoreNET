#Release Notes

##Google Merchant Center (GMC) 3.0.0.1
###Improvements
* Perf: Added missing database indexes
* Export yes/no rather than true/false
###Bugfixes
* "identifier_exists" did not follow specification

##Google Merchant Center (GMC) 2.6.0.1
###Bugfixes
* Id should be unique when exporting attribute combinations as products
* No special price exported when the special price period was not specified

##Google Merchant Center (GMC) 2.5.0.1
###Bugfixes
* GMC feed does not generate the sale price if the sale price is set for a future date

##Google Merchant Center (GMC) 2.2.0.5
###New Features
* Supports GMC fields: multipack, bundle, adult, energy efficiency class and custom label (0 to 4)
* Export of availability date
###Improvements
* Removed "online_only" because it's not part of the GMC feed specification anymore

##Google Merchant Center (GMC) 2.2.0.4
###Improvements
* Supports new export framework
###Bugfixes
* Availability value "available for order" deprecated

##Google Merchant Center (GMC) 2.2.0.3
###Bugfixes
* Include\exclude option in product tab should initially be activated

##Google Merchant Center (GMC) 2.2.0.2
###Improvements
* Supporting of paged google-product data query for SQL-Server Compact Edition

##Google Merchant Center (GMC) 2.2.0.1
###New Features
* #582 Option to include\exclude a product

##Google Merchant Center (GMC) 2.2.0
###Improvements
* Paged product query to reduce memory payload