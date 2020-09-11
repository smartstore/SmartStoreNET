using System;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Modelling.Results
{
    public class RootActionViewResult : ViewResult
    {
        protected override ViewEngineResult FindView(ControllerContext context)
        {
            if (base.ViewName.IsEmpty())
                throw new InvalidOperationException($"{nameof(ViewName)} is required when using {nameof(RootActionViewResult)}.");

            return base.FindView(context.GetRootControllerContext());
        }
    }

    public class RootActionPartialViewResult : PartialViewResult
    {
        protected override ViewEngineResult FindView(ControllerContext context)
        {
            if (base.ViewName.IsEmpty())
                throw new InvalidOperationException($"{nameof(ViewName)} is required when using {nameof(RootActionPartialViewResult)}.");

            return base.FindView(context.GetRootControllerContext());
        }
    }
}
