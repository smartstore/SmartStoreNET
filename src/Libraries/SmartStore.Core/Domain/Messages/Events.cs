using System;

namespace SmartStore.Core.Domain.Messages
{
    public class EmailSubscribedEvent : IEquatable<EmailSubscribedEvent>
    {
        public EmailSubscribedEvent(string email)
        {
            Email = email;
        }

        public string Email { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as EmailSubscribedEvent);
        }

        public bool Equals(EmailSubscribedEvent other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Equals(other.Email, Email);
        }

        public override int GetHashCode()
        {
            return (Email != null ? Email.GetHashCode() : 0);
        }
    }

    public class EmailUnsubscribedEvent : IEquatable<EmailUnsubscribedEvent>
    {
        public EmailUnsubscribedEvent(string email)
        {
            Email = email;
        }

        public string Email { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as EmailUnsubscribedEvent);
        }

        public bool Equals(EmailUnsubscribedEvent other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Equals(other.Email, Email);
        }

        public override int GetHashCode()
        {
            return (Email != null ? Email.GetHashCode() : 0);
        }
    }
}
