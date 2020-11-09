using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.PayPal.Filters;
using SmartStore.PayPal.Services;
using SmartStore.Web.Controllers;

namespace SmartStore.PayPal
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<PayPalService>().As<IPayPalService>().InstancePerRequest();

            if (isActiveModule)
            {
                builder.RegisterType<PayPalExpressCheckoutFilter>().AsActionFilterFor<CheckoutController>(x => x.PaymentMethod()).InstancePerRequest();

                builder.RegisterType<PayPalExpressWidgetZoneFilter>().AsResultFilterFor<ShoppingCartController>(x => x.OffCanvasShoppingCart()).InstancePerRequest();

                builder.RegisterType<PayPalPlusCheckoutFilter>()
                    .AsActionFilterFor<CheckoutController>(x => x.PaymentMethod())
                    .InstancePerRequest();

                builder.RegisterType<PayPalPlusWidgetZoneFilter>()
                    .AsResultFilterFor<CheckoutController>(x => x.Completed())
                    .InstancePerRequest();

                builder.RegisterType<PayPalInstalmentsCheckoutFilter>()
                    .AsActionFilterFor<CheckoutController>()
                    .InstancePerRequest();

                //builder.RegisterType<PayPalFilter>().AsActionFilterFor<PublicControllerBase>();
            }
        }

        public int Order => 1;
    }
}
