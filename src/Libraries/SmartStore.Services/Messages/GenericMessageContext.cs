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
        public string ToEmail { get; set; }
        public string ToName { get; set; }

		/// <summary>
		/// Gets or sets a value specifying whether customer's email should be used as reply address
		/// </summary>
		/// <remarks>Value is ignored, if <c>Customer</c> property is <c>null</c></remarks>
		public bool ReplyToCustomer { get; set; }

		/// <summary>
		/// Gets or sets the reply email address
		/// </summary>
		/// <remarks>Value is ignored, if <c>ReplyToCustomer</c> is <c>true</c> AND <c>Customer</c> property is not <c>null</c></remarks>
		public string ReplyToEmail { get; set; }

		/// <summary>
		/// Gets or sets the reply to name
		/// </summary>
		/// <remarks>Value is ignored, if <c>ReplyToCustomer</c> is <c>true</c> AND <c>Customer</c> property is not <c>null</c></remarks>
		public string ReplyToName { get; set; }
    }
}
