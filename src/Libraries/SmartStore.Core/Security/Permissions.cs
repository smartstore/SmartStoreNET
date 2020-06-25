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
            public const string DisplayPrice = "catalog.displayprice";

            public static class Product
            {
                public const string Self = "catalog.product";
                public const string Read = "catalog.product.read";
                public const string Update = "catalog.product.update";
                public const string Create = "catalog.product.create";
                public const string Delete = "catalog.product.delete";
                public const string EditCategory = "catalog.product.editcategory";
                public const string EditManufacturer = "catalog.product.editmanufacturer";
                public const string EditAssociatedProduct = "catalog.product.editassociatedproduct";
                public const string EditBundle = "catalog.product.editbundle";
                public const string EditAttribute = "catalog.product.editattribute";
                public const string EditVariant = "catalog.product.editvariant";
                public const string EditPromotion = "catalog.product.editpromotion";
                public const string EditPicture = "catalog.product.editpicture";
                public const string EditTag = "catalog.product.edittag";
                public const string EditTierPrice = "catalog.product.edittierprice";
            }

            public static class ProductReview
            {
                public const string Self = "catalog.productreview";
                public const string Read = "catalog.productreview.read";
                public const string Update = "catalog.productreview.update";
                public const string Create = "catalog.productreview.create";
                public const string Delete = "catalog.productreview.delete";
                public const string Approve = "catalog.productreview.Approve";
            }

            public static class Category
            {
                public const string Self = "catalog.category";
                public const string Read = "catalog.category.read";
                public const string Update = "catalog.category.update";
                public const string Create = "catalog.category.create";
                public const string Delete = "catalog.category.delete";
                public const string EditProduct = "catalog.category.editproduct";
            }

            public static class Manufacturer
            {
                public const string Self = "catalog.manufacturer";
                public const string Read = "catalog.manufacturer.read";
                public const string Update = "catalog.manufacturer.update";
                public const string Create = "catalog.manufacturer.create";
                public const string Delete = "catalog.manufacturer.delete";
                public const string EditProduct = "catalog.manufacturer.editproduct";
            }

            public static class Variant
            {
                public const string Self = "catalog.variant";
                public const string Read = "catalog.variant.read";
                public const string Update = "catalog.variant.update";
                public const string Create = "catalog.variant.create";
                public const string Delete = "catalog.variant.delete";
                public const string EditSet = "catalog.variant.editoptionset";
            }

            public static class Attribute
            {
                public const string Self = "catalog.attribute";
                public const string Read = "catalog.attribute.read";
                public const string Update = "catalog.attribute.update";
                public const string Create = "catalog.attribute.create";
                public const string Delete = "catalog.attribute.delete";
                public const string EditOption = "catalog.attribute.editoption";
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
            public const string EditAddress = "customer.editaddress";
            public const string EditRole = "customer.editcustomerrole";
            public const string SendPm = "customer.sendpm";

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
            public const string EditItem = "order.editorderitem";
            public const string EditShipment = "order.editshipment";
            public const string EditRecurringPayment = "order.editrecurringpayment";

            public static class GiftCard
            {
                public const string Self = "order.giftcard";
                public const string Read = "order.giftcard.read";
                public const string Update = "order.giftcard.update";
                public const string Create = "order.giftcard.create";
                public const string Delete = "order.giftcard.delete";
                public const string Notify = "order.giftcard.notify";
            }

            public static class ReturnRequest
            {
                public const string Self = "order.returnrequest";
                public const string Read = "order.returnrequest.read";
                public const string Update = "order.returnrequest.update";
                public const string Create = "order.returnrequest.create";
                public const string Delete = "order.returnrequest.delete";
                public const string Accept = "order.returnrequest.accept";
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
                public const string EditAnswer = "cms.poll.editanswer";
            }

            public static class News
            {
                public const string Self = "cms.news";
                public const string Read = "cms.news.read";
                public const string Update = "cms.news.update";
                public const string Create = "cms.news.create";
                public const string Delete = "cms.news.delete";
                public const string EditComment = "cms.news.editcomment";
            }

            public static class Blog
            {
                public const string Self = "cms.blog";
                public const string Read = "cms.blog.read";
                public const string Update = "cms.blog.update";
                public const string Create = "cms.blog.create";
                public const string Delete = "cms.blog.delete";
                public const string EditComment = "cms.blog.editcomment";
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
                public const string Self = "cms.messagetemplate";
                public const string Read = "cms.messagetemplate.read";
                public const string Update = "cms.messagetemplate.update";
                public const string Create = "cms.messagetemplate.create";
                public const string Delete = "cms.messagetemplate.delete";
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
                public const string EditResource = "configuration.language.editresource";
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
                public const string Self = "configuration.paymentmethod";
                public const string Read = "configuration.paymentmethod.read";
                public const string Update = "configuration.paymentmethod.update";
                public const string Activate = "configuration.paymentmethod.activate";
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
                public const string Self = "configuration.deliverytime";
                public const string Read = "configuration.deliverytime.read";
                public const string Update = "configuration.deliverytime.update";
                public const string Create = "configuration.deliverytime.create";
                public const string Delete = "configuration.deliverytime.delete";
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
                public const string Self = "configuration.activitylog";
                public const string Read = "configuration.activitylog.read";
                public const string Update = "configuration.activitylog.update";
                public const string Delete = "configuration.activitylog.delete";
            }

            public static class Acl
            {
                public const string Self = "configuration.acl";
                public const string Read = "configuration.acl.read";
                public const string Update = "configuration.acl.update";
            }

            public static class EmailAccount
            {
                public const string Self = "configuration.emailaccount";
                public const string Read = "configuration.emailaccount.read";
                public const string Update = "configuration.emailaccount.update";
                public const string Create = "configuration.emailaccount.create";
                public const string Delete = "configuration.emailaccount.delete";
            }

            public static class Store
            {
                public const string Self = "configuration.store";
                public const string Read = "configuration.store.read";
                public const string Update = "configuration.store.update";
                public const string Create = "configuration.store.create";
                public const string Delete = "configuration.store.delete";
                public const string ReadStats = "configuration.store.readstats";
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
            public const string AccessBackend = "system.accessbackend";
            public const string AccessShop = "system.accessshop";

            public static class Log
            {
                public const string Self = "system.log";
                public const string Read = "system.log.read";
                public const string Delete = "system.log.delete";
            }

            public static class Rule
            {
                public const string Self = "system.rule";
                public const string Read = "system.rule.read";
                public const string Update = "system.rule.update";
                public const string Create = "system.rule.create";
                public const string Delete = "system.rule.delete";
                public const string Execute = "system.rule.execute";
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
                public const string Read = "system.maintenance.read";
                public const string Execute = "system.maintenance.execute";
            }

            public static class ScheduleTask
            {
                public const string Self = "system.scheduletask";
                public const string Read = "system.scheduletask.read";
                public const string Update = "system.scheduletask.update";
                public const string Delete = "system.scheduletask.delete";
                public const string Execute = "system.scheduletask.execute";
            }

            public static class UrlRecord
            {
                public const string Self = "system.urlrecord";
                public const string Read = "system.urlrecord.read";
                public const string Update = "system.urlrecord.update";
                public const string Delete = "system.urlrecord.delete";
            }
        }

        public static class Cart
        {
            public const string Self = "cart";
            public const string Read = "cart.read";
            public const string AccessShoppingCart = "cart.accessshoppingcart";
            public const string AccessWishlist = "cart.accesswishlist";

            public static class CheckoutAttribute
            {
                public const string Self = "cart.checkoutattribute";
                public const string Read = "cart.checkoutattribute.read";
                public const string Update = "cart.checkoutattribute.update";
                public const string Create = "cart.checkoutattribute.create";
                public const string Delete = "cart.checkoutattribute.delete";
            }
        }

        public static class Media
        {
            public const string Self = "media";
            public const string Update = "media.update";
            public const string Delete = "media.delete";
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
