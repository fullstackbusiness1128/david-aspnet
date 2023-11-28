using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FunTokenz.Filter;
using FunTokenz.Models.Business;
using FunTokenz.Models.Data;
using FunTokenz.Models.View;
using FunTokenz.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using Stripe;
using Amazon.SimpleNotificationService;
using System.Globalization;
using Amazon.SimpleNotificationService.Model;

namespace FunTokenz.Controllers
{

    [Authorize]
    [ValidateSessionAttribute]
    public class SettingsController : BaseController
    {

        private readonly ILogger<SettingsController> _logger;
        private readonly IIdentity _Identity;
        private readonly IRDS _RDS;
        private readonly IComms _Comms;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(
                ILogger<SettingsController> logger,
                IIdentity Identity,
                IRDS RDS,
                IComms Comms,
                UserManager<ApplicationUser> userManager
        ) : base(logger, RDS, userManager)
        {
            _logger = logger;
            _Identity = Identity;
            _userManager = userManager;
            _RDS = RDS;
            _Comms = Comms;
        }

        [HttpGet]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/settings/home")]
        public IActionResult Home()
        {
            SettingsView model = new SettingsView();
            var rates = _RDS.GetRates();
            Task.WhenAll(rates);
            model.Rates = rates.Result;
            model.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 20 } });
            return View(model);
        }

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/settings/home")]
        public IActionResult Home([FromBody] Results result)
        {
            SettingsView model = new SettingsView();
            model.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = result.pageNumber, pageSize = result.pageSize } });
            return PartialView("_Customers", model.CustomerRecords);
        }

        

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/settings/rates")]
        public IActionResult Rates([FromForm] RatesForm post)
        {

            SettingsView model = new SettingsView();

            if (ModelState.IsValid)
            {
                // Update The Rates
                var save = _RDS.UpdateRates(post);
            }

            var rates = _RDS.GetRates();
            Task.WhenAll(rates);

            model.Rates = rates.Result;
            return View("Home",model);
        }

    }
}
