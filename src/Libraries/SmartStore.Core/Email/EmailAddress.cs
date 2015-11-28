using System;
using System.Net.Mail;
using System.Text;

namespace SmartStore.Core.Email
{
    public class EmailAddress
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }

        public EmailAddress(string address)
        {
            this.Address = address;
        }

        public EmailAddress(string address, string displayName)
        {
            this.Address = address;
            this.DisplayName = displayName;
        }

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public override string ToString()
		{
			if (this.DisplayName.IsEmpty())
			{
				return this.Address;
			}

			return "{0} [{1}]".FormatCurrent(this.DisplayName, this.Address);
		}

		public MailAddress ToMailAddress()
		{
			return new MailAddress(this.Address, this.DisplayName);
		}
    }
}
