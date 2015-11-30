using System;
using System.IO;
using SmartStore.Core;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Data;

namespace SmartStore.Services.Messages
{
    public partial interface INewsLetterSubscriptionService
    {

        /// <summary>
        /// Executes a bulk import of subscriber data from a CSV source
        /// </summary>
        /// <param name="stream">The input file stream</param>
        /// <returns>The import result</returns>
        ImportResult ImportSubscribers(Stream stream);
        
        /// <summary>
        /// Inserts a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        void InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        /// <summary>
        /// Updates a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        /// <summary>
        /// Deletes a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        void DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription identifier
        /// </summary>
        /// <param name="newsLetterSubscriptionId">The newsletter subscription identifier</param>
        /// <returns>NewsLetter subscription</returns>
        NewsLetterSubscription GetNewsLetterSubscriptionById(int newsLetterSubscriptionId);

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription GUID
        /// </summary>
        /// <param name="newsLetterSubscriptionGuid">The newsletter subscription GUID</param>
        /// <returns>NewsLetter subscription</returns>
        NewsLetterSubscription GetNewsLetterSubscriptionByGuid(Guid newsLetterSubscriptionGuid);

        /// <summary>
        /// Gets a newsletter subscription by email
        /// </summary>
        /// <param name="email">The newsletter subscription email</param>
		/// <param name="storeId">The store identifier</param>
        /// <returns>NewsLetter subscription</returns>
		NewsLetterSubscription GetNewsLetterSubscriptionByEmail(string email, int storeId = 0);

        /// <summary>
        /// Gets the newsletter subscription list
        /// </summary>
        /// <param name="email">Email to search or string. Empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
		/// <param name="showHidden">A value indicating whether the not active subscriptions should be loaded</param>
		/// <param name="storeId">The store identifier</param>
        /// <returns>NewsLetterSubscription entity list</returns>
		IPagedList<NewsLetterSubscription> GetAllNewsLetterSubscriptions(string email, int pageIndex, int pageSize, bool showHidden = false, int storeId = 0);
    }
}
