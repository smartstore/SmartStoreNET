using System;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents the style in which prices are displayed
    /// </summary>
    [Flags]
    public enum PriceDisplayStyle
    {
        /// <summary>
        /// Display prices without badges
        /// </summary>
        Default = 1,

        /// <summary>
        /// Display all prices within badges
        /// </summary>
        BadgeAll = 2,

        /// <summary>
        /// Display prices of free products within badges 
        /// </summary>
        BadgeFreeProductsOnly = 4
    }
}
