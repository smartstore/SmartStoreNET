CREATE NONCLUSTERED INDEX [IX_StateProvince_CountryId] ON [StateProvince] ([CountryId]) INCLUDE ([DisplayOrder])
GO

CREATE NONCLUSTERED INDEX [IX_PSAM_AllowFiltering] ON [Product_SpecificationAttribute_Mapping] ([AllowFiltering] ASC) INCLUDE ([ProductId],[SpecificationAttributeOptionId])
GO

CREATE NONCLUSTERED INDEX [IX_PSAM_SpecificationAttributeOptionId_AllowFiltering] ON [Product_SpecificationAttribute_Mapping] ([SpecificationAttributeOptionId] ASC, [AllowFiltering] ASC) INCLUDE ([ProductId])
GO

CREATE NONCLUSTERED INDEX [IX_LocalizedProperty_Key] ON [LocalizedProperty] ([Id])	INCLUDE ([EntityId], [LocaleKeyGroup], [LocaleKey])
GO

CREATE NONCLUSTERED INDEX [IX_SeekExport1] ON [dbo].[Product]
(
	[Published] ASC,
	[Id] ASC,
	[VisibleIndividually] ASC,
	[Deleted] ASC,
	[IsSystemProduct] ASC,
	[AvailableStartDateTimeUtc] ASC,
	[AvailableEndDateTimeUtc] ASC
)
INCLUDE ([UpdatedOnUtc]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO