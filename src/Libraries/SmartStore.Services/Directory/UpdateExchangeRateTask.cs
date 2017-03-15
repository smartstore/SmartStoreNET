using System;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// Represents a task for updating exchange rates
    /// </summary>
    public partial class UpdateExchangeRateTask : ITask
    {
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
		private readonly ICommonServices _services;

        public UpdateExchangeRateTask(
			ICurrencyService currencyService, 
			CurrencySettings currencySettings,
			ICommonServices services)
        {
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
			this._services = services;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
		public void Execute(TaskExecutionContext ctx)
        {
            if (!_currencySettings.AutoUpdateEnabled)
                return;

            long lastUpdateTimeTicks = _currencySettings.LastUpdateTime;
            DateTime lastUpdateTime = DateTime.FromBinary(lastUpdateTimeTicks);
            lastUpdateTime = DateTime.SpecifyKind(lastUpdateTime, DateTimeKind.Utc);

            if (lastUpdateTime.AddHours(1) < DateTime.UtcNow)
            {
                // update rates every hour
                var exchangeRates = _currencyService.GetCurrencyLiveRates(_services.StoreContext.CurrentStore.PrimaryExchangeRateCurrency.CurrencyCode);

                foreach (var exchageRate in exchangeRates)
                {
                    var currency = _currencyService.GetCurrencyByCode(exchageRate.CurrencyCode);
                    if (currency != null)
                    {
						if (currency.Rate != exchageRate.Rate)
						{
							currency.Rate = exchageRate.Rate;
							_currencyService.UpdateCurrency(currency);
						}
                    }
                }

                // save new update time value
                _currencySettings.LastUpdateTime = DateTime.UtcNow.ToBinary();
				_services.Settings.SaveSetting(_currencySettings);
            }
        }
    }
}
