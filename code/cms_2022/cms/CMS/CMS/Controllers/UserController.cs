using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CMS.Models.AccountViewModels;
using CMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CMS.Services;
using CMS.Models.UserViewModels;
using CMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CMS.Filters;
using System.Security.Claims;
using CMS.Models.ActivityViewModels;
using CMS.Models.BusinessModels;
using CMS.Services.RDS;
using System.Text;
using CMS.Models.DataModels.LLA;

namespace CMS.Controllers
{
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(ValidateSessionAttribute))]
    public class UsersController : BaseController
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UsersController> _logger;
        private readonly IIdentityService _Identity;
        private readonly IRDS _RDS;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger,
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

        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation("Users Loaded");
            UserViewModel model = new UserViewModel();
            model = _Identity.GetUsers(new ResultOptions() { Results = new Results() { pageNumber = 1, pageSize = 50 }, Filter = new Filter() { FilterA = "C" } });
            return View("Index",model);
        }

        [HttpPost]
        [Route("/users/updateresults")]
        public IActionResult UpdateResults([FromBody] ResultOptions options)
        {
            UserViewModel model = new UserViewModel();
            model = _Identity.GetUsers(options);
            return PartialView("_UserResults", model);
        }



        [HttpGet]
        [Route("/resellers")]
        public IActionResult ResellersIndex()
        {
            _logger.LogInformation("Users Loaded");
            UserViewModel model = new UserViewModel();
            model = _Identity.GetUsers(new ResultOptions() { Results = new Results() { pageNumber = 1, pageSize = 50 }, Filter = new Filter() { FilterA = "R" } });
            return View("Resellers", model);
        }


        [HttpPost]
        [Route("/resellers/updateresults")]
        public IActionResult ResellersUpdateResults([FromBody] ResultOptions options)
        {
            UserViewModel model = new UserViewModel();
            model = _Identity.GetUsers(options);
            return PartialView("_UserResults", model);
        }

        [HttpGet]
        [Route("/merchants")]
        public IActionResult MerchantsIndex()
        {
            _logger.LogInformation("Users Loaded");
            UserViewModel model = new UserViewModel();
            model = _Identity.GetUsers(new ResultOptions() { Results = new Results() { pageNumber = 1, pageSize = 50 }, Filter = new Filter() { FilterA = "M" } });
            return View("Merchants", model);
        }

        [HttpPost]
        [Route("/merchants/updateresults")]
        public IActionResult MerchantsUpdateResults([FromBody] ResultOptions options)
        {
            UserViewModel model = new UserViewModel();
            model = _Identity.GetUsers(options);
            return PartialView("_UserResults", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/resellers/status")]
        public async Task<bool> ToggleStatus([FromBody] Key key)
        {
            if (ModelState.IsValid)
            {
                var set = await _Identity.ToggleResellerStatus(key);
                return true;
            }
            return false;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/resellers/doc_status")]
        public async Task<bool> DocStatus([FromBody] Key key)
        {
            if (ModelState.IsValid)
            {
                var dbfile = await _RDS.ToggleDocumentStatus(Convert.ToInt32(key.Id), key.DocStatus);
                return dbfile;
            }
            return false;
        }


    }

}