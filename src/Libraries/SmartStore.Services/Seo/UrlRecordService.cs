using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Seo
{
    /// <summary>
    /// Provides information about URL records
    /// </summary>
    public partial class UrlRecordService : IUrlRecordService
    {
        #region Constants

		// {0} = id, {1} = name, {2} = language
        private const string URLRECORD_KEY = "SmartStore.urlrecord.{0}-{1}-{2}";
		private const string URLRECORD_ALL_ACTIVESLUGS_KEY = "SmartStore.urlrecord.all-active-slugs";
		private const string URLRECORD_PATTERN_KEY = "SmartStore.urlrecord.";

        #endregion

        #region Fields

        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly ICacheManager _cacheManager;
		private readonly SeoSettings _seoSettings;

        #endregion

        #region Ctor

        public UrlRecordService(ICacheManager cacheManager, IRepository<UrlRecord> urlRecordRepository, SeoSettings seoSettings)
        {
            this._cacheManager = cacheManager;
            this._urlRecordRepository = urlRecordRepository;
			this._seoSettings = seoSettings;
        }

        #endregion

        #region Methods

        public virtual void DeleteUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            _urlRecordRepository.Delete(urlRecord);

            _cacheManager.RemoveByPattern(URLRECORD_PATTERN_KEY);
        }

        public virtual UrlRecord GetUrlRecordById(int urlRecordId)
        {
            if (urlRecordId == 0)
                return null;

            var urlRecord = _urlRecordRepository.GetById(urlRecordId);
            return urlRecord;
        }

        public virtual void InsertUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            _urlRecordRepository.Insert(urlRecord);

            //cache
            _cacheManager.RemoveByPattern(URLRECORD_PATTERN_KEY);
        }

        public virtual void UpdateUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            _urlRecordRepository.Update(urlRecord);

            //cache
            _cacheManager.RemoveByPattern(URLRECORD_PATTERN_KEY);
        }

        public virtual IPagedList<UrlRecord> GetAllUrlRecords(string slug, int pageIndex, int pageSize)
        {
            var query = _urlRecordRepository.Table;
            if (!String.IsNullOrWhiteSpace(slug))
                query = query.Where(ur => ur.Slug.Contains(slug));
                query = query.OrderBy(ur => ur.Slug);

                var urlRecords = new PagedList<UrlRecord>(query, pageIndex, pageSize);
                return urlRecords;
        }

		public virtual IList<UrlRecord> GetUrlRecordsFor(string entityName, int entityId, bool activeOnly = false)
		{
			Guard.ArgumentNotEmpty(() => entityName);

			var query = from ur in _urlRecordRepository.Table
						where ur.EntityId == entityId &&
						ur.EntityName == entityName
						select ur;

			if (activeOnly)
			{
				query = query.Where(ur => ur.IsActive);
			}

			return query.ToList();
		}

		public virtual UrlRecord GetBySlug(string slug)
		{
			// INFO: (mc) Caching unnecessary here. This is not a 'bottleneck' function.
			if (String.IsNullOrEmpty(slug))
				return null;

			var query = from ur in _urlRecordRepository.Table
						where ur.Slug == slug
						select ur;
			var urlRecord = query.FirstOrDefault();
			return urlRecord;
		}

        public virtual string GetActiveSlug(int entityId, string entityName, int languageId)
        {
			string slug = null;

			if (_seoSettings.LoadAllUrlAliasesOnStartup)
			{
				var allActiveSlugs = _cacheManager.Get(URLRECORD_ALL_ACTIVESLUGS_KEY, () =>
				{
					var query = from x in _urlRecordRepository.TableUntracked
								where x.IsActive
								orderby x.Id descending
								select x;

					var result = query.ToDictionary(
						x => GenerateKey(x.EntityId, x.EntityName, x.LanguageId), // Key
						x => x.Slug, // Value
						StringComparer.OrdinalIgnoreCase);

					return result;
				});

				var key = GenerateKey(entityId, entityName, languageId);
				if (!allActiveSlugs.TryGetValue(key, out slug))
				{
					return string.Empty;
				}
			}
			else
			{
				string cacheKey = string.Format(URLRECORD_KEY, entityId, entityName, languageId);
				slug = _cacheManager.Get(cacheKey, () =>
				{
					var query = from ur in _urlRecordRepository.Table
								where ur.EntityId == entityId &&
								ur.EntityName == entityName &&
								ur.LanguageId == languageId &&
								ur.IsActive
								orderby ur.Id descending
								select ur.Slug;
					return query.FirstOrDefault() ?? string.Empty;
				});
			}

			return slug;
        }

        public virtual UrlRecord SaveSlug<T>(T entity, string slug, int languageId) where T : BaseEntity, ISlugSupported
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;
			UrlRecord result = null;

            var query = from ur in _urlRecordRepository.Table
                        where ur.EntityId == entityId &&
                        ur.EntityName == entityName &&
                        ur.LanguageId == languageId
                        orderby ur.Id descending 
                        select ur;
            var allUrlRecords = query.ToList();

            var activeUrlRecord = allUrlRecords.FirstOrDefault(x => x.IsActive);
            if (activeUrlRecord == null && !string.IsNullOrWhiteSpace(slug))
            {
                // find in non-active records with the specified slug
                var nonActiveRecordWithSpecifiedSlug = allUrlRecords.FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                if (nonActiveRecordWithSpecifiedSlug != null)
                {
                    // mark non-active record as active
                    nonActiveRecordWithSpecifiedSlug.IsActive = true;
                    UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);
                }
                else
                {
                    // new record
                    var urlRecord = new UrlRecord
                    {
                        EntityId = entity.Id,
                        EntityName = entityName,
                        Slug = slug,
                        LanguageId = languageId,
                        IsActive = true,
                    };
                    InsertUrlRecord(urlRecord);
					result = urlRecord;
                }
            }

            if (activeUrlRecord != null && string.IsNullOrWhiteSpace(slug))
            {
                // disable the previous active URL record
                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }

            if (activeUrlRecord != null && !string.IsNullOrWhiteSpace(slug))
            {
                // is it the same slug as in active URL record?
                if (activeUrlRecord.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase))
                {
                    // yes. do nothing
                    // P.S. wrote this way for more source code readability
                }
                else
                {
                    // find in non-active records with the specified slug
                    var nonActiveRecordWithSpecifiedSlug = allUrlRecords
                        .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                    if (nonActiveRecordWithSpecifiedSlug != null)
                    {
                        // mark non-active record as active
                        nonActiveRecordWithSpecifiedSlug.IsActive = true;
                        UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);

                        //disable the previous active URL record
                        activeUrlRecord.IsActive = false;
                        UpdateUrlRecord(activeUrlRecord);
                    }
                    else
                    {
						// MC: Absolutely ensure, that we have no duplicate active record for this entity.
						// In such case a record other than "activeUrlRecord" could have same seName
						// and the DB would report an Index error.
						var alreadyActiveDuplicate = allUrlRecords.FirstOrDefault(x => x.Slug.IsCaseInsensitiveEqual(slug) && x.IsActive);
						if (alreadyActiveDuplicate != null)
						{
							// deactivate all
							allUrlRecords.Each(x => x.IsActive = false);
							// set the existing one to active again
							alreadyActiveDuplicate.IsActive = true;
							// update all records
							allUrlRecords.Each(x => UpdateUrlRecord(x));
						}
						else
						{
							// insert new record
							// we do not update the existing record because we should track all previously entered slugs
							// to ensure that URLs will work fine
							var urlRecord = new UrlRecord
							{
								EntityId = entity.Id,
								EntityName = entityName,
								Slug = slug,
								LanguageId = languageId,
								IsActive = true,
							};
							InsertUrlRecord(urlRecord);
							result = urlRecord;

							// disable the previous active URL record
							activeUrlRecord.IsActive = false;
							UpdateUrlRecord(activeUrlRecord);
						}
                    }

                }
            }

			return result;
        }

		public virtual UrlRecord SaveSlug<T>(T entity, Expression<Func<T, string>> nameProperty) where T : BaseEntity, ISlugSupported
		{
			string name = nameProperty.Compile().Invoke(entity);

			string existingSeName = entity.GetSeName<T>(0, true, false);
			existingSeName = entity.ValidateSeName(existingSeName, name, true);

			return SaveSlug(entity, existingSeName, 0);
		}

		private string GenerateKey(int entityId, string entityName, int languageId)
		{
			return "{0}.{1}.{2}".FormatInvariant(entityId, entityName, languageId);
		}

        #endregion
    }
}