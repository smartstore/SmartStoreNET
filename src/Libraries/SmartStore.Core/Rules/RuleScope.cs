using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public enum RuleScope
    {
        Cart = 0,
        OrderItem = 1,
        Customer = 2,
        Product = 3,
    }
}
