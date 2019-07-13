using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public interface IRuleMetadataAccessor
    {
        RuleMetadata Metadata { get; }
    }
}
