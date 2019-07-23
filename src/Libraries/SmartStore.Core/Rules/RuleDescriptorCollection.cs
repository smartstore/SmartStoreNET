using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public class RuleDescriptorCollection : Collection<RuleDescriptor>
    {
        public RuleDescriptorCollection()
        {
        }

        public RuleDescriptorCollection(IList<RuleDescriptor> descriptors)
            : base(descriptors)
        {
        }

        public RuleDescriptor FindDescriptor(string name)
        {
            Guard.NotEmpty(name, nameof(name));
            
            // TODO: Impl
            return null;
        }
    }
}
