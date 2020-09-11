using System;
using System.Collections.Generic;

namespace SmartStore.Services.Themes
{
    public class ThemeValidationException : Exception
    {
        public ThemeValidationException(string message, IDictionary<string, object> attemptedVars)
            : base(message)
        {
            Guard.NotNull(attemptedVars, nameof(attemptedVars));

            AttemptedVars = attemptedVars;
        }

        public IDictionary<string, object> AttemptedVars { get; private set; }
    }
}
