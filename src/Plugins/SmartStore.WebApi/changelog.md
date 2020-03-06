#Release Notes

##Web Api 4.0.0.0
###New Features
* #1809 Added a parameter to start an import after uploading import files.
* #1801 Added endpoints for ProductPicture, ProductCategory, ProductManufacturer to allow to update DisplayOrder.
* Added endpoints for CustomerRoleMapping.

##Web Api 3.1.5.1
###New Features
* Added endpoints for ProductSpecificationAttribute.

##Web Api 3.0.3.1
###New Features
* Added endpoint to get order in PDF format.
* Added endpoint to complete an order.
* Added endpoints for MeasureWeight and MeasureDimension.
###Bugfixes
* Fixed instance must not be null exception.

##Web Api 3.0.0.1
###New Features
* Added endpoints for blog post and blog comment.

##Web Api 2.6.0.5
###New Features
* #1072 Add support for TaxCategory.
* #1074 Extend product image upload to allow updating of images.
* #1064 Deleting all product categories/manufacturers per product in one go.
* #1063 Adding product category/manufacturer ignores any other property like DisplayOrder.
* Added endpoint "Infos" for order and order item entity for additional information like aggregated data.
* Added endpoints for new entity ProductAttributeOption.
###Improvements
* #1073 Settings for maximum pagesize ($top) and maximum expansion depth ($expand).

##Web Api 2.6.0.4
###Improvements
* Filter options for user grid on configuration page

##Web Api 2.6.0.3
###Bugfixes
* Products.ManageAttributes with Synchronize set to true should only delete an attribute if it has no values and if its control type supports configuring values

##Web Api 2.6.0.2
###New Features
* Integration of Swagger documentation

##Web Api 2.6.0.1
###New Features
* #1002 Add support for addresses and customer roles navigation property of customer entity

##Web Api 2.5.0.1
###New Features
* Option to allow authentification without MD5 content hash

##Web Api 2.2.0.5
###New Features
* Bridge to import framework: uploading import files to import profile directory

##Web Api 2.2.0.4
###New Features
* Added OData endpoint for shipment items
* Added OData action to add a shipment to an order and to set it as shipped
###Improvements
* OData actions should return SingleResult<TEntity> (instead of entity instance) to let expand option be recognized

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