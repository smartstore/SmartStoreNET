using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Common;
using SmartStore.Collections;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    [Validator(typeof(CustomerValidator))]
    public class CustomerModel : TabbableModel
    {
        public CustomerModel()
        {
            AvailableTimeZones = new List<SelectListItem>();
            SendEmail = new SendEmailModel();
            SendPm = new SendPmModel();
            AssociatedExternalAuthRecords = new List<AssociatedExternalAuthModel>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        public bool AllowUsersToChangeUsernames { get; set; }
        public bool UsernamesEnabled { get; set; }
        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
        [AllowHtml]
        public string Username { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Password")]
        [AllowHtml]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Title")]
        public string Title { get; set; }
        public bool TitleEnabled { get; set; }

        public bool GenderEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Gender")]
        public string Gender { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.FullName")]
        public string FullName { get; set; }

        public bool DateOfBirthEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        public bool CompanyEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }

        public bool CustomerNumberEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.CustomerNumber")]
        [AllowHtml]
        public string CustomerNumber { get; set; }


        public bool StreetAddressEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.StreetAddress")]
        [AllowHtml]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.StreetAddress2")]
        [AllowHtml]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        public bool CountryEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Country")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Phone")]
        [AllowHtml]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Fax")]
        [AllowHtml]
        public string Fax { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Active")]
        public bool Active { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Affiliate")]
        public int AffiliateId { get; set; }
        public string AffiliateFullName { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.TimeZoneId")]
        [AllowHtml]
        public string TimeZoneId { get; set; }

        public bool AllowCustomersToSetTimeZone { get; set; }

        public IList<SelectListItem> AvailableTimeZones { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.VatNumber")]
        [AllowHtml]
        public string VatNumber { get; set; }

        public string VatNumberStatusNote { get; set; }

        public bool DisplayVatNumber { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.IPAddress")]
        public string LastIpAddress { get; set; }


        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.LastVisitedPage")]
        public string LastVisitedPage { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        public string CustomerRoleNames { get; set; }

        [UIHint("CustomerRoles")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Customers.CustomerRoles")]
        public int[] SelectedCustomerRoleIds { get; set; }
        public bool AllowManagingCustomerRoles { get; set; }

        public bool DisplayRewardPointsHistory { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.RewardPoints.Fields.AddRewardPointsValue")]
        public int AddRewardPointsValue { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.RewardPoints.Fields.AddRewardPointsMessage")]
        [AllowHtml]
        public string AddRewardPointsMessage { get; set; }

        public SendEmailModel SendEmail { get; set; }
        public SendPmModel SendPm { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.AssociatedExternalAuth")]
        public IList<AssociatedExternalAuthModel> AssociatedExternalAuthRecords { get; set; }

        public bool Deleted { get; set; }

        public TreeNode<IPermissionNode> PermissionTree { get; set; }
        public List<AddressModel> Addresses { get; set; }

        #region Nested classes

        public class AssociatedExternalAuthModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Customers.Customers.AssociatedExternalAuth.Fields.Email")]
            public string Email { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.AssociatedExternalAuth.Fields.ExternalIdentifier")]
            public string ExternalIdentifier { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.AssociatedExternalAuth.Fields.AuthMethodName")]
            public string AuthMethodName { get; set; }
        }

        public class RewardPointsHistoryModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Customers.Customers.RewardPoints.Fields.Points")]
            public int Points { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.RewardPoints.Fields.PointsBalance")]
            public int PointsBalance { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.RewardPoints.Fields.Message")]
            [AllowHtml]
            public string Message { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public class SendEmailModel : ModelBase
        {
            [AllowHtml]
            [Required]
            [SmartResourceDisplayName("Admin.Customers.Customers.SendEmail.Subject")]
            public string Subject { get; set; }

            [AllowHtml]
            [Required]
            [SmartResourceDisplayName("Admin.Customers.Customers.SendEmail.Body")]
            public string Body { get; set; }
        }

        public class SendPmModel : ModelBase
        {
            [Required]
            [SmartResourceDisplayName("Admin.Customers.Customers.SendPM.Subject")]
            public string Subject { get; set; }

            [Required]
            [SmartResourceDisplayName("Admin.Customers.Customers.SendPM.Message")]
            public string Message { get; set; }
        }

        public class OrderModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Customers.Customers.Orders.ID")]
            public override int Id { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.Orders.OrderStatus")]
            public string OrderStatus { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.Orders.PaymentStatus")]
            public string PaymentStatus { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.Orders.ShippingStatus")]
            public string ShippingStatus { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.Orders.OrderTotal")]
            public string OrderTotal { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.Orders.Store")]
            public string StoreName { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public partial class ActivityLogModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Customers.Customers.ActivityLog.ActivityLogType")]
            public string ActivityLogTypeName { get; set; }

            [SmartResourceDisplayName("Admin.Customers.Customers.ActivityLog.Comment")]
            public string Comment { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }

    public partial class CustomerValidator : AbstractValidator<CustomerModel>
    {
        public CustomerValidator(CustomerSettings customerSettings)
        {
            if (customerSettings.FirstNameRequired)
                RuleFor(x => x.FirstName).NotEmpty();

            if (customerSettings.LastNameRequired)
                RuleFor(x => x.LastName).NotEmpty();

            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
                RuleFor(x => x.Company).NotEmpty();

            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
                RuleFor(x => x.StreetAddress).NotEmpty();

            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
                RuleFor(x => x.StreetAddress2).NotEmpty();

            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
                RuleFor(x => x.ZipPostalCode).NotEmpty();

            if (customerSettings.CityRequired && customerSettings.CityEnabled)
                RuleFor(x => x.City).NotEmpty();

            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
                RuleFor(x => x.Phone).NotEmpty();

            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
                RuleFor(x => x.Fax).NotEmpty();
        }
    }
}