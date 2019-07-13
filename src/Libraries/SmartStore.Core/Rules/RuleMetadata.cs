using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public class RuleMetadata
    {
        public RuleTypeCode TypeCode { get; set; }
        public string[] Operators { get; set; }
        public IEnumerable<IRuleConstraint> Constraints { get; set; }
        public string Editor { get; set; }
    }

    public interface IRuleConstraint
    {
        bool Match(RuleExpression expression);
    }
}
