using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class TopCustomerReportLineModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderCount")]
        public string OrderCount { get; set; }

        public string CustomerDisplayName { get; set; }

        [SmartResourceDisplayName("Admin.Common.Entity.Fields.Id")]
        public int CustomerId { get; set; }

        [SmartResourceDisplayName("Account.Fields.CustomerNumber")]
        public string CustomerNumber { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.FullName")]
        public string FullName { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Active")]
        public bool Active { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }
    }
}