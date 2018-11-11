namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents the customer number method
    /// </summary>
    public enum CustomerNumberMethod
    {
        /// <summary>
        /// no customer number will be saved
        /// </summary>
        Disabled = 10,

        /// <summary>
        /// customer numbers can be saved
        /// </summary>
        Enabled = 20,

        /// <summary>
        /// customer numbers will automatically be set when new customers are created
        /// </summary>
        AutomaticallySet = 30,

    }
}
