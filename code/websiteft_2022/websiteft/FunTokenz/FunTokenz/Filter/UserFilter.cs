using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FunTokenz.Filter
{
    public class MustBeSignedIn : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == false)
            {
                // Sign Out
                context.HttpContext.Session.Clear();
                var returnUrl = string.Format("{0}://{1}{2}{3}", context.HttpContext.Request.Scheme, context.HttpContext.Request.Host, context.HttpContext.Request.Path, context.HttpContext.Request.QueryString);
                context.HttpContext.Response.Redirect("~/?returnUrl=" + returnUrl);
            }
        }
    }

    public class MustBeSignedOut : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == true)
            {
                if(context.HttpContext.Session.GetString("Type") == "C")
                {
                    context.HttpContext.Response.Redirect("/customers/home");
                }
                else if (context.HttpContext.Session.GetString("Type") == "M")
                {
                    context.HttpContext.Response.Redirect("/merchants/home");
                }
                else if (context.HttpContext.Session.GetString("Type") == "R")
                {
                    context.HttpContext.Response.Redirect("/resellers/home");
                }
                
            }
        }
    }

    public class RestrictedCustomerArea : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == true)
            {
                if (context.HttpContext.Session.GetString("Type") == "M")
                {
                    context.HttpContext.Response.Redirect("/merchants/home");
                }
                else if (context.HttpContext.Session.GetString("Type") == "R")
                {
                    context.HttpContext.Response.Redirect("/resellers/home");
                }
            }
        }
    }

    public class RestrictedMerchantArea : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == true)
            {
                if (context.HttpContext.Session.GetString("Type") == "C")
                {
                    context.HttpContext.Response.Redirect("/customers/home");
                }
                else if (context.HttpContext.Session.GetString("Type") == "R")
                {
                    context.HttpContext.Response.Redirect("/resellers/home");
                }
            }
        }
    }

    public class RestrictedResellerArea : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == true)
            {
                if (context.HttpContext.Session.GetString("Type") == "M")
                {
                    context.HttpContext.Response.Redirect("/merchants/home");
                }
                else if (context.HttpContext.Session.GetString("Type") == "C")
                {
                    context.HttpContext.Response.Redirect("/customers/home");
                }
            }
        }
    }

    public class RestrictedResellerValidatedArea : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == true)
            {
                if (context.HttpContext.Session.GetString("ResellerValidated") != "1")
                {
                    context.HttpContext.Response.Redirect("/resellers/awaiting-approval");
                }
            }
        }
    }
}
