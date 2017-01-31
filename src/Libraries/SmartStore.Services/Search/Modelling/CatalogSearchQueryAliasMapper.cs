using System;
using System.Collections.Concurrent;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search.Modelling
{
	public class CatalogSearchQueryAliasMapper : ICatalogSearchQueryAliasMapper
	{
		private readonly static object _lock = new object();
		private static ConcurrentDictionary<AliasMappingKey, SearchQueryAliasMapping> _attributeMappings;

		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;

		public CatalogSearchQueryAliasMapper(
			ISpecificationAttributeService specificationAttributeService,
			IRepository<LocalizedProperty> localizedPropertyRepository)
		{
			_specificationAttributeService = specificationAttributeService;
			_localizedPropertyRepository = localizedPropertyRepository;
		}

		private ConcurrentDictionary<AliasMappingKey, SearchQueryAliasMapping> AttributeMappings
		{
			get
			{
				if (_attributeMappings == null)
				{
					lock (_lock)
					{
						if (_attributeMappings == null)
						{
							_attributeMappings = new ConcurrentDictionary<AliasMappingKey, SearchQueryAliasMapping>();

							var attributes = _specificationAttributeService.GetSpecificationAttributes()
								.Expand(x => x.SpecificationAttributeOptions)
								.ToList();

							var locAttributes = _localizedPropertyRepository.TableUntracked
								.Where(x => x.LocaleKeyGroup == "SpecificationAttribute" && x.LocaleKey == "Alias")
								.ToList()
								.ToMultimap(x => x.EntityId, x => x);

							var locValues = _localizedPropertyRepository.TableUntracked
								.Where(x => x.LocaleKeyGroup == "SpecificationAttributeOption" && x.LocaleKey == "Alias")
								.ToList()
								.ToMultimap(x => x.EntityId, x => x);


							foreach (var attribute in attributes)
							{
								foreach (var value in attribute.SpecificationAttributeOptions)
								{
									SearchQueryAliasMapping mapping = null;

									if (attribute.Alias.HasValue() && value.Alias.HasValue())
									{
										mapping = new SearchQueryAliasMapping(attribute.Id, value.Id);

										_attributeMappings.TryAdd(new AliasMappingKey(attribute.Alias, value.Alias), mapping);
									}

									if (locAttributes.ContainsKey(attribute.Id) && locValues.ContainsKey(value.Id))
									{
										foreach (var locAttribute in locAttributes[attribute.Id])
										{
											var locValue = locValues[value.Id].FirstOrDefault(x => x.LanguageId == locAttribute.LanguageId);

											if (locValue != null && locAttribute.LocaleValue.HasValue() && locValue.LocaleValue.HasValue())
											{
												if (mapping == null)
													mapping = new SearchQueryAliasMapping(attribute.Id, value.Id);

												_attributeMappings.TryAdd(new AliasMappingKey(locAttribute.LocaleValue, locValue.LocaleValue), mapping);
											}
										}
									}
								}
							}

						}
					}
				}

				return _attributeMappings;
			}
		}

		public SearchQueryAliasMapping GetAttributeByAlias(string attributeAlias, string optionAlias)
		{
			if (attributeAlias.HasValue() && optionAlias.HasValue())
			{
				SearchQueryAliasMapping mapping;

				if (AttributeMappings.TryGetValue(new AliasMappingKey(attributeAlias, optionAlias), out mapping))
					return mapping;
			}

			return null;
		}

		public bool AddAttribute(string attributeAlias, string optionAlias, SearchQueryAliasMapping mapping)
		{
			Guard.NotNull(mapping, nameof(mapping));

			if (attributeAlias.HasValue() && optionAlias.HasValue())
			{
				AttributeMappings.AddOrUpdate(new AliasMappingKey(attributeAlias, optionAlias), mapping, (k, v) =>
				{
					v.CopyFrom(mapping);
					return v;
				});

				return true;
			}

			return false;
		}

		public bool RemoveAttribute(string attributeAlias, string optionAlias)
		{
			if (attributeAlias.HasValue() && optionAlias.HasValue())
			{
				SearchQueryAliasMapping unused;
				return AttributeMappings.TryRemove(new AliasMappingKey(attributeAlias, optionAlias), out unused);
			}

			return false;
		}

		public void RemoveAllAttributes()
		{
			if (_attributeMappings != null)
			{
				_attributeMappings.Clear();
				_attributeMappings = null;
			}
		}
	}


	internal class AliasMappingKey : Tuple<string, string>
	{
		public AliasMappingKey(string fieldAlias, string valueAlias)
			: base(fieldAlias.EmptyNull().ToLowerInvariant(), valueAlias.EmptyNull().ToLowerInvariant())
		{
		}

		public string FieldAlias
		{
			get { return Item1; }
		}

		public string ValueAlias
		{
			get { return Item2; }
		}
	}
}
