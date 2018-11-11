using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Filters
{
    
	public class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string[] _submitButtonNames;
        private readonly FormValueRequirement _requirement;
		private readonly FormValueRequirementRule _rule;
		private readonly bool _inverse;

        public FormValueRequiredAttribute(params string[] submitButtonNames):
            this(FormValueRequirement.Equal, FormValueRequirementRule.MatchAny, false, submitButtonNames)
        {
        }

        public FormValueRequiredAttribute(FormValueRequirement requirement, params string[] submitButtonNames)
			: this(requirement, FormValueRequirementRule.MatchAny, false, submitButtonNames)
        {
        }

		public FormValueRequiredAttribute(FormValueRequirementRule rule, params string[] submitButtonNames)
			: this(FormValueRequirement.Equal, rule, false, submitButtonNames)
		{
		}

		public FormValueRequiredAttribute(FormValueRequirement requirement, FormValueRequirementRule rule, params string[] submitButtonNames)
			: this(requirement, rule, false, submitButtonNames)
		{
		}

		protected internal FormValueRequiredAttribute(
			FormValueRequirement requirement, 
			FormValueRequirementRule rule, 
			bool inverse, 
			params string[] submitButtonNames)
		{
			// at least one submit button should be found (or being absent if 'inverse')
			this._submitButtonNames = submitButtonNames;
			this._requirement = requirement;
			this._rule = rule;
			this._inverse = inverse;
		}

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
			return IsValidForRequest(controllerContext.HttpContext.Request.Form);
		}

		protected internal virtual bool IsValidForRequest(NameValueCollection form)
		{
			try
			{
				bool isMatch = false;
				if (_rule == FormValueRequirementRule.MatchAny)
				{
					isMatch = _submitButtonNames.Any(x => IsMatch(form, x));
				}
				else
				{
					isMatch = _submitButtonNames.All(x => IsMatch(form, x));
				}
				return isMatch;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			return false;
		}

		private bool IsMatch(NameValueCollection form, string key)
		{
			string value = "";

			if (_requirement == FormValueRequirement.Equal)
			{
				// do not iterate because "Invalid request" exception can be thrown
				value = form[key];
			}
			else
			{
				var firstMatch = form.AllKeys.FirstOrDefault(x => x.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
				if (firstMatch != null)
				{
					value = form[firstMatch];
				}
			}

			if (_inverse)
			{
				return value.IsEmpty();
			}

			return value.HasValue();
		}
    }

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class FormValueAbsentAttribute : FormValueRequiredAttribute
	{
		public FormValueAbsentAttribute(params string[] submitButtonNames):
            base(FormValueRequirement.Equal, FormValueRequirementRule.MatchAny, true, submitButtonNames)
        {
        }

		public FormValueAbsentAttribute(FormValueRequirement requirement, params string[] submitButtonNames)
			: base(requirement, FormValueRequirementRule.MatchAny, true, submitButtonNames)
        {
        }

		public FormValueAbsentAttribute(FormValueRequirementRule rule, params string[] submitButtonNames)
			: base(FormValueRequirement.Equal, rule, true, submitButtonNames)
		{
		}

		public FormValueAbsentAttribute(FormValueRequirement requirement, FormValueRequirementRule rule, params string[] submitButtonNames)
			: base(requirement, rule, true, submitButtonNames)
		{
		}
	}

    public enum FormValueRequirement
    {
        Equal,
        StartsWith
    }

	public enum FormValueRequirementRule
	{
		MatchAny,
		MatchAll
	}
}
