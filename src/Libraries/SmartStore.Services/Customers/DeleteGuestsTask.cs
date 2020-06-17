using System;
using System.Threading.Tasks;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Represents a task for deleting guest customers
    /// </summary>
    public partial class DeleteGuestsTask : AsyncTask
    {
        private readonly ICustomerService _customerService;

        public DeleteGuestsTask(ICustomerService customerService)
        {
            _customerService = customerService;
        }

		public override async Task ExecuteAsync(TaskExecutionContext ctx)
		{
			//60*24 = 1 day
			var olderThanMinutes = 1440; // TODO: move to settings
			await _customerService.DeleteGuestCustomersAsync(null, DateTime.UtcNow.AddMinutes(-olderThanMinutes), true);
		}
	}
}
