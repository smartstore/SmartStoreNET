#Release Notes

##Paypal 3.0.0.3
* PayPal PLUS: Fixed #1200 Invalid request if the order amount is zero. "Amount cannot be zero" still occurred.

##Paypal 3.0.0.2
###New Features
* PayPal Standard: New settings "UsePayPalAddress" and "IsShippingAddressRequired" to avoid payment rejection due to address validation.
###Bugfixes
* PayPal PLUS: Fixed HTTP 401 "Unauthorized" when calling PatchShipping.

##Paypal 2.6.0.7
###Bugfixes
* PayPal Express: Fixed net price issue.

##Paypal 2.6.0.6
###Bugfixes
* PayPal PLUS: Skip payment if cart total is zero.
* PayPal PLUS: Do not display payment wall if method is filtered
###Improvements
* PayPal PLUS: Log more information in case of a request failure.

##Paypal 2.6.0.5
###Bugfixes
* PayPal PLUS: Fixed "Cannot perform runtime binding on a null reference" when rendering the payment wall.

##Paypal 2.6.0.4
###Bugfixes
* PayPal PLUS: Excluding tax issue. Fixed "Transaction amount details (subtotal, tax, shipping) must add up to specified amount total".

##Paypal 2.6.0.3
###Bugfixes
* PayPal PLUS: Integration review through PayPal
* PayPal PLUS: Generic attribute caching problem. Fixed "Item amount must add up to specified amount subtotal (or total if amount details not specified)".

##PayPal 2.6.0.1
###Improvements
* Added PayPal partner attribution Id as request header

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