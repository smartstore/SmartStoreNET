using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Domain.Catalog
{
    
    /// <summary>
    /// ComplexType required for base price quotations ("PAnGV" in Germany)
    /// </summary>
    public class BasePriceQuotation
    {

        /// <summary>
        /// Gets or sets if base price quotation (PAnGV) is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Measure unit for the base price (e.g. "kg", "g", "qm²" etc.)
        /// </summary>
        public string MeasureUnit { get; set; }

        /// <summary>
        /// Amount of product per packing unit in the given measure unit 
        /// (e.g. 250 ml shower gel: "0.25" if MeasureUnit = "liter" and BaseAmount = 1)
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Reference value for the given measure unit 
        /// (e.g. "1" liter. Formula: [BaseAmount] [MeasureUnit] = [SellingPrice] / [Amount])
        /// </summary>
        public int? BaseAmount { get; set; }

        public bool HasValue
        {
            get
            {
                return Enabled && Amount.GetValueOrDefault() > 0 && BaseAmount.GetValueOrDefault() > 0 && MeasureUnit.HasValue();
            }
        }

    }

}
