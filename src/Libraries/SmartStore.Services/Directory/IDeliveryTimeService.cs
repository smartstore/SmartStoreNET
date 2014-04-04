using System.Collections.Generic;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// DeliveryTime service
    /// </summary>
    public partial interface IDeliveryTimeService
    {
        /// <summary>
        /// Checks if the delivery time is associated with
        /// at least one dependant entity
        /// </summary>
        bool IsAssociated(int deliveryTimeId);
        
        /// <summary>
        /// Deletes delivery time
        /// </summary>
        /// <param name="deliveryTime">DeliveryTime</param>
        void DeleteDeliveryTime(DeliveryTime deliveryTime);

        /// <summary>
        /// Gets a delivery time
        /// </summary>
        /// <param name="deliveryTimeId">delivery time identifier</param>
        /// <returns>DeliveryTime</returns>
        DeliveryTime GetDeliveryTimeById(int deliveryTimeId);

        /// <summary>
        /// Gets all delivery times
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>delivery time collection</returns>
        IList<DeliveryTime> GetAllDeliveryTimes();

        /// <summary>
        /// Inserts a delivery time
        /// </summary>
        /// <param name="currency">DeliveryTime</param>
        void InsertDeliveryTime(DeliveryTime deliveryTime);

		void UpdateDeliveryTime(DeliveryTime deliveryTime);
    }
}