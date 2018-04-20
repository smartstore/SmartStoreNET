using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Gift card service
    /// </summary>
    public partial class GiftCardService : IGiftCardService
    {
        #region Fields
        
        private readonly IRepository<GiftCard> _giftCardRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="giftCardRepository">Gift card context</param>
        /// <param name="eventPublisher">Event published</param>
        public GiftCardService(IRepository<GiftCard> giftCardRepository, IEventPublisher eventPublisher)
        {
            _giftCardRepository = giftCardRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Deletes a gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual void DeleteGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            _giftCardRepository.Delete(giftCard);

        }

        /// <summary>
        /// Gets a gift card
        /// </summary>
        /// <param name="giftCardId">Gift card identifier</param>
        /// <returns>Gift card entry</returns>
        public virtual GiftCard GetGiftCardById(int giftCardId)
        {
            if (giftCardId == 0)
                return null;

            var giftCard = _giftCardRepository.GetById(giftCardId);
            return giftCard;
        }

        /// <summary>
        /// Gets all gift cards
        /// </summary>
        /// <param name="purchasedWithOrderId">Associated order ID; null to load all records</param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <param name="isGiftCardActivated">Value indicating whether gift card is activated; null to load all records</param>
        /// <param name="giftCardCouponCode">Gift card coupon code; null or string.empty to load all records</param>
        /// <returns>Gift cards</returns>
        public virtual IList<GiftCard> GetAllGiftCards(int? purchasedWithOrderId, 
            DateTime? startTime = null, DateTime? endTime = null,
            bool? isGiftCardActivated = null, string giftCardCouponCode = "")
        {
            var query = _giftCardRepository.Table;
            if (purchasedWithOrderId.HasValue)
                query = query.Where(gc => gc.PurchasedWithOrderItem != null && gc.PurchasedWithOrderItem.OrderId == purchasedWithOrderId.Value);
            if (startTime.HasValue)
                query = query.Where(gc => startTime.Value <= gc.CreatedOnUtc);
            if (endTime.HasValue)
                query = query.Where(gc => endTime.Value >= gc.CreatedOnUtc);
            if (isGiftCardActivated.HasValue)
                query = query.Where(gc => gc.IsGiftCardActivated == isGiftCardActivated.Value);
            if (!String.IsNullOrEmpty(giftCardCouponCode))
                query = query.Where(gc => gc.GiftCardCouponCode == giftCardCouponCode);
            query = query.OrderByDescending(gc => gc.CreatedOnUtc);

            var giftCards = query.ToList();
            return giftCards;
        }

        /// <summary>
        /// Inserts a gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual void InsertGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            _giftCardRepository.Insert(giftCard);
        }

        /// <summary>
        /// Updates the gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual void UpdateGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            _giftCardRepository.Update(giftCard);
        }

        /// <summary>
        /// Gets gift cards by 'PurchasedWithOrderItemId'
        /// </summary>
        /// <param name="purchasedWithOrderItemId">Purchased with order item identifier</param>
        /// <returns>Gift card entries</returns>
        public virtual IList<GiftCard> GetGiftCardsByPurchasedWithOrderItemId(int purchasedWithOrderItemId)
        {
            if (purchasedWithOrderItemId == 0)
                return new List<GiftCard>();

            var query = _giftCardRepository.Table;
            query = query.Where(gc => gc.PurchasedWithOrderItemId.HasValue && gc.PurchasedWithOrderItemId.Value == purchasedWithOrderItemId);
            query = query.OrderBy(gc => gc.Id);

            var giftCards = query.ToList();
            return giftCards;
        }

        public virtual IList<GiftCard> GetActiveGiftCardsAppliedByCustomer(Customer customer, int storeId)
        {
            var result = new List<GiftCard>();
            if (customer == null)
                return result;

            string[] couponCodes = customer.ParseAppliedGiftCardCouponCodes();
            foreach (var couponCode in couponCodes)
            {
                var giftCards = GetAllGiftCards(null, null, null, true, couponCode);
                foreach (var gc in giftCards)
                {
                    if (gc.IsGiftCardValid(storeId))
                        result.Add(gc);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate new gift card code
        /// </summary>
        /// <returns>Result</returns>
        public virtual string GenerateGiftCardCode()
        {
            int length = 13;
            string result = Guid.NewGuid().ToString();
            if (result.Length > length)
                result = result.Substring(0, length);
            return result;
        }

        #endregion
    }
}
