namespace SmartStore.Core.Security
{
    /// <summary>
    /// Provides system names of standard permissions.
    /// Usage: [Permission(PermissionSystemNames.Customer.Read)]
    /// </summary>
    public static class Permissions
    {
        public static class Catalog
        {
            public const string Self = "catalog";
            public const string DisplayPrice = "catalog.display-price";

            public static class Product
            {
                public const string Self = "catalog.product";
                public const string Read = "catalog.product.read";
                public const string Update = "catalog.product.update";
                public const string Create = "catalog.product.create";
                public const string Delete = "catalog.product.delete";
                public const string EditCategory = "catalog.product.edit-category";
                public const string EditManufacturer = "catalog.product.edit-manufacturer";
                public const string EditAssociatedProduct = "catalog.product.edit-associated-product";
                public const string EditBundle = "catalog.product.edit-bundle";
                public const string EditAttribute = "catalog.product.edit-attribute";
                public const string EditVariant = "catalog.product.edit-variant";
                public const string EditPromotion = "catalog.product.edit-promotion";
                public const string EditPicture = "catalog.product.edit-picture";
                public const string EditTag = "catalog.product.edit-tag";
                public const string EditTierPrice = "catalog.product.edit-tier-price";
            }

            public static class ProductReview
            {
                public const string Self = "catalog.product-review";
                public const string Read = "catalog.product-review.read";
                public const string Update = "catalog.product-review.update";
                public const string Create = "catalog.product-review.create";
                public const string Delete = "catalog.product-review.delete";
                public const string Approve = "catalog.product-review.Approve";
            }

            public static class Category
            {
                public const string Self = "catalog.category";
                public const string Read = "catalog.category.read";
                public const string Update = "catalog.category.update";
                public const string Create = "catalog.category.create";
                public const string Delete = "catalog.category.delete";
            }

            public static class Manufacturer
            {
                public const string Self = "catalog.manufacturer";
                public const string Read = "catalog.manufacturer.read";
                public const string Update = "catalog.manufacturer.update";
                public const string Create = "catalog.manufacturer.create";
                public const string Delete = "catalog.manufacturer.delete";
            }

            public static class Variant
            {
                public const string Self = "catalog.variant";
                public const string Read = "catalog.variant.read";
                public const string Update = "catalog.variant.update";
                public const string Create = "catalog.variant.create";
                public const string Delete = "catalog.variant.delete";
                public const string EditSet = "catalog.variant.edit-option-set";
            }

            public static class Attribute
            {
                public const string Self = "catalog.attribute";
                public const string Read = "catalog.attribute.read";
                public const string Update = "catalog.attribute.update";
                public const string Create = "catalog.attribute.create";
                public const string Delete = "catalog.attribute.delete";
            }
        }

        public static class Customer
        {
            public const string Self = "customer";
            public const string Read = "customer.read";
            public const string Update = "customer.update";
            public const string Create = "customer.create";
            public const string Delete = "customer.delete";
            public const string Impersonate = "customer.impersonate";

            public static class Role
            {
                public const string Self = "customer.role";
                public const string Read = "customer.role.read";
                public const string Update = "customer.role.update";
                public const string Create = "customer.role.create";
                public const string Delete = "customer.role.delete";
            }
        }

        public static class Order
        {
            public const string Self = "order";
            public const string Read = "order.read";
            public const string Update = "order.update";
            public const string Create = "order.create";
            public const string Delete = "order.delete";
            public const string EditItem = "order.edit-order-item";
            public const string EditShipment = "order.edit-shipment";

            public static class GiftCard
            {
                public const string Self = "order.gift-card";
                public const string Read = "order.gift-card.read";
                public const string Update = "order.gift-card.update";
                public const string Create = "order.gift-card.create";
                public const string Delete = "order.gift-card.delete";
                public const string Notify = "order.gift-card.notify";
            }

            public static class ReturnRequest
            {
                public const string Self = "order.return-request";
                public const string Read = "order.return-request.read";
                public const string Update = "order.return-request.update";
                public const string Create = "order.return-request.create";
                public const string Delete = "order.return-request.delete";
                public const string Accept = "order.return-request.accept";
            }
        }

        public static class Promotion
        {
            public const string Self = "promotion";

            public static class Affiliate
            {
                public const string Self = "promotion.affiliate";
                public const string Read = "promotion.affiliate.read";
                public const string Update = "promotion.affiliate.update";
                public const string Create = "promotion.affiliate.create";
                public const string Delete = "promotion.affiliate.delete";
            }

            public static class Campaign
            {
                public const string Self = "promotion.campaign";
                public const string Read = "promotion.campaign.read";
                public const string Update = "promotion.campaign.update";
                public const string Create = "promotion.campaign.create";
                public const string Delete = "promotion.campaign.delete";
            }

