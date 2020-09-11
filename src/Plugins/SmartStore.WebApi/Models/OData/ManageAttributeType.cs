using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.WebApi.Models.OData
{
    /// <summary>
    /// Data send by the consumer to manage product attributes.
    /// </summary>
    [Serializable]
    internal class ManageAttributeType
    {
        public ManageAttributeType()
        {
            Values = new List<ManageAttributeValue>();
            ControlTypeId = (int)AttributeControlType.DropdownList;
            IsRequired = true;
        }

        public string Name { get; set; }
        public int ControlTypeId { get; set; }
        public bool IsRequired { get; set; }

        public IList<ManageAttributeValue> Values { get; set; }

        [Serializable]
        public class ManageAttributeValue
        {
            public string Name { get; set; }
            public string Alias { get; set; }
            public string Color { get; set; }
            public decimal PriceAdjustment { get; set; }
            public decimal WeightAdjustment { get; set; }
            public bool IsPreSelected { get; set; }
        }
    }
}