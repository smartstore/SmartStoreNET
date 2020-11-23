using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Cart
{
    public class ValidatingCartEvent
    {
        public ValidatingCartEvent(
            Customer customer, 
            IList<string> warnings,
            IList<OrganizedShoppingCartItem> cart)
        {
            Customer = customer;
            Warnings = warnings;
            Cart = cart;
        }

        public Customer Customer { get; set; }

        public IList<OrganizedShoppingCartItem> Cart { get; set; }

        public IList<string> Warnings { get; set; }

        public ActionResult Result { get; set; }
    }
}