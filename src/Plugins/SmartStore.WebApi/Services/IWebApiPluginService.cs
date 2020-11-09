using System.Linq;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.WebApi.Services
{
    public partial interface IWebApiPluginService
    {
        IQueryable<Customer> GetCustomers();

        bool CreateKeys(int customerId);

        void RemoveKeys(int customerId);

        void EnableOrDisableUser(int customerId, bool enable);
    }
}
