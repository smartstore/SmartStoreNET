using FluentValidation;
using SmartStore.Admin.Models.Directory;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;

namespace SmartStore.Admin.Validators.Tasks
{
	public partial class ScheduleTaskValidator : AbstractValidator<ScheduleTaskModel>
    {
        public ScheduleTaskValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.ScheduleTasks.Name.Required"));
            //RuleFor(x => x.Seconds).GreaterThan(0).WithMessage(localizationService.GetResource("Admin.System.ScheduleTasks.Seconds.Positive"));
			RuleFor(x => x.CronExpression).Must(x => CronExpression.IsValid(x)).WithMessage("Invalid CRON expression"); // TODO Loc
        }
    }
}