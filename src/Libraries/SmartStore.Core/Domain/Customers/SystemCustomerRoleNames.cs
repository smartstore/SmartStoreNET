
namespace SmartStore.Core.Domain.Customers
{
    public static partial class SystemCustomerRoleNames
    {
		public static string SuperAdministrators { get { return "SuperAdmins"; } }

        public static string Administrators { get { return "Administrators"; } }
        
        public static string ForumModerators { get { return "ForumModerators"; } }

        public static string Registered { get { return "Registered"; } }

        public static string Guests { get { return "Guests"; } }
    }
}