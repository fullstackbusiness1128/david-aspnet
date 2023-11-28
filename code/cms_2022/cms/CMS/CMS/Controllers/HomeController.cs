using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CMS.Filters;
using CMS.Models.BusinessModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Controllers
{
    public class HomeController : BaseController
    {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) : base(logger)
        {
            _logger = logger;
        }


        public IActionResult Index()
        {
           // return RedirectToAction("Login", "Account");
            return RedirectPreserveMethod("~/Account/Login");
        }

        public IActionResult Error()
        {
            return View();
        }

        [Route("denied")]
        public IActionResult Denied()
        {
            return View();
        }
    }
}
