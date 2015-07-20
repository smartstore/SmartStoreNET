#Release Notes

##Web Api 2.2.0.3
###New Features
* Added OData endpoint for payment method
* #727 Option to deactivate TimestampOlderThanLastRequest validation
* #731 Allow deletion and inserting of product category and manufacturer assignments
###Improvements
* Using header timestamp as last user request date rather than API server date

##Web Api 2.2.0.2
###Improvements
* WebApiAuthenticate attribute can be applied on methods too
* Product image upload requires manage catalog permission

##Web Api 2.2.0.1
###New Features
* #652 Support for file upload and multipart mime

##Web Api 1.32
###New Features
* #618 Add endpoint for quantity units

##Web Api 1.31
###Bugfixes
* WebApiController requires more permission checks

##Web Api 1.23
###New Features
* #431 Add support for localized properties

##Web Api 1.22
###New Features
* #393 Implement OData actions for simpler working with product attributes -> added ProductsController.ManageAttributes and ProductsController.CreateAttributeCombinations

##Web Api 1.21
###Improvements
* #384 Inserting sluged recources like products require an URL record
###Bugfixes
* PUT implementation was incomplete