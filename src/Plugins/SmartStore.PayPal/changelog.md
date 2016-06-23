#Release Notes

##Paypal 2.5.0.2
###New Features
* PayPal PLUS payment provider

##Paypal 2.5.0.1
###Bugfixes
* PayPal Standard: The order amount transmitted to PayPal was wrong if gift cards or reward points were applied

##Paypal 2.2.0.4
###New Features
* Option for API security protocol
* Option to display express checkout button in mini shopping cart
* Support for partial refunds
* Option whether IPD may change the payment status of an order
###Bugfixes
* "The request was aborted: Could not create SSL/TLS secure channel." See https://devblog.paypal.com/upcoming-security-changes-notice/
* PayPal Express: Void and refund out of function ("The transaction id is not valid")

##Paypal 2.2.0.3
###New Features
* Option to add order note when order total validation fails

##PayPal 2.2.0.2
###Improvements
* Redirecting to payment provider performed by core instead of plugin

##Paypal 2.2.0.1
###New Features
* Supports order list label for new incoming IPNs

##Paypal 1.22
###Bugfixes
* PayPal Standard provider now using shipping rather than billing address if shipping is required

##Paypal 1.21
###Improvements
* Multistore configuration