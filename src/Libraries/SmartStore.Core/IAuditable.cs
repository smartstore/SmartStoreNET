using System;

namespace SmartStore.Core
{
    public interface IAuditable
    {
        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity update
        /// </summary>
        DateTime UpdatedOnUtc { get; set; }
    }
}
