using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Controllers
{
    
	public class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string[] _submitButtonNames;
        private readonly FormValueRequirement _requirement;
		private readonly bool _inverse;

        public FormValueRequiredAttribute(params string[] submitButtonNames):
            this(FormValueRequirement.Equal, false, submitButtonNames)
        {
        }

        public FormValueRequiredAttribute(FormValueRequirement requirement, params string[] submitButtonNames)
			: this(requirement, false, submitButtonNames)
        {
        }

		protected internal FormValueRequiredAttribute(FormValueRequirement requirement, bool inverse, params string[] submitButtonNames)
		{
			// at least one submit button should be found (or being absent if 'inverse')
			this._submitButtonNames = submitButtonNames;
			this._requirement = requirement;
			this._inverse = inverse;
		}

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            foreach (string buttonName in _submitButtonNames)
            {
                try
                {
                    string value = "";
                    switch (this._requirement)
                    {
                        case FormValueRequirement.Equal:
                            {
                                // do not iterate because "Invalid request" exception can be thrown
                                value = controllerContext.HttpContext.Request.Form[buttonName];
                            }
                            break;
                        case FormValueRequirement.StartsWith:
                            {
                                foreach (var formValue in controllerContext.HttpContext.Request.Form.AllKeys)
                                {
                                    if (formValue.StartsWith(buttonName, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        value = controllerContext.HttpContext.Request.Form[formValue];
                                        break;
                                    }
                                }
                            }
                            break;
                    }

					if (!_inverse)
					{
						if (!String.IsNullOrEmpty(value))
							return true;
					}
					else
					{
						if (String.IsNullOrEmpty(value))
							return true;
					}
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                }
            }
            return false;
        }
    }

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class FormValueAbsentAttribute : FormValueRequiredAttribute
	{
		public FormValueAbsentAttribute(params string[] submitButtonNames):
            base(FormValueRequirement.Equal, true, submitButtonNames)
        {
        }

		public FormValueAbsentAttribute(FormValueRequirement requirement, params string[] submitButtonNames)
			: base(requirement, true, submitButtonNames)
        {
        }
	}

    public enum FormValueRequirement
    {
        Equal,
        StartsWith
    }
}
