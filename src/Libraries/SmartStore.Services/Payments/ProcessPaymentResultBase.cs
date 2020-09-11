using System.Collections.Generic;

namespace SmartStore.Services.Payments
{
    public partial class ProcessPaymentResultBase
    {
        public ProcessPaymentResultBase()
        {
            this.Errors = new List<string>();
        }

        public IList<string> Errors { get; set; }

        public bool Success => (this.Errors.Count == 0);

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }
}
