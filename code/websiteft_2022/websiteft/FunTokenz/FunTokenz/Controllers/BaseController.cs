using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using FunTokenz.Filter;
using FunTokenz.Models.Business;
using FunTokenz.Models.Data;
using FunTokenz.Models.View;
using FunTokenz.Services;
using Newtonsoft.Json;

namespace FunTokenz.Controllers
{
    public class BaseController : Controller
    {

        private readonly ILogger<BaseController> _logger;
        private readonly IRDS _RDS;
        private readonly UserManager<ApplicationUser> _userManager;

        public BaseController(ILogger<BaseController> logger, IRDS RDS, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _RDS = RDS;
            _userManager = userManager;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

            base.OnActionExecuted(filterContext);

            // Don't process for AJAX Requests
            if (filterContext.HttpContext.Request.Headers["x-requested-with"] != "XMLHttpRequest")
            {
                //If Signed In Get Common Values
                if (HttpContext.User.Identity.IsAuthenticated == true)
                {

                    if (string.IsNullOrEmpty(HttpContext.Session.GetString("FirstName")) || string.IsNullOrEmpty(HttpContext.Session.GetString("LastName")))
                    {
                        using (var user = _userManager.FindByIdAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value))
                        {
                            //Set Session
                            if (!string.IsNullOrEmpty(user.Result.Type)) { HttpContext.Session.SetString("Type", user.Result.Type); }
                            if (!string.IsNullOrEmpty(user.Result.Type) && user.Result.Type == "R") {
                                if (user.Result.ResellerBalance >= 0)
                                {
                                    HttpContext.Session.SetString("ResellerBalance", user.Result.ResellerBalance.ToString());
                                }

                                if (user.Result.ResellerProfit >= 0)
                                {
                                    HttpContext.Session.SetString("ResellerProfit", user.Result.ResellerProfit.ToString());
                                }


                                HttpContext.Session.SetString("ResellerValidated", Convert.ToInt32(user.Result.ResellerApproved).ToString());
                            }
                            if (!string.IsNullOrEmpty(user.Result.Firstname)) { HttpContext.Session.SetString("FirstName", user.Result.Firstname); }
                            if (!string.IsNullOrEmpty(user.Result.Lastname)) { HttpContext.Session.SetString("LastName", user.Result.Lastname); }
                            if (!string.IsNullOrEmpty(user.Result.PhoneNumber)) { HttpContext.Session.SetString("PhoneNumber", user.Result.PhoneNumber); }
                            if (!string.IsNullOrEmpty(user.Result.CountryCode)) { HttpContext.Session.SetString("PhoneCode", user.Result.CountryCode); }
                            if (!string.IsNullOrEmpty(user.Result.Email)) { HttpContext.Session.SetString("Email", user.Result.Email); }
                            if (!string.IsNullOrEmpty(user.Result.ResellerCurrency)) { HttpContext.Session.SetString("ResellerCurrency", user.Result.ResellerCurrency); }

                        }

                    }
                    else if(HttpContext.Session.GetString("Type") == "R")
                    {
                        using (var user = _userManager.FindByIdAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value))
                        {
                            if (!string.IsNullOrEmpty(user.Result.Type) && user.Result.Type == "R")
                            {
                                if (user.Result.ResellerBalance >= 0)
                                {
                                    HttpContext.Session.SetString("ResellerBalance", user.Result.ResellerBalance.ToString());
                                }

                                if (user.Result.ResellerProfit >= 0)
                                {
                                    HttpContext.Session.SetString("ResellerProfit", user.Result.ResellerProfit.ToString());
                                }
                                HttpContext.Session.SetString("ResellerValidated", Convert.ToInt32(user.Result.ResellerApproved).ToString());
                            }
                        }
                    }

                }
            }
        }
    }
}
