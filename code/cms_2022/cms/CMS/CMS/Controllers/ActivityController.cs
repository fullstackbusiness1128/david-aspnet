using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
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
using CMS.Services.RDS;
using CMS.Services.Cache;
using CMS.Models.ActivityViewModels;
using CMS.Models.ViewModels;

namespace CMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ActivityController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ActivityController> _logger;
        private readonly IRDS _RDS;
        private readonly ICacheService _Cache;

        public ActivityController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<ActivityController> logger,
            IRDS RDS,
            ICacheService Cache) : base(logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _RDS = RDS;
            _Cache = Cache;
        }

        [HttpGet]
        [Route("/activity")]
        public IActionResult Index()
        {
            _logger.LogInformation("Dashboard Loaded");
            ActivityViewModel model = new ActivityViewModel();
            model = _RDS.GetTransactions(new ResultOptions() { Results = new Results() { pageNumber = 1, pageSize = 50 } });
            return View(model);
        }

        [HttpGet]
        [Route("/activity/{UserId}")]
        public IActionResult Index(string UserId)
        {
            _logger.LogInformation("Dashboard Loaded");
            ActivityViewModel model = new ActivityViewModel();
            model = _RDS.GetTransactions(new ResultOptions() { Results = new Results() { pageNumber = 1, pageSize = 50 }, Filter = new Filter() { FilterA = UserId } });
            return View(model);
        }

        [HttpPost]
        [Route("/activity/updateresults")]
        public IActionResult UpdateResults([FromBody] ResultOptions options)
        {
            ActivityViewModel model = new ActivityViewModel();
            model = _RDS.GetTransactions(options);
            return PartialView("_Transactions", model);
        }

    }
}
