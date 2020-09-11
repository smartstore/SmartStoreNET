using System;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Represents a task for deleting guest customers.
    /// </summary>
    public partial class DeleteGuestsTask : ITask
    {
        private readonly ICustomerService _customerService;
        private readonly CommonSettings _commonSettings;

        public DeleteGuestsTask(
            ICustomerService customerService,
            CommonSettings commonSettings)
        {
            _customerService = customerService;
            _commonSettings = commonSettings;
        }

        public void Execute(TaskExecutionContext ctx)
        {
            Guard.NotNegative(_commonSettings.MaxGuestsRegistrationAgeInMinutes, nameof(_commonSettings.MaxGuestsRegistrationAgeInMinutes));

            var registrationTo = DateTime.UtcNow.AddMinutes(-_commonSettings.MaxGuestsRegistrationAgeInMinutes);

            _customerService.DeleteGuestCustomers(null, registrationTo, true);
        }
    }
}
