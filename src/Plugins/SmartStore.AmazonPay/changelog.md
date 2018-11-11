#Release Notes

##Pay with Amazon 2.2.0.3
###Improvements
* Added message token %SmartStore.AmazonPay.BillingAddressMessageNote% for billing address note in order placed customer notification

##Pay with Amazon 2.2.0.2
###Bugfixes
* Send currency code of primary store currency (not of working currency) to payment gateway

##Pay with Amazon 2.2.0.1
### New Features
* Supports order list label for new incoming IPNs

##Pay with Amazon 1.20
###Bugfixes
* PlatformID must be .NET ID not merchant ID. PlatformID needs to be identical for all orders

##Pay with Amazon 1.19
###Bugfixes
* Declined authorization IPN did not void the payment status

##Pay with Amazon 1.18
###Bugfixes
* Order wasn't found if the capturing\refunding took place at Amazon Seller Central and the notification came through IPN

##Pay with Amazon 1.17
###Improvements
* Amazon payments review

##Pay with Amazon 1.16
###Bugfixes
* Reflect refunds made at amazon seller central when using data polling

##Pay with Amazon 1.15
###Bugfixes
* Multistore configuration might be lost if "All stores" are left empty

##Pay with Amazon 1.14
###Bugfixes
* Data polling did not reflect the transaction status correctly if the action took place at amazon seller central

##Pay with Amazon 1.13
###Bugfixes
* Sometimes shipping method restrictions were not applied
* Shipping tab in checkout may show "Shipping address is not set" even if set

##Pay with Amazon 1.12
###Improvements
* Created this changelog
* Changed string „Bezahlen über Amazon“ into „Bezahlen mit Amazon“

