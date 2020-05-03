using System;
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
						Name = "یک دسته بندی اضافه کرد"
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
						Name = "یک مشتری اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewCustomerRole",
						Enabled = true,
						Name = "یک رول کاربری اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewDiscount",
						Enabled = true,
						Name = "تخفیف اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewGiftCard",
						Enabled = true,
						Name = "کارت هدیه اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewManufacturer",
						Enabled = true,
						Name = "سازنده جدید اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewProduct",
						Enabled = true,
						Name = "محصول جدید اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewProductAttribute",
						Enabled = true,
						Name = "ویژگی محصول اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewSetting",
						Enabled = true,
						Name = "تنظیمات جدید اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewSpecAttribute",
						Enabled = true,
						Name = "خصوصیت جدید اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "AddNewWidget",
						Enabled = true,
						Name = "یک ابزارک اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteCategory",
						Enabled = true,
						Name = "دسته بندی حذف کرد"
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
						Name = "مشتری حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteCustomerRole",
						Enabled = true,
						Name = "رول کاربری حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteDiscount",
						Enabled = true,
						Name = "یک تخفیف حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteGiftCard",
						Enabled = true,
						Name = "یک کارت هدیه حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteManufacturer",
						Enabled = true,
						Name = "یک سازنده حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteProduct",
						Enabled = true,
						Name = "یک محصول حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteProductAttribute",
						Enabled = true,
						Name = "ویژگی محصول حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteReturnRequest",
						Enabled = true,
						Name = "یک درخواست برگشت حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteSetting",
						Enabled = true,
						Name = "یک تنظیمات حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteSpecAttribute",
						Enabled = true,
						Name = "یک خصوصیت حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "DeleteWidget",
						Enabled = true,
						Name = "یک ابزارک حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditCategory",
						Enabled = true,
						Name = "دسته بندی ویرایش کرد"
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
						Name = "مشتری ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditCustomerRole",
						Enabled = true,
						Name = "رول کابری ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditDiscount",
						Enabled = true,
						Name = "کد تخفیف ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditGiftCard",
						Enabled = true,
						Name = "کارت هدیه ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditManufacturer",
						Enabled = true,
						Name = "سازنده ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditProduct",
						Enabled = true,
						Name = "محصول ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditProductAttribute",
						Enabled = true,
						Name = "ویژگی محصول ویرایش کرد"
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
						Name = "درخواست بازگشت ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditSettings",
						Enabled = true,
						Name = "تنظیمات ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditSpecAttribute",
						Enabled = true,
						Name = "خصوصیت محصول ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditWidget",
						Enabled = true,
						Name = "ابزارک ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "EditThemeVars",
						Enabled = true,
						Name = "متغیر تم ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "ResetThemeVars",
						Enabled = true,
						Name = "تنظیمات متغیرهای تم را ریست کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "ImportThemeVars",
						Enabled = true,
						Name = "متغیرهای تم ایمپورت کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "ExportThemeVars",
						Enabled = true,
						Name = "متغیرهای تم را اکسپورت کرد"
					},

				//public store activities
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.ViewCategory",
						Enabled = false,
						Name = "در فروشگاه یک دسته بندی را دید"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.ViewManufacturer",
						Enabled = false,
						Name = "در فروشگاه صفحه سازنده را دید"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.ViewProduct",
						Enabled = false,
						Name = "در فروشگاه یک محصول را مشاهده کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.PlaceOrder",
						Enabled = false,
						Name = "در فروشگاه یک سفارش ثبت کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.SendPM",
						Enabled = false,
						Name = "در فروشگاه یک پیام ارسال کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.ContactUs",
						Enabled = false,
						Name = "در فروشگاه پیغام تماس با ما ارسال کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddToCompareList",
						Enabled = false,
						Name = "در فروشگاه محصول ب مقایسه افزود"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddToShoppingCart",
						Enabled = false,
						Name = "در فروشگاه محصول به سبد اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddToWishlist",
						Enabled = false,
						Name = "در فروشگاه محصول به علاقه مندی افزود"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.Login",
						Enabled = false,
						Name = "در فروشگاه لوگین کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.Logout",
						Enabled = false,
						Name = "در فروشگاه لوگات کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddProductReview",
						Enabled = false,
						Name = "در فروشگاه برای محصول نقد ارسال کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddNewsComment",
						Enabled = false,
						Name = "در فروشگاه برای محصول نظر ارسال کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddBlogComment",
						Enabled = false,
						Name = "در فروشگاه برای بلاگ نظر ارسال کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddForumTopic",
						Enabled = false,
						Name = "در فروشگاه تاپیک ارسال کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.EditForumTopic",
						Enabled = false,
						Name = "در فروشگاه تاپیک ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.DeleteForumTopic",
						Enabled = false,
						Name = "در فروشگاه تاپیک حذف کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.AddForumPost",
						Enabled = false,
						Name = "در فروشگاه پست فروم اضافه کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.EditForumPost",
						Enabled = false,
						Name = "در فروشگاه پست فروم ویرایش کرد"
					},
				new ActivityLogType
					{
						SystemKeyword = "PublicStore.DeleteForumPost",
						Enabled = false,
						Name = "در فروشگاه پست فروم حذف کرد"
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
