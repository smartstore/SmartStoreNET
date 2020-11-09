using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Seo
{
    /// <summary>
    /// Provides information about URL records
    /// </summary>
    public partial interface IUrlRecordService : IScopedService
    {
        /// <summary>
        /// Deletes an URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        void DeleteUrlRecord(UrlRecord urlRecord);

        /// <summary>
        /// Gets an URL record
        /// </summary>
        /// <param name="urlRecordId">URL record identifier</param>
        /// <returns>URL record</returns>
        UrlRecord GetUrlRecordById(int urlRecordId);

        /// <summary>
        /// Gets URL records by identifiers
        /// </summary>
        /// <param name="urlRecordIds"></param>
        /// <returns>List of URL records</returns>
        IList<UrlRecord> GetUrlRecordsByIds(int[] urlRecordIds);

        /// <summary>
        /// Inserts an URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        void InsertUrlRecord(UrlRecord urlRecord);

        /// <summary>
        /// Updates the URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        void UpdateUrlRecord(UrlRecord urlRecord);

        /// <summary>
        /// Find URL record
        /// </summary>
        /// <param name="slug">Slug</param>
	    /// <returns>Found URL record</returns>
	    UrlRecord GetBySlug(string slug);

        /// <summary>
        /// Gets all URL records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
		/// <param name="slug">Slug</param>
		/// <param name="entityName">Entity name</param>
		/// <param name="entityId">Entity identifier</param>
		/// <param name="isActive">Whether to load only active records</param>
		/// <param name="languageId">Language identifier</param>
        /// <returns>Customer collection</returns>
		IPagedList<UrlRecord> GetAllUrlRecords(int pageIndex, int pageSize, string slug, string entityName, int? entityId, int? languageId, bool? isActive);

        /// <summary>
        /// Prefetches a collection of url records properties for a range of entities in one go
        /// and caches them for the duration of the current request.
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="entityIds">
        /// The entity ids to prefetch url records for. Can be null,
        /// in which case all records for the requested entity name are loaded.
        /// </param>
        /// <param name="isRange">Whether <paramref name="entityIds"/> represents a range of ids (perf).</param>
        /// <param name="isSorted">Whether <paramref name="entityIds"/> is already sorted (perf).</param>
        /// <returns>Url record collection</returns>
        /// <remarks>
        /// Be careful not to load large amounts of data at once (e.g. for "Product" scope with large range).
        /// </remarks>
        void PrefetchUrlRecords(string entityName, int[] languageIds, int[] entityIds, bool isRange = false, bool isSorted = false);

        /// <summary>
        /// Prefetches a collection of url records properties for a range of entities in one go.
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="entityIds">
        /// The entity ids to prefetch url records for. Can be null,
        /// in which case all records for the requested entity name are loaded.
        /// </param>
        /// <param name="isRange">Whether <paramref name="entityIds"/> represents a range of ids (perf).</param>
        /// <param name="isSorted">Whether <paramref name="entityIds"/> is already sorted (perf).</param>
        /// <returns>Url record collection</returns>
        /// <remarks>
        /// Be careful not to load large amounts of data at once (e.g. for "Product" scope with large range).
        /// </remarks>
        UrlRecordCollection GetUrlRecordCollection(string entityName, int[] languageIds, int[] entityIds, bool isRange = false, bool isSorted = false);

        /// <summary>
        /// Gets all URL records for the specified entity
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="activeOnly">Specifies whether only active URL records should be returned</param>
        /// <returns>List of URL records</returns>
        IList<UrlRecord> GetUrlRecordsFor(string entityName, int entityId, bool activeOnly = false);

        /// <summary>
        /// Find slug
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Found slug</returns>
        string GetActiveSlug(int entityId, string entityName, int languageId);

        /// <summary>
        /// Save slug
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="slug">Slug</param>
        /// <param name="languageId">Language ID</param>
		/// <returns>
		/// A <see cref="UrlRecord"/> instance when a new record had to be inserted, <c>null</c> otherwise.
		/// </returns>
        UrlRecord SaveSlug<T>(T entity, string slug, int languageId) where T : BaseEntity, ISlugSupported;

        /// <summary>
        /// Save slug
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="nameProperty">Name of a property</param>
        /// <returns>Url record</returns>
        UrlRecord SaveSlug<T>(T entity, Func<T, string> nameProperty) where T : BaseEntity, ISlugSupported;

        /// <summary>
        /// Get number of slugs per entity
        /// </summary>
        /// <param name="urlRecordIds">URL record identifier</param>
        /// <returns>Dictionary of slugs per entity count</returns>
        Dictionary<int, int> CountSlugsPerEntity(int[] urlRecordIds);

        /// <summary>
        /// Get number of slugs per entity
        /// </summary>
        /// <param name="entityName">Entity name</param>
        /// <param name="entityId">Entity identifier</param>
        /// <returns>Number of slugs per entity</returns>
        int CountSlugsPerEntity(string entityName, int entityId);
    }
}