using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CMS.Models.DashboardViewModels;
using CMS.Services;
using Microsoft.AspNetCore.Identity;
using CMS.Models;
using Microsoft.Extensions.Logging;
using CMS.Models.BusinessModels;
using CMS.Models.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CMS.Services.RDS;
using CMS.Filters;
using CMS.Models.ActivityViewModels;

namespace CMS.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    [ServiceFilter(typeof(ValidateSessionAttribute))]
    public class DashboardController : BaseController
    {

            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly SignInManager<ApplicationUser> _signInManager;
            private readonly ILogger<DashboardController> _logger;
            private readonly IIdentityService _Identity;
            private readonly IRDS _RDS;

            public DashboardController(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<IdentityRole> roleManager,
                ILogger<DashboardController> logger,
                IIdentityService identity,
                IRDS RDS
            ) : base(logger)
            {
                _userManager = userManager;
                _roleManager = roleManager;
                _signInManager = signInManager;
                _logger = logger;
                _Identity = identity;
                _RDS = RDS;
            }

        public IActionResult Index()
        {

            _logger.LogInformation("Dashboard Loaded");
            DashboardModelView model = new DashboardModelView();
            model.Activities = _RDS.GetTransactions(new ResultOptions() { Results = new Results() { pageNumber = 1, pageSize = 50 } });                
            model.TotalCustomers = _Identity.GetCustomersTotal();
            model.TotalResellers = _Identity.GetResellersTotal();
            model.TotalResellersActive = _Identity.GetResellersActive();
            model.TotalMerchants = _Identity.GetMerchantsTotal();
            model.TotalCustomerPurchased = _RDS.GetTotalCustomerPurchased();
            model.TotalCustomerPurchasedFT = _RDS.GetTotalCustomerPurchasedFT();
            model.TotalCustomerTokens = _RDS.GetTotalCustomerTokens();
            model.TotalResellerSales = _RDS.GetTotalResellerSales();
            model.TotalResellerActiveFunds = _RDS.GetTotalResellerActiveFunds();
            model.TotalResellerBulkPurchased = _RDS.GetTotalResellerBulkPurchased();
            model.TotalResellerBuyBacks = _RDS.GetTotalResellerBuyBacks();
            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateResults([FromBody] ResultOptions options)
        {
            ActivityViewModel model = new ActivityViewModel();
            model = _RDS.GetTransactions(options);
            return PartialView("_Transactions", model);
        }
    }
}