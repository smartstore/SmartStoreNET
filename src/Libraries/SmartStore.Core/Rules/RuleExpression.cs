using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public class RuleExpression
    {
        public RuleDescriptor Descriptor { get; set; }
        public RuleOperation Operator { get; set; }
        public object Value { get; set; }
        public object UpperValue { get; set; }
        public string RawValue { get; set; }
        public string RawUpperValue { get; set; }
    }
}
