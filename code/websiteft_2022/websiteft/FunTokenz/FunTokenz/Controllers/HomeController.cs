using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FunTokenz.Filter;
using FunTokenz.Models.Business;
using FunTokenz.Models.Data;
using FunTokenz.Models.View;
using FunTokenz.Services;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Globalization;
using System.Text;

namespace FunTokenz.Controllers
{
    [Authorize]
    [ValidateSessionAttribute]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRDS _RDS;
        private readonly IIdentity _Identity;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
                ILogger<HomeController> logger,
                IRDS RDS,
                IIdentity Identity,
                UserManager<ApplicationUser> userManager
        ) : base(logger, RDS, userManager)
        {
            _logger = logger;
            _RDS = RDS;
            _Identity = Identity;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        [MustBeSignedOut]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/terms")]
        public IActionResult Terms()
        {
            GeneralView view = new GeneralView();
            view.Title = "Terms";
            view.ContentSection = new ContentSection();
            return View("General",view);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/privacy")]
        public IActionResult Privacy()
        {
            GeneralView view = new GeneralView();
            view.Title = "Privacy";
            view.ContentSection = new ContentSection();
            return View("General", view);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/about")]
        public IActionResult About()
        {
            GeneralView view = new GeneralView();
            view.Title = "About";
            view.ContentSection = new ContentSection();
            return View("General", view);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/support")]
        public IActionResult Support()
        {
            GeneralView view = new GeneralView();
            view.Title = "Support";
            view.ContentSection = new ContentSection();
            return View("General", view);
        }



        /* * */

        /* PAYPAL */
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("/finalise")]
        //public IActionResult Finalise([FromBody] PayPalOutput output)
        //{

        //    _logger.LogInformation("Finalise Called");
        //    if (ModelState.IsValid)
        //    {
        //        _logger.LogInformation("Finalise - Model Is Valid");

        //        var save = _RDS.UpdatePurchase(output.JSON, output.Ref);

        //        if (save > 0)
        //        {
        //            // Send the SMS
        //            var token = _RDS.GetToken(save);

        //            if (token != null && !string.IsNullOrEmpty(token.Code))
        //            {
        //                _logger.LogInformation("Send SMS: " + token.Code);

        //                var expiry = DateTime.ParseExact(token.ExpiryTime.ToString(), "yyyyMMddHHmmssffff", CultureInfo.InvariantCulture).ToString("dd MMM yyyy");

        //                var sb = new StringBuilder()
        //                    .Append("Hello, a FunTokenz.com user has sent you tokens for use with our partners.\n\n")
        //                    .Append("You have been given: " + token.Currency + token.Value+ "\n\n")
        //                    .Append("Your token code is: " + token.Code + "\n\n")
        //                    .Append("Valid Till: " + expiry);
        //                var message = sb.ToString();


        //                AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.EUCentral1);
        //                PublishRequest pubRequest = new PublishRequest();
        //                pubRequest.Message = message;
        //                pubRequest.PhoneNumber = token.Phone;
        //                pubRequest.MessageAttributes.Add("AWS.SNS.SMS.SenderID", new MessageAttributeValue { StringValue = "FunTokenz", DataType = "String" });
        //                pubRequest.MessageAttributes.Add("AWS.SNS.SMS.SMSType", new MessageAttributeValue { StringValue = "Transactional", DataType = "String" });
        //                var pubResponse = snsClient.PublishAsync(pubRequest);

        //                Task.WhenAll(pubResponse);
        //            }

        //            _logger.LogInformation("Finalise - Save Is True");
        //            return PartialView("_Success");
        //        }
        //        _logger.LogInformation("Finalise - Save Is False");
        //    }
        //    _logger.LogInformation("Finalise - Model Is Invalid");

        //    return PartialView("_Failed");
        //}

        [HttpPost]
        [AllowAnonymous]
        [Route("/spend")]
        public JsonResult Spend([FromBody] Spend model)
        {

            _logger.LogInformation("Spend Called: " + JsonConvert.SerializeObject(model));

            FunTokenzResponse response = new FunTokenzResponse() { };
            if (ModelState.IsValid)
            {

                _logger.LogInformation("Spend Called: Valid");

                // Checks if Code Exists, Is Activated and Is Within Expiry Period - Returns Owner and Debits to check the Amount Available
                var check = _RDS.CheckSpend(model.Phone, model.CountryCode, model.Code);

                if (check != null && !string.IsNullOrEmpty(check.UserGuid) && check.Amount >= model.Amount)
                {

                    _logger.LogInformation("Spend Called - Check 1");

                    if (check.DebitRecords != null)
                    {

                        _logger.LogInformation("Spend Called - Check 2");

                        if (check.Amount >= (check.DebitRecords.Debits.Sum(s => s.DebitAmount) + model.Amount))
                        {
                            _logger.LogInformation("Spend Called - Check 3");

                            // OK
                            var spend = _RDS.SpendToken(check.TokenId, model.Amount, check.Amount - check.DebitRecords.Debits.Sum(s => s.DebitAmount) - model.Amount);
                            response.Processed = true;
                            response.Outcome = "Transaction was successful";
                        }
                        else
                        {
                            _logger.LogInformation("Spend Called - Check 4");

                            // Not Enough Credit Available
                            response.Processed = false;
                            response.Outcome = "There is not enough credit available";
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Spend Called - Check 5");

                        // No Prior Debit Records - OK ?
                        if (check.Amount >= model.Amount)
                        {
                            _logger.LogInformation("Spend Called - Check 6");

                            var spend = _RDS.SpendToken(check.TokenId, model.Amount, check.Amount - model.Amount);
                            response.Processed = true;
                            response.Outcome = "Transaction was successful";
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Spend Called - Check 7");

                    // NOT VALID
                    response.Processed = false;
                }
            }
            else
            {
                _logger.LogInformation("Spend Called - Check 8");

                // NOT VALID
                response.Processed = false;
            }

            JsonResult jsonResponse = new JsonResult(response);
            jsonResponse.StatusCode = 200;
            jsonResponse.ContentType = "application/json";
            return jsonResponse;
        }

    }
}
