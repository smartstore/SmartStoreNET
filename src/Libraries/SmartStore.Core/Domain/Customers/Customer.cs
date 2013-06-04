using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using SmartStore.Core.Domain.Affiliates;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;

namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    [DataContract]
    public partial class Customer : BaseEntity
    {
        private ICollection<ExternalAuthenticationRecord> _externalAuthenticationRecords;
        private ICollection<CustomerContent> _customerContent;
        private ICollection<CustomerRole> _customerRoles;
        private ICollection<ShoppingCartItem> _shoppingCartItems;
        private ICollection<Order> _orders;
        private ICollection<RewardPointsHistory> _rewardPointsHistory;
        private ICollection<ReturnRequest> _returnRequests;
        private ICollection<Address> _addresses;
        private ICollection<ForumTopic> _forumTopics;
        private ICollection<ForumPost> _forumPosts;

        public Customer()
        {
            this.CustomerGuid = Guid.NewGuid();
            this.PasswordFormat = PasswordFormat.Clear;
        }

        /// <summary>
        /// Gets or sets the customer Guid
        /// </summary>
        [DataMember]
        public Guid CustomerGuid { get; set; }

        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }

        public int PasswordFormatId { get; set; }
        public PasswordFormat PasswordFormat
        {
            get { return (PasswordFormat)PasswordFormatId; }
            set { this.PasswordFormatId = (int)value; }
        }

        public string PasswordSalt { get; set; }
        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        [DataMember]
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the currency identifier
        /// </summary>
        public int CurrencyId { get; set; }

        /// codehint: sm-edit
        /// <summary>
        /// Gets or sets the tax display type identifier
        /// </summary>
        public int? TaxDisplayTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is tax exempt
        /// </summary>
        [DataMember]
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// Gets or sets a VAT number (including counry code)
        /// </summary>
        [DataMember]
        public string VatNumber { get; set; }

        /// <summary>
        /// Gets or sets the VAT number status identifier
        /// </summary>
        public int VatNumberStatusId { get; set; }

        /// <summary>
        /// Gets or sets the last payment method system name (selected one)
        /// </summary>
        public string SelectedPaymentMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the selected checkout attributes (serialized)
        /// </summary>
        public string CheckoutAttributes { get; set; }

        /// <summary>
        /// Gets or sets the applied discount coupon code
        /// </summary>
        public string DiscountCouponCode { get; set; }

        /// <summary>
        /// Gets or sets the applied gift card coupon codes (serialized)
        /// </summary>
        public string GiftCardCouponCodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use reward points during checkout
        /// </summary>
        public bool UseRewardPointsDuringCheckout { get; set; }

        /// <summary>
        /// Gets or sets the time zone identifier
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
        public int AffiliateId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the customer is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer account is system
        /// </summary>
        public bool IsSystemAccount { get; set; }

        /// <summary>
        /// Gets or sets the customer system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDateUtc { get; set; }
        
        #region Custom properties

        /// <summary>
        /// Gets the tax display type
        /// </summary>
        public TaxDisplayType? TaxDisplayType
        {
            get
            {
                return (TaxDisplayType)this.TaxDisplayTypeId;
            }
            set
            {
                this.TaxDisplayTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets the VAT number status
        /// </summary>
        public VatNumberStatus VatNumberStatus
        {
            get
            {
                return (VatNumberStatus)this.VatNumberStatusId;
            }
            set
            {
                this.VatNumberStatusId = (int)value;
            }
        }
        
        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets customer generated content
        /// </summary>
        public virtual ICollection<ExternalAuthenticationRecord> ExternalAuthenticationRecords
        {
            get { return _externalAuthenticationRecords ?? (_externalAuthenticationRecords = new List<ExternalAuthenticationRecord>()); }
            protected set { _externalAuthenticationRecords = value; }
        }

        /// <summary>
        /// Gets or sets customer generated content
        /// </summary>
        public virtual ICollection<CustomerContent> CustomerContent
        {
            get { return _customerContent ?? (_customerContent = new List<CustomerContent>()); }
            protected set { _customerContent = value; }
        }

        /// <summary>
        /// Gets or sets the customer roles
        /// </summary>
        public virtual ICollection<CustomerRole> CustomerRoles
        {
            get { return _customerRoles ?? (_customerRoles = new List<CustomerRole>()); }
            protected set { _customerRoles = value; }
        }

        /// <summary>
        /// Gets or sets shopping cart items
        /// </summary>
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems
        {
            get { return _shoppingCartItems ?? (_shoppingCartItems = new List<ShoppingCartItem>()); }
            protected set { _shoppingCartItems = value; }            
        }

        /// <summary>
        /// Gets or sets orders
        /// </summary>
        public virtual ICollection<Order> Orders
        {
            get { return _orders ?? (_orders = new List<Order>()); }
            protected set { _orders = value; }            
        }

        /// <summary>
        /// Gets or sets reward points history
        /// </summary>
        public virtual ICollection<RewardPointsHistory> RewardPointsHistory
        {
            get { return _rewardPointsHistory ?? (_rewardPointsHistory = new List<RewardPointsHistory>()); }
            protected set { _rewardPointsHistory = value; }            
        }

        /// <summary>
        /// Gets or sets return request of this customer
        /// </summary>
        public virtual ICollection<ReturnRequest> ReturnRequests
        {
            get { return _returnRequests ?? (_returnRequests = new List<ReturnRequest>()); }
            protected set { _returnRequests = value; }            
        }
        
        /// <summary>
        /// Default billing address
        /// </summary>
        public virtual Address BillingAddress { get; set; }

        /// <summary>
        /// Default shipping address
        /// </summary>
        public virtual Address ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets customer addresses
        /// </summary>
        public virtual ICollection<Address> Addresses
        {
            get { return _addresses ?? (_addresses = new List<Address>()); }
            protected set { _addresses = value; }            
        }

        /// <summary>
        /// Gets or sets the created forum topics
        /// </summary>
        public virtual ICollection<ForumTopic> ForumTopics
        {
            get { return _forumTopics ?? (_forumTopics = new List<ForumTopic>()); }
            protected set { _forumTopics = value; }
        }

        /// <summary>
        /// Gets or sets the created forum posts
        /// </summary>
        public virtual ICollection<ForumPost> ForumPosts
        {
            get { return _forumPosts ?? (_forumPosts = new List<ForumPost>()); }
            protected set { _forumPosts = value; }
        }
        
        #endregion

        #region Addresses

        public virtual void RemoveAddress(Address address)
        {
            if (this.Addresses.Contains(address))
            {
                if (this.BillingAddress == address) this.BillingAddress = null;
                if (this.ShippingAddress == address) this.ShippingAddress = null;

                this.Addresses.Remove(address);
            }
        }

        #endregion

        #region Reward points

        public void AddRewardPointsHistoryEntry(int points, string message = "",
            Order usedWithOrder = null, decimal usedAmount = 0M)
        {
            int newPointsBalance = this.GetRewardPointsBalance() + points;

            var rewardPointsHistory = new RewardPointsHistory()
            {
                Customer = this,
                UsedWithOrder = usedWithOrder,
                Points = points,
                PointsBalance = newPointsBalance,
                UsedAmount = usedAmount,
                Message = message,
                CreatedOnUtc = DateTime.UtcNow
            };

            this.RewardPointsHistory.Add(rewardPointsHistory);
        }

        /// <summary>
        /// Gets reward points balance
        /// </summary>
        public int GetRewardPointsBalance()
        {
            int result = 0;
            if (this.RewardPointsHistory.Count > 0)
                result = this.RewardPointsHistory.OrderByDescending(rph => rph.CreatedOnUtc).ThenByDescending(rph => rph.Id).FirstOrDefault().PointsBalance;
            return result;
        }

        #endregion

        #region Gift cards

        /// <summary>
        /// Gets coupon codes
        /// </summary>
        /// <returns>Coupon codes</returns>
        public string[] ParseAppliedGiftCardCouponCodes()
        {
            string serializedGiftCartCouponCodes = this.GiftCardCouponCodes;

            var couponCodes = new List<string>();
            if (String.IsNullOrEmpty(serializedGiftCartCouponCodes))
                return couponCodes.ToArray();

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(serializedGiftCartCouponCodes);

                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string code = node1.Attributes["Code"].InnerText.Trim();
                        couponCodes.Add(code);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return couponCodes.ToArray();
        }

        /// <summary>
        /// Adds a coupon code
        /// </summary>
        /// <param name="couponCode">Coupon code</param>
        /// <returns>New coupon codes document</returns>
        public void ApplyGiftCardCouponCode(string couponCode)
        {
            string result = string.Empty;
            try
            {
                var serializedGiftCartCouponCodes = this.GiftCardCouponCodes;

                couponCode = couponCode.Trim().ToLower();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(serializedGiftCartCouponCodes))
                {
                    var element1 = xmlDoc.CreateElement("GiftCardCouponCodes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(serializedGiftCartCouponCodes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//GiftCardCouponCodes");

                XmlElement gcElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["Code"] != null)
                    {
                        string _couponCode = node1.Attributes["Code"].InnerText.Trim();
                        if (_couponCode.ToLower() == couponCode.ToLower())
                        {
                            gcElement = (XmlElement)node1;
                            break;
                        }
                    }
                }

                //create new one if not found
                if (gcElement == null)
                {
                    gcElement = xmlDoc.CreateElement("CouponCode");
                    gcElement.SetAttribute("Code", couponCode);
                    rootElement.AppendChild(gcElement);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            //apply new value
            this.GiftCardCouponCodes = result;
        }

        /// <summary>
        /// Removes a coupon code
        /// </summary>
        /// <param name="couponCode">Coupon code to remove</param>
        /// <returns>New coupon codes document</returns>
        public void RemoveGiftCardCouponCode(string couponCode)
        {
            //get applied coupon codes
            var existingCouponCodes = ParseAppliedGiftCardCouponCodes();

            //clear them
            this.GiftCardCouponCodes = string.Empty;

            //save again except removed one
            foreach (string existingCouponCode in existingCouponCodes)
                if (!existingCouponCode.Equals(couponCode, StringComparison.InvariantCultureIgnoreCase))
                    ApplyGiftCardCouponCode(existingCouponCode);
        }
        
        #endregion
    }
}