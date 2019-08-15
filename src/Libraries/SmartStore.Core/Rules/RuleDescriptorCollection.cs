using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public class RuleDescriptorCollection : KeyedCollection<string, RuleDescriptor>
    {
        public RuleDescriptorCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public RuleDescriptorCollection(IEnumerable<RuleDescriptor> descriptors)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Guard.NotNull(descriptors, nameof(descriptors));

            descriptors.Each(x => Add(x));
        }

        protected override string GetKeyForItem(RuleDescriptor item)
        {
            return item.Name;
        }

        public RuleDescriptor FindDescriptor(string name)
        {
            return this[name];
        }
    }
}
