using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.DirectDebit.Models
{
    public class PaymentInfoModel : ModelBase
    {
        public string DescriptionText { get; set; }


        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.EnterIBAN")]
        public string EnterIBAN { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitAccountHolder")]
        public string DirectDebitAccountHolder { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitAccountNumber")]
        public string DirectDebitAccountNumber { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitBankCode")]
        public string DirectDebitBankCode { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitCountry")]
        public string DirectDebitCountry { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitBankName")]
        public string DirectDebitBankName { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitIban")]
        public string DirectDebitIban { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitBic")]
        public string DirectDebitBic { get; set; }

        public List<SelectListItem> Countries { get; set; }
    }
}