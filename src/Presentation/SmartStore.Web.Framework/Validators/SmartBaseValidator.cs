using System.Web.Mvc;
using FluentValidation;

namespace SmartStore.Web.Framework.Validators
{
	public abstract class SmartBaseValidator<T> : AbstractValidator<T> where T : class
	{
		protected SmartBaseValidator()
		{
		}

		public void Validate(T model, ModelStateDictionary modelState)
		{
			var result = Validate(model);

			if (!result.IsValid)
			{
				int i = 0;
				foreach (var error in result.Errors)
				{
					modelState.AddModelError(error.PropertyName + (++i).ToString(), error.ErrorMessage);
				}
			}
		}
	}
}
