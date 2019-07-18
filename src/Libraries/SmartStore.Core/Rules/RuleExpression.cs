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
        public RuleOperator Operator { get; set; }
        public object Value { get; set; }
        public string RawValue { get; set; }
    }
}
