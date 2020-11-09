namespace SmartStore.PayPal.Filters
{
    //public class PayPalFilter : IActionFilter
    //{
    //    private readonly Lazy<IWidgetProvider> _widgetProvider;
    //    private readonly Lazy<PayPalInstalmentsSettings> _payPalInstalmentsSettings;

    //    public PayPalFilter(
    //        Lazy<IWidgetProvider> widgetProvider,
    //        Lazy<PayPalInstalmentsSettings> payPalInstalmentsSettings)
    //    {
    //        _widgetProvider = widgetProvider;
    //        _payPalInstalmentsSettings = payPalInstalmentsSettings;
    //    }

    //    public void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //    }

    //    public void OnActionExecuted(ActionExecutedContext filterContext)
    //    {
    //        if (filterContext.IsChildAction)
    //            return;

    //        if (!filterContext.Result.IsHtmlViewResult())
    //            return;

    //        if (filterContext.HttpContext.Request.HttpMethod != "GET")
    //            return;

    //        // Promotion for instalments payment.
    //        if (_payPalInstalmentsSettings.Value.Promote && _payPalInstalmentsSettings.Value.PromotionWidgetZones.HasValue())
    //        {
    //            _widgetProvider.Value.RegisterAction(
    //                _payPalInstalmentsSettings.Value.PromotionWidgetZones.SplitSafe(","),
    //                "Promote",
    //                "PayPalInstalments",
    //                new { area = Plugin.SystemName },
    //                _payPalInstalmentsSettings.Value.PromotionDisplayOrder);
    //        }
    //    }
    //}
}