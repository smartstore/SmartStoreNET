DROP INDEX [IX_LocaleStringResource] ON [LocaleStringResource]
GO

DROP INDEX [IX_Product_PriceDatesEtc] ON [Product] 
GO

DROP INDEX [IX_Country_DisplayOrder] ON [Country]
GO

DROP INDEX [IX_Currency_DisplayOrder] ON [Currency]
GO

DROP INDEX [IX_Log_CreatedOnUtc] ON [Log]
GO

DROP INDEX [IX_Customer_Email] ON [Customer]
GO

DROP INDEX [IX_Customer_Username] ON [Customer]
GO

DROP INDEX [IX_Customer_CustomerGuid] ON [Customer]
GO

DROP INDEX [IX_GenericAttribute_EntityId_and_KeyGroup] ON [GenericAttribute]
GO

DROP INDEX [IX_QueuedEmail_CreatedOnUtc] ON [QueuedEmail]
GO

DROP INDEX [IX_Order_CustomerId] ON [Order]
GO

DROP INDEX [IX_Language_DisplayOrder] ON [Language]
GO

DROP INDEX [IX_BlogPost_LanguageId] ON [BlogPost]
GO

DROP INDEX [IX_BlogComment_BlogPostId] ON [BlogComment]
GO

DROP INDEX [IX_News_LanguageId] ON [News]
GO

DROP INDEX [IX_NewsComment_NewsItemId] ON [NewsComment]
GO

DROP INDEX [IX_PollAnswer_PollId] ON [PollAnswer]
GO

DROP INDEX [IX_ProductReview_ProductId] ON [ProductReview]
GO

DROP INDEX [IX_OrderItem_OrderId] ON [OrderItem]
GO

DROP INDEX [IX_OrderNote_OrderId] ON [OrderNote]
GO

DROP INDEX [IX_TierPrice_ProductId] ON [TierPrice]
GO

DROP INDEX [IX_ShoppingCartItem_ShoppingCartTypeId_CustomerId] ON [ShoppingCartItem]
GO

DROP INDEX [IX_RelatedProduct_ProductId1] ON [RelatedProduct]
GO

DROP INDEX [IX_ProductVariantAttributeValue_ProductVariantAttributeId] ON [ProductVariantAttributeValue]
GO

DROP INDEX [IX_Product_ProductAttribute_Mapping_ProductId] ON [Product_ProductAttribute_Mapping]
GO

DROP INDEX [IX_Manufacturer_DisplayOrder] ON [Manufacturer]
GO

DROP INDEX [IX_Category_DisplayOrder] ON [Category]
GO

DROP INDEX [IX_Category_ParentCategoryId] ON [Category]
GO

DROP INDEX [IX_Forums_Group_DisplayOrder] ON [Forums_Group]
GO

DROP INDEX [IX_Forums_Forum_DisplayOrder] ON [Forums_Forum]
GO

DROP INDEX [IX_Forums_Forum_ForumGroupId] ON [Forums_Forum]
GO

DROP INDEX [IX_Forums_Topic_ForumId] ON [Forums_Topic]
GO

DROP INDEX [IX_Forums_Post_TopicId] ON [Forums_Post]
GO

DROP INDEX [IX_Forums_Post_CustomerId] ON [Forums_Post]
GO

DROP INDEX [IX_Forums_Subscription_ForumId] ON [Forums_Subscription]
GO

DROP INDEX [IX_Forums_Subscription_TopicId] ON [Forums_Subscription]
GO

DROP INDEX [IX_Product_Deleted_and_Published] ON [Product]
GO

DROP INDEX [IX_Product_Published] ON [Product]
GO

DROP INDEX [IX_Product_ShowOnHomepage] ON [Product]
GO

DROP INDEX [IX_Product_ParentGroupedProductId] ON [Product]
GO

DROP INDEX [IX_Product_VisibleIndividually] ON [Product]
GO

DROP INDEX [IX_PCM_Product_and_Category] ON [Product_Category_Mapping]
GO

DROP INDEX [IX_PMM_Product_and_Manufacturer] ON [Product_Manufacturer_Mapping]
GO

DROP INDEX [IX_ProductTag_Name] ON [ProductTag]
GO

DROP INDEX [IX_ActivityLog_CreatedOnUtc] ON [ActivityLog]
GO

DROP INDEX [IX_UrlRecord_Slug] ON [UrlRecord]
GO

DROP INDEX [IX_AclRecord_EntityId_EntityName] ON [AclRecord]
GO

DROP INDEX [IX_StoreMapping_EntityId_EntityName] ON [StoreMapping]
GO

DROP INDEX [IX_Category_LimitedToStores] ON [Category]
GO

DROP INDEX [IX_Manufacturer_LimitedToStores] ON [Manufacturer]
GO

DROP INDEX [IX_Product_LimitedToStores] ON [Product]
GO

DROP INDEX [IX_ProductVariantAttributeCombination_SKU]
GO

DROP INDEX [IX_Product_Name] ON [Product]
GO

DROP INDEX [IX_Product_Sku] ON [Product]
GO

DROP INDEX [IX_ProductBundleItem_ProductId] ON [ProductBundleItem]
GO

DROP INDEX [IX_ProductBundleItem_BundleProductId] ON [ProductBundleItem]
GO

DROP INDEX [IX_ProductBundleItemAttributeFilter_BundleItemId] ON [ProductBundleItemAttributeFilter]
GO