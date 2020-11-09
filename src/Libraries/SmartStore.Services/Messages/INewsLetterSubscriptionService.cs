using System;
using SmartStore.Core;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
    public partial interface INewsLetterSubscriptionService
    {
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
        /// Adds or deletes a newsletter subscription
        /// </summary>
        /// <param name="add"><c>true</c> add subscription, <c>false</c> delete</param>
        /// <param name="email">Email address</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns><c>true</c> added subscription, <c>false</c> removed subscription, <c>null</c> did nothing</returns>
        bool? AddNewsLetterSubscriptionFor(bool add, string email, int storeId);

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
        /// Gets a list of newsletter subscriptions including associated customers.
        /// </summary>
        /// <param name="email">Filter by email.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
		/// <param name="showHidden">A value indicating whether the not active subscriptions should be loaded.</param>
		/// <param name="storeIds">Filter by store identifiers.</param>
        /// <<param name="customerRolesIds">Filter by customer role identifiers.</param>
        /// <returns>List of newsletter scubscribers.</returns>
        IPagedList<NewsletterSubscriber> GetAllNewsLetterSubscriptions(
            string email,
            int pageIndex,
            int pageSize,
            bool showHidden = false,
            int[] storeIds = null,
            int[] customerRolesIds = null);
    }
}