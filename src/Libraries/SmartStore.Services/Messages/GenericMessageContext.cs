using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Messages
{
    public class GenericMessageContext
    {
        public GenericMessageContext()
        {
            this.Tokens = new List<Token>();
        }
        public int? LanguageId { get; set; }
        public int? StoreId { get; set; }
        public IList<Token> Tokens { get; internal set; }
        public IMessageTokenProvider MessagenTokenProvider { get; internal set; }
        public Customer Customer { get; set; }
        public bool ReplyToCustomer { get; set; }
        public string ToEmail { get; set; }
        public string ToName { get; set; }
    }
}
