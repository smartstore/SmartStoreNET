using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Filters
{
    public class FilterDescriptor : RuleDescriptor
    {
        public Type EntityType { get; set; }
        public string Member { get; set; }
    }
}