            public static class Discount
            {
                public const string Self = "promotion.discount";
                public const string Read = "promotion.discount.read";
                public const string Update = "promotion.discount.update";
                public const string Create = "promotion.discount.create";
                public const string Delete = "promotion.discount.delete";
            }

            public static class Newsletter
            {
                public const string Self = "promotion.newsletter";
                public const string Read = "promotion.newsletter.read";
                public const string Update = "promotion.newsletter.update";
                public const string Create = "promotion.newsletter.create";
                public const string Delete = "promotion.newsletter.delete";
            }
        }

        public static class Cms
        {
            public const string Self = "cms";

            public static class Poll
            {
                public const string Self = "cms.poll";
                public const string Read = "cms.poll.read";
                public const string Update = "cms.poll.update";
                public const string Create = "cms.poll.create";
                public const string Delete = "cms.poll.delete";
                public const string EditAnswer = "cms.poll.edit-answer";
            }

            public static class News
            {
                public const string Self = "cms.news";
                public const string Read = "cms.news.read";
                public const string Update = "cms.news.update";
                public const string Create = "cms.news.create";
                public const string Delete = "cms.news.delete";
            }

            public static class Blog
            {
                public const string Self = "cms.blog";
                public const string Read = "cms.blog.read";
                public const string Update = "cms.blog.update";
                public const string Create = "cms.blog.create";
                public const string Delete = "cms.blog.delete";
            }

            public static class Widget
            {
                public const string Self = "cms.widget";
                public const string Read = "cms.widget.read";
                public const string Update = "cms.widget.update";
                public const string Activate = "cms.widget.activate";
            }

            public static class Topic
            {
                public const string Self = "cms.topic";
                public const string Read = "cms.topic.read";
                public const string Update = "cms.topic.update";
                public const string Create = "cms.topic.create";
                public const string Delete = "cms.topic.delete";
            }

            public static class Menu
            {
                public const string Self = "cms.menu";
                public const string Read = "cms.menu.read";
                public const string Update = "cms.menu.update";
                public const string Create = "cms.menu.create";
                public const string Delete = "cms.menu.delete";
            }

            public static class Forum
            {
                public const string Self = "cms.forum";
                public const string Read = "cms.forum.read";
                public const string Update = "cms.forum.update";
                public const string Create = "cms.forum.create";
                public const string Delete = "cms.forum.delete";
            }

            public static class MessageTemplate
            {
                public const string Self = "cms.message-template";
                public const string Read = "cms.message-template.read";
                public const string Update = "cms.message-template.update";
                public const string Create = "cms.message-template.create";
                public const string Delete = "cms.message-template.delete";
            }
        }

        public static class Configuration
        {
            public const string Self = "configuration";

            public static class Country
            {
                public const string Self = "configuration.country";
                public const string Read = "configuration.country.read";
                public const string Update = "configuration.country.update";
                public const string Create = "configuration.country.create";
                public const string Delete = "configuration.country.delete";
            }

            public static class Language
            {
                public const string Self = "configuration.language";
                public const string Read = "configuration.language.read";
                public const string Update = "configuration.language.update";
                public const string Create = "configuration.language.create";
                public const string Delete = "configuration.language.delete";
            }

            public static class Setting
            {
                public const string Self = "configuration.setting";
                public const string Read = "configuration.setting.read";
                public const string Update = "configuration.setting.update";
                public const string Create = "configuration.setting.create";
                public const string Delete = "configuration.setting.delete";
            }

            public static class PaymentMethod
            {
                public const string Self = "configuration.payment-method";
                public const string Read = "configuration.payment-method.read";
                public const string Update = "configuration.payment-method.update";
                public const string Activate = "configuration.payment-method.activate";
            }

            public static class Authentication
            {
                public const string Self = "configuration.authentication";
                public const string Read = "configuration.authentication.read";
                public const string Update = "configuration.authentication.update";
                public const string Activate = "configuration.authentication.activate";
            }

            public static class Currency
            {
                public const string Self = "configuration.currency";
                public const string Read = "configuration.currency.read";
                public const string Update = "configuration.currency.update";
                public const string Create = "configuration.currency.create";
                public const string Delete = "configuration.currency.delete";
            }

            public static class DeliveryTime
            {
                public const string Self = "configuration.delivery-time";
                public const string Read = "configuration.delivery-time.read";
                public const string Update = "configuration.delivery-time.update";
                public const string Create = "configuration.delivery-time.create";
                public const string Delete = "configuration.delivery-time.delete";
            }

            public static class Theme
            {
                public const string Self = "configuration.theme";
                public const string Read = "configuration.theme.read";
                public const string Update = "configuration.theme.update";
                public const string Upload = "configuration.theme.upload";
            }

