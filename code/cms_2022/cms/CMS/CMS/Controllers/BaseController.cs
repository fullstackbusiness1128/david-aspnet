using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using CMS.Models;
using CMS.Models.AccountViewModels;
using CMS.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CMS.Filters;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using CMS.Models.BusinessModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CMS.Controllers
{
    public class BaseController : Controller
    {
        private readonly ILogger<BaseController> _logger;

        public BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

            base.OnActionExecuted(filterContext);

            // Don't process for AJAX Requests
            if (filterContext.HttpContext.Request.Headers["x-requested-with"] != "XMLHttpRequest")
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("Currency")))
                {
                    HttpContext.Session.SetString("Currency", "PGK");
                    HttpContext.Session.SetString("CurrencySymbol", "K");
                }
            }
        }
    }
}
