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
    public class MerchantsController : BaseController
    {

        private readonly ILogger<MerchantsController> _logger;
        private readonly IIdentity _Identity;
        private readonly IRDS _RDS;
        private readonly IComms _Comms;
        private readonly UserManager<ApplicationUser> _userManager;

        public MerchantsController(
                ILogger<MerchantsController> logger,
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
        [RestrictedMerchantArea]
        [Route("/merchants/home")]
        public IActionResult Home()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [RestrictedMerchantArea]
        [Route("/merchants/api")]
        public IActionResult API()
        {
            GeneralView view = new GeneralView();
            view.Title = "Merchants API";
            view.ContentSection = new ContentSection();
            return View("General", view);
        }
    }
}

