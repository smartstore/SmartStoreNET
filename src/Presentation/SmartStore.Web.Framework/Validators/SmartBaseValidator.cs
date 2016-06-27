using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;

namespace SmartStore.Web.Framework.Validators
{
	public abstract class SmartValidatorBase<T> : AbstractValidator<T> where T : class
	{
		protected SmartValidatorBase()
		{
		}

		public void Validate(T model, ModelStateDictionary modelState)
		{
			var result = Validate(model);

			if (!result.IsValid)
			{
				foreach (var error in result.Errors)
				{
					try
					{
						modelState.AddModelError(error.PropertyName, error.ErrorMessage);
					}
					catch { }
				}
			}
		}

		public bool Validate(T model, List<string> warnings)
		{
			var result = Validate(model);

			if (!result.IsValid)
			{
				foreach (var error in result.Errors)
					warnings.Add(error.ErrorMessage);

				return false;
			}
			return true;
		}
	}
}
