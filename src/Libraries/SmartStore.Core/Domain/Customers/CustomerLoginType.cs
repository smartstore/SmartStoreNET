namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents the customer login type
    /// </summary>
    public enum CustomerLoginType
    {
        /// <summary>
        /// The username will be used to login
        /// </summary>
        Username = 10,

        /// <summary>
        /// The email will be used to login
        /// </summary>
        Email = 20,

        /// <summary>
        /// The username or the email address can be used to login
        /// </summary>
        UsernameOrEmail = 30
    }
}
