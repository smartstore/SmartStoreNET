truncate table [Log]

truncate table [QueuedEmailAttachment]
delete from [QueuedEmail]

truncate table [ShoppingCartItem]

truncate table [ShipmentItem]
delete from [Shipment]

truncate table [OrderNote]
delete from [OrderItem]
delete from [Order]


delete from [GenericAttribute] where KeyGroup like 'Order'
delete from [GenericAttribute] where KeyGroup like 'Customer' and EntityId >= 10

delete from [CustomerAddresses]

delete from [Customer_CustomerRole_Mapping] where Customer_Id >= 11
delete from [Customer] where ID >= 11

update [Customer] set ShippingAddress_Id = null, BillingAddress_Id = null

delete from [Address] 


