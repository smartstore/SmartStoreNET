using System.Collections.Generic;
using SmartStore.Core.Domain.Logging;

namespace SmartStore.Data.Setup
{
    public abstract partial class InvariantSeedData
    {
        public IList<ActivityLogType> ActivityLogTypes()
        {
            var entities = new List<ActivityLogType>()
            {
				//admin area activities
				new ActivityLogType
                    {
                        SystemKeyword = "AddNewCategory",
                        Enabled = true,
                        Name = "Add a new category"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewCheckoutAttribute",
                        Enabled = true,
                        Name = "Add a new checkout attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewCustomer",
                        Enabled = true,
                        Name = "Add a new customer"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewCustomerRole",
                        Enabled = true,
                        Name = "Add a new customer role"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewDiscount",
                        Enabled = true,
                        Name = "Add a new discount"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewGiftCard",
                        Enabled = true,
                        Name = "Add a new gift card"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewManufacturer",
                        Enabled = true,
                        Name = "Add a new manufacturer"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewProduct",
                        Enabled = true,
                        Name = "Add a new product"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewProductAttribute",
                        Enabled = true,
                        Name = "Add a new product attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewSetting",
                        Enabled = true,
                        Name = "Add a new setting"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewSpecAttribute",
                        Enabled = true,
                        Name = "Add a new specification attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "AddNewWidget",
                        Enabled = true,
                        Name = "Add a new widget"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteCategory",
                        Enabled = true,
                        Name = "Delete category"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteCheckoutAttribute",
                        Enabled = true,
                        Name = "Delete a checkout attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteCustomer",
                        Enabled = true,
                        Name = "Delete a customer"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteCustomerRole",
                        Enabled = true,
                        Name = "Delete a customer role"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteDiscount",
                        Enabled = true,
                        Name = "Delete a discount"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteGiftCard",
                        Enabled = true,
                        Name = "Delete a gift card"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteManufacturer",
                        Enabled = true,
                        Name = "Delete a manufacturer"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteProduct",
                        Enabled = true,
                        Name = "Delete a product"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteProductAttribute",
                        Enabled = true,
                        Name = "Delete a product attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteReturnRequest",
                        Enabled = true,
                        Name = "Delete a return request"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteSetting",
                        Enabled = true,
                        Name = "Delete a setting"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteSpecAttribute",
                        Enabled = true,
                        Name = "Delete a specification attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "DeleteWidget",
                        Enabled = true,
                        Name = "Delete a widget"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditCategory",
                        Enabled = true,
                        Name = "Edit category"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditCheckoutAttribute",
                        Enabled = true,
                        Name = "Edit a checkout attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditCustomer",
                        Enabled = true,
                        Name = "Edit a customer"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditCustomerRole",
                        Enabled = true,
                        Name = "Edit a customer role"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditDiscount",
                        Enabled = true,
                        Name = "Edit a discount"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditGiftCard",
                        Enabled = true,
                        Name = "Edit a gift card"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditManufacturer",
                        Enabled = true,
                        Name = "Edit a manufacturer"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditProduct",
                        Enabled = true,
                        Name = "Edit a product"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditProductAttribute",
                        Enabled = true,
                        Name = "Edit a product attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditPromotionProviders",
                        Enabled = true,
                        Name = "Edit promotion providers"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditReturnRequest",
                        Enabled = true,
                        Name = "Edit a return request"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditSettings",
                        Enabled = true,
                        Name = "Edit setting(s)"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditSpecAttribute",
                        Enabled = true,
                        Name = "Edit a specification attribute"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditWidget",
                        Enabled = true,
                        Name = "Edit a widget"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditThemeVars",
                        Enabled = true,
                        Name = "Edit theme variables"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "EditOrder",
                        Enabled = true,
                        Name = "Edit an order"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "ResetThemeVars",
                        Enabled = true,
                        Name = "Reset theme variables to defaults"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "ImportThemeVars",
                        Enabled = true,
                        Name = "Import theme variables"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "ExportThemeVars",
                        Enabled = true,
                        Name = "Export theme variables"
                    },

				//public store activities
				new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.ViewCategory",
                        Enabled = false,
                        Name = "Public store. View a category"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.ViewManufacturer",
                        Enabled = false,
                        Name = "Public store. View a manufacturer"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.ViewProduct",
                        Enabled = false,
                        Name = "Public store. View a product"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.PlaceOrder",
                        Enabled = false,
                        Name = "Public store. Place an order"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.SendPM",
                        Enabled = false,
                        Name = "Public store. Send PM"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.ContactUs",
                        Enabled = false,
                        Name = "Public store. Use contact us form"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddToCompareList",
                        Enabled = false,
                        Name = "Public store. Add to compare list"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddToShoppingCart",
                        Enabled = false,
                        Name = "Public store. Add to shopping cart"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddToWishlist",
                        Enabled = false,
                        Name = "Public store. Add to wishlist"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.Login",
                        Enabled = false,
                        Name = "Public store. Login"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.Logout",
                        Enabled = false,
                        Name = "Public store. Logout"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddProductReview",
                        Enabled = false,
                        Name = "Public store. Add product review"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddNewsComment",
                        Enabled = false,
                        Name = "Public store. Add news comment"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddBlogComment",
                        Enabled = false,
                        Name = "Public store. Add blog comment"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddForumTopic",
                        Enabled = false,
                        Name = "Public store. Add forum topic"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.EditForumTopic",
                        Enabled = false,
                        Name = "Public store. Edit forum topic"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.DeleteForumTopic",
                        Enabled = false,
                        Name = "Public store. Delete forum topic"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.AddForumPost",
                        Enabled = false,
                        Name = "Public store. Add forum post"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.EditForumPost",
                        Enabled = false,
                        Name = "Public store. Edit forum post"
                    },
                new ActivityLogType
                    {
                        SystemKeyword = "PublicStore.DeleteForumPost",
                        Enabled = false,
                        Name = "Public store. Delete forum post"
                    },
			//new ActivityLogType
			//      {
			//          SystemKeyword = "EditThemeVars",
			//          Enabled = false,
			//          Name = "Edit theme variable"
			//      },
			//new ActivityLogType
			//      {
			//          SystemKeyword = "ResetThemeVars",
			//          Enabled = false,
			//          Name = "Reset theme variable"
			//      },
			};

            this.Alter(entities);
            return entities;
        }
    }
}
