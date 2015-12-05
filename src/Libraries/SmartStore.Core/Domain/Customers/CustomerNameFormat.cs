namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents the customer name fortatting enumeration
    /// </summary>
    public enum CustomerNameFormat
    {
        /// <summary>
        /// Show emails
        /// </summary>
        ShowEmails = 1,
        /// <summary>
        /// Show usernames
        /// </summary>
        ShowUsernames = 2,
        /// <summary>
        /// Show full names
        /// </summary>
        ShowFullNames = 3,
		/// <summary>
		/// Show first name
		/// </summary>
		ShowFirstName = 4,
		/// <summary>
		/// Show shorted name and city
		/// </summary>
		ShowNameAndCity = 5
    }
}
