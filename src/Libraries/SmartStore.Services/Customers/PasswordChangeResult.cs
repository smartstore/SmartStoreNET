using System.Collections.Generic;

namespace SmartStore.Services.Customers
{
    public class PasswordChangeResult
    {
        public IList<string> Errors { get; set; }

        public PasswordChangeResult()
        {
            this.Errors = new List<string>();
        }

        public bool Success => (this.Errors.Count == 0);

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }
    }
}
