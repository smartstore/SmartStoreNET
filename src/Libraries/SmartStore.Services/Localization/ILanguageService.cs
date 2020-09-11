using System.Collections.Generic;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
    /// <summary>
    /// Language service interface
    /// </summary>
    public partial interface ILanguageService
    {
        /// <summary>
        /// Deletes a language
        /// </summary>
        /// <param name="language">Language</param>
        void DeleteLanguage(Language language);

        /// <summary>
        /// Gets all languages
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Language collection</returns>
		IList<Language> GetAllLanguages(bool showHidden = false, int storeId = 0);

        /// <summary>
        /// Gets languages count
        /// </summary>
        /// <param name="showHidden">A value indicating whether to consider hidden records</param>
        /// <returns>The count of Languages</returns>
        int GetLanguagesCount(bool showHidden = false);

        /// <summary>
        /// Gets a language
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Language</returns>
        Language GetLanguageById(int languageId);

        /// <summary>
        /// Gets a language by culture code (e.g.: en-US)
        /// </summary>
        /// <param name="culture">Culture code</param>
        /// <returns>Language</returns>
        Language GetLanguageByCulture(string culture);

        /// <summary>
        /// Gets a language by it's unique seo code (e.g.: en)
        /// </summary>
        /// <param name="seoCode">SEO code</param>
        /// <returns>Language</returns>
        Language GetLanguageBySeoCode(string seoCode);

        /// <summary>
        /// Inserts a language
        /// </summary>
        /// <param name="language">Language</param>
        void InsertLanguage(Language language);

        /// <summary>
        /// Updates a language
        /// </summary>
        /// <param name="language">Language</param>
        void UpdateLanguage(Language language);

        /// <summary>
        /// Determines whether a language is active/published
        /// </summary>
        /// <param name="seoCode">The SEO code of the language to check</param>
        /// <param name="storeId">The store id</param>
        /// <returns><c>true</c> when the language is published, <c>false</c> otherwise</returns>
        bool IsPublishedLanguage(string seoCode, int storeId = 0);

        /// <summary>
        /// Determines whether a language is active/published
        /// </summary>
        /// <param name="languageId">The id of the language to check</param>
        /// <param name="storeId">The store id</param>
        /// <returns><c>true</c> when the language is published, <c>false</c> otherwise</returns>
        bool IsPublishedLanguage(int languageId, int storeId = 0);

        /// <summary>
        /// Gets the seo code of the default (first) active language
        /// </summary>
        /// <param name="storeId">The store id</param>
        /// <returns>The seo code</returns>
        string GetDefaultLanguageSeoCode(int storeId = 0);

        /// <summary>
        /// Gets the id of the default (first) active language
        /// </summary>
        /// <param name="storeId">The store id</param>
        /// <returns>The language id</returns>
        int GetDefaultLanguageId(int storeId = 0);
    }
}
