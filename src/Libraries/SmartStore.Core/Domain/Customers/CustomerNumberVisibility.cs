namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents the customer visibility in the frontend
    /// </summary>
    public enum CustomerNumberVisibility
    {
        /// <summary>
        /// Customer number won't be displayed in the frontend
        /// </summary>
        None = 10,

        /// <summary>
        /// Customer number will be displayed in the frontend
        /// </summary>
        Display = 20,

        /// <summary>
        /// A customer can enter his own number if customer number wasn't saved yet
        /// </summary>
        EditableIfEmpty = 30,

        /// <summary>
        /// A customer can enter his own number and alter it
        /// </summary>
        Editable = 40,
    }
}
