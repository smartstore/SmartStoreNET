namespace SmartStore.Services.Security
{
    // e.g. [Permission(PermissionSystemNames.Customer.Read)]
    public static class PermissionSystemNames
    {
        public static class Customer
        {
            public const string Self = "customer";
            public const string Read = "customer.read";
            public const string Update = "customer.update";
            public const string Create = "customer.create";
            public const string Delete = "customer.delete";
            public const string Impersonate = "customer.impersonate";

            public static class Role
            {
                public const string Self = "customer.role";
                public const string Read = "customer.role.read";
                public const string Update = "customer.role.update";
                public const string Create = "customer.role.create";
                public const string Delete = "customer.role.delete";
            }
        }

        public static class Order
        {
            public const string Self = "order";
            public const string Read = "order.read";
            //...
        }
        //...
    }
}
