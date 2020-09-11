using System.Net.Mail;

namespace SmartStore.Core.Email
{
    public class EmailAddress
    {
        private readonly MailAddress _inner;

        public EmailAddress(string address)
        {
            _inner = new MailAddress(address);
        }

        public EmailAddress(string address, string displayName)
        {
            _inner = new MailAddress(address, displayName);
        }

        public EmailAddress(MailAddress address)
        {
            _inner = address;
        }

        public string Address => _inner.Address;

        public string DisplayName => _inner.DisplayName;

        public string User => _inner.User;

        public string Host => _inner.Host;

        public override int GetHashCode()
        {
            return _inner.GetHashCode();
        }

        public override string ToString()
        {
            return _inner.ToString();
        }

        public MailAddress ToMailAddress()
        {
            return _inner;
        }

        public static implicit operator string(EmailAddress obj)
        {
            return obj.ToString();
        }
    }
}
