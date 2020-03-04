using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Updates the assignments to customer roles for rules.
    /// </summary>
    public partial class CustomerRolesAssignmentsTask : AsyncTask
    {
        public CustomerRolesAssignmentsTask()
        {
        }

        public override Task ExecuteAsync(TaskExecutionContext ctx)
        {
            return null;
        }
    }
}
