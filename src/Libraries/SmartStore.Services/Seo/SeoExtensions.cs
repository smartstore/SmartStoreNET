using System;
using System.Linq;
using System.Runtime.CompilerServices;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

namespace SmartStore.Services.Seo
{
    public static class SeoExtensions
    {
        #region Entities

        /// <summary>
        /// Gets product tag SE (search engine) name
        /// </summary>
        /// <param name="productTag">Product tag</param>
        /// <returns>Product tag SE (search engine) name</returns>
        public static string GetSeName(this ProductTag productTag)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            return GetSeName(productTag, workContext.WorkingLanguage.Id);
        }

        /// <summary>
        /// Gets product tag SE (search engine) name
        /// </summary>
        /// <param name="productTag">Product tag</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Product tag SE (search engine) name</returns>
        public static string GetSeName(this ProductTag productTag, int languageId)
        {
            Guard.NotNull(productTag, nameof(productTag));

            return GetSeName((string)productTag.GetLocalized(x => x.Name, languageId));
        }

        /// <summary>
        /// Gets blog post SE (search engine) name
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Blog post SE (search engine) name</returns>
        public static string GetSeName(this BlogPost blogPost, int languageId)
        {
            Guard.NotNull(blogPost, nameof(blogPost));

            return GetSeName(blogPost.GetLocalized(x => x.Title, languageId));
        }

        /// <summary>
        /// Gets blog post SE (search engine) name
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>Blog post SE (search engine) name</returns>
        public static string GetSeName(this BlogPostTag blogPostTag)
        {
            Guard.NotNull(blogPostTag, nameof(blogPostTag));

            return GetSeName(blogPostTag.Name);
        }

        /// <summary>
        /// Gets news item SE (search engine) name
        /// </summary>
        /// <param name="newsItem">News item</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>News item SE (search engine) name</returns>
        public static string GetSeName(this NewsItem newsItem, int languageId)
        {
            Guard.NotNull(newsItem, nameof(newsItem));

            return GetSeName(newsItem.GetLocalized(x => x.Title, languageId));
        }

        /// <summary>
        /// Gets ForumTopic SE (search engine) name
        /// </summary>
        /// <param name="forumTopic">ForumTopic</param>
        /// <returns>ForumTopic SE (search engine) name</returns>
        public static string GetSeName(this ForumTopic forumTopic)
        {
            Guard.NotNull(forumTopic, nameof(forumTopic));

            string seName = GetSeName(forumTopic.Subject);

            // Trim SE name to avoid URLs that are too long
            var maxLength = 100;
            if (seName.Length > maxLength)
            {
                seName = seName.Substring(0, maxLength);
            }

            return seName;
        }

        /// <summary>
        /// Get search engine name for a category node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>SEO slug</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSeName(this ICategoryNode node)
        {
            Guard.NotNull(node, nameof(node));

            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetSeName("Category", node.Id, null);
        }

        #endregion

        #region Generic

        /// <summary>
        ///  Get search engine name
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if language specified one is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <returns>SEO slug</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSeName<T>(this T entity,
            int? languageId = null,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true)
            where T : BaseEntity, ISlugSupported
        {
            Guard.NotNull(entity, nameof(entity));

            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetSeName(
                entity.GetEntityName(),
                entity.Id,
                languageId,
                returnDefaultValue,
                ensureTwoPublishedLanguages);
        }

        /// <summary>
        /// Validate search engine name
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="seName">Search engine name to validate</param>
        /// <param name="name">User-friendly name used to generate sename</param>
        /// <param name="ensureNotEmpty">Ensreu that sename is not empty</param>
        /// <returns>Valid sename</returns>
        public static string ValidateSeName<T>(this T entity, string seName, string name, bool ensureNotEmpty, int? languageId = null)
             where T : BaseEntity, ISlugSupported
        {
            return entity.ValidateSeName(
                seName,
                name,
                ensureNotEmpty,
                EngineContext.Current.Resolve<IUrlRecordService>(),
                EngineContext.Current.Resolve<SeoSettings>(),
                languageId);
        }

        public static string ValidateSeName<T>(this T entity,
            string seName,
            string name,
            bool ensureNotEmpty,
            IUrlRecordService urlRecordService,
            SeoSettings seoSettings,
            int? languageId = null,
            Func<string, UrlRecord> extraSlugLookup = null)
            where T : BaseEntity, ISlugSupported
        {
            Guard.NotNull(urlRecordService, nameof(urlRecordService));
            Guard.NotNull(seoSettings, nameof(seoSettings));
            Guard.NotNull(entity, nameof(entity));

            // use name if sename is not specified
            if (String.IsNullOrWhiteSpace(seName) && !String.IsNullOrWhiteSpace(name))
                seName = name;

            // validation
            seName = GetSeName(seName, seoSettings);

            // max length
            seName = seName.Truncate(400);

            if (String.IsNullOrWhiteSpace(seName))
            {
                if (ensureNotEmpty)
                {
                    // use entity identifier as sename if empty
                    seName = entity.Id.ToString();
                }
                else
                {
                    // return. no need for further processing
                    return seName;
                }
            }

            // validate and alter SeName if it could be interpreted as SEO code
            if (LocalizationHelper.IsValidCultureCode(seName))
            {
                if (seName.Length == 2)
                {
                    seName += "-0";
                }
            }

            // ensure this sename is not reserved yet
            string entityName = entity.GetEntityName();
            int i = 2;
            var tempSeName = seName;

            while (true)
            {
                // check whether such slug already exists (and that it's not the current entity)
                var urlRecord = urlRecordService.GetBySlug(tempSeName) ?? extraSlugLookup?.Invoke(tempSeName);
                var reserved1 = urlRecord != null && !(urlRecord.EntityId == entity.Id && urlRecord.EntityName.Equals(entityName, StringComparison.InvariantCultureIgnoreCase));

                if (!reserved1 && urlRecord != null && languageId.HasValue)
                    reserved1 = (urlRecord.LanguageId != languageId.Value);

                // and it's not in the list of reserved slugs
                var reserved2 = seoSettings.ReservedUrlRecordSlugs.Contains(tempSeName, StringComparer.InvariantCultureIgnoreCase);
                if (!reserved1 && !reserved2)
                    break;

                tempSeName = string.Format("{0}-{1}", seName, i);
                i++;
            }
            seName = tempSeName;

            return seName;
        }


        /// <summary>
        /// Get SEO friendly name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Result</returns>
        public static string GetSeName(string name)
        {
            var seoSettings = EngineContext.Current.Resolve<SeoSettings>();
            return GetSeName(name, seoSettings);
        }

        /// <summary>
        /// Get SEO friendly name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="seoSettings">SEO settings</param>
        /// <returns>Result</returns>
        public static string GetSeName(string name, SeoSettings seoSettings)
        {
            return SeoHelper.GetSeName(
                name,
                seoSettings == null ? false : seoSettings.ConvertNonWesternChars,
                seoSettings == null ? false : seoSettings.AllowUnicodeCharsInUrls,
                true,
                seoSettings?.SeoNameCharConversion);
        }

        #endregion
    }
}