            public static class Measure
            {
                public const string Self = "configuration.measure";
                public const string Read = "configuration.measure.read";
                public const string Update = "configuration.measure.update";
                public const string Create = "configuration.measure.create";
                public const string Delete = "configuration.measure.delete";
            }

            public static class ActivityLog
            {
                public const string Self = "configuration.activity-log";
                public const string Read = "configuration.activity-log.read";
                public const string Update = "configuration.activity-log.update";
            }

            public static class Acl
            {
                public const string Self = "configuration.acl";
                public const string Read = "configuration.acl.read";
                public const string Update = "configuration.acl.update";
            }

            public static class EmailAccount
            {
                public const string Self = "configuration.email-account";
                public const string Read = "configuration.email-account.read";
                public const string Update = "configuration.email-account.update";
                public const string Create = "configuration.email-account.create";
                public const string Delete = "configuration.email-account.delete";
            }

            public static class Store
            {
                public const string Self = "configuration.store";
                public const string Read = "configuration.store.read";
                public const string Update = "configuration.store.update";
                public const string Create = "configuration.store.create";
                public const string Delete = "configuration.store.delete";
            }

            public static class Shipping
            {
                public const string Self = "configuration.shipping";
                public const string Read = "configuration.shipping.read";
                public const string Update = "configuration.shipping.update";
                public const string Create = "configuration.shipping.create";
                public const string Delete = "configuration.shipping.delete";
                public const string Activate = "configuration.shipping.activate";
            }

            public static class Tax
            {
                public const string Self = "configuration.tax";
                public const string Read = "configuration.tax.read";
                public const string Update = "configuration.tax.update";
                public const string Create = "configuration.tax.create";
                public const string Delete = "configuration.tax.delete";
                public const string Activate = "configuration.tax.activate";
            }

            public static class Plugin
            {
                public const string Self = "configuration.plugin";
                public const string Read = "configuration.plugin.read";
                public const string Update = "configuration.plugin.update";
                public const string Upload = "configuration.plugin.upload";
                public const string Install = "configuration.plugin.install";
                public const string License = "configuration.plugin.license";
            }

            public static class Export
            {
                public const string Self = "configuration.export";
                public const string Read = "configuration.export.read";
                public const string Update = "configuration.export.update";
                public const string Create = "configuration.export.create";
                public const string Delete = "configuration.export.delete";
                public const string Execute = "configuration.export.execute";
            }

            public static class Import
            {
                public const string Self = "configuration.import";
                public const string Read = "configuration.import.read";
                public const string Update = "configuration.import.update";
                public const string Create = "configuration.import.create";
                public const string Delete = "configuration.import.delete";
                public const string Execute = "configuration.import.execute";
            }
        }

        public static class System
        {
            public const string Self = "system";
            public const string AccessBackend = "system.access-backend";
            public const string AccessShop = "system.access-shop";

            public static class Log
            {
                public const string Self = "system.log";
                public const string Read = "system.log.read";
                public const string Delete = "system.log.delete";
            }

            public static class Message
            {
                public const string Self = "system.message";
                public const string Read = "system.message.read";
                public const string Update = "system.message.update";
                public const string Create = "system.message.create";
                public const string Delete = "system.message.delete";
                public const string Send = "system.message.send";
            }

            public static class Maintenance
            {
                public const string Self = "system.maintenance";
                public const string Execute = "system.maintenance.execute";
            }

            public static class ScheduleTask
            {
                public const string Self = "system.schedule-task";
                public const string Read = "system.schedule-task.read";
                public const string Update = "system.schedule-task.update";
                public const string Delete = "system.schedule-task.delete";
                public const string Execute = "system.schedule-task.execute";
            }

            public static class UrlRecord
            {
                public const string Self = "system.url-record";
                public const string Read = "system.url-record.read";
                public const string Update = "system.url-record.update";
                public const string Delete = "system.url-record.delete";
            }
        }

        public static class Cart
        {
            public const string Self = "cart";
            public const string AccessShoppingCart = "cart.access-shopping-cart";
            public const string AccessWishlist = "cart.access-wishlist";

            public static class CheckoutAttribute
            {
                public const string Self = "cart.checkout-attribute";
                public const string Read = "cart.checkout-attribute.read";
                public const string Update = "cart.checkout-attribute.update";
                public const string Create = "cart.checkout-attribute.create";
                public const string Delete = "cart.checkout-attribute.delete";
            }
        }

        public static class Media
        {
            public const string Self = "media";
            public const string Upload = "media.upload";

            public static class Download
            {
                public const string Self = "media.download";
                public const string Read = "media.download.read";
                public const string Update = "media.download.update";
                public const string Create = "media.download.create";
                public const string Delete = "media.download.delete";
            }
        }
    }
}
