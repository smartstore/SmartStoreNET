using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Orders
{
    public class OrderPaidEvent
    {
        private readonly Order _order;

        public OrderPaidEvent(Order order)
        {
            this._order = order;
        }

        public Order Order
        {
            get { return _order; }
        }
    }

    public class OrderPlacedEvent
    {
        private readonly Order _order;

        public OrderPlacedEvent(Order order)
        {
            this._order = order;
        }

        public Order Order
        {
            get { return _order; }
        }
    }

	public class OrderUpdatedEvent
	{
		private readonly Order _order;

		public OrderUpdatedEvent(Order order)
		{
			this._order = order;
		}

		public Order Order
		{
			get { return _order; }
		}
	}

	public class MigrateShoppingCartEvent
	{
		private readonly Customer _fromCustomer;
		private readonly Customer _toCustomer;

		public MigrateShoppingCartEvent(Customer fromCustomer, Customer toCustomer)
		{
			_fromCustomer = fromCustomer;
			_toCustomer = toCustomer;
		}

		public Customer FromCustomer
		{
			get { return _fromCustomer; }
		}

		public Customer ToCustomer
		{
			get { return _toCustomer; }
		}
	}
}