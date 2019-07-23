using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    [Flags]
    public enum RuleScope
    {
        Cart = 1,
        OrderItem = 2,
        Customer = 4,
        Product = 8,
    }
}
