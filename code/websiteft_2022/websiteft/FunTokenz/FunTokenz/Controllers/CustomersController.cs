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
using System.Linq;
using System.Collections.Generic;

namespace FunTokenz.Controllers
{

    [Authorize]
    [ValidateSessionAttribute]
    public class CustomersController : BaseController
    {

        private readonly ILogger<CustomersController> _logger;
        private readonly IIdentity _Identity;
        private readonly IRDS _RDS;
        private readonly IComms _Comms;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(
                ILogger<CustomersController> logger,
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
        [RestrictedCustomerArea]
        [Route("/customers/home")]
        public IActionResult Home()
        {
            PurchasesView model = new PurchasesView();
            model = _RDS.GetPurchases(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 3 } });
            return View(model);
        }

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedCustomerArea]
        [Route("/customers/home")]
        public IActionResult Home([FromBody] Results result)
        {
            PurchasesView model = new PurchasesView();
            model = _RDS.GetPurchases(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = result.pageNumber, pageSize = result.pageSize } });
            return PartialView("_Tokens",model);
        }

        [HttpGet]
        [MustBeSignedIn]
        [RestrictedCustomerArea]
        [Route("/customers/tokens/{token}")]
        public IActionResult Token(Int32 token)
        {
            PurchaseView model = new PurchaseView();
            model = _RDS.GetPurchase(token);
            return View(model);
        }

        [HttpGet]
        [MustBeSignedIn]
        [RestrictedCustomerArea]
        [Route("/Customers/buy")]
        public IActionResult Buy(Int32 token)
        {
            _logger.LogInformation("Buy Loaded");
            PurchaseForm model = new PurchaseForm();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MustBeSignedIn]
        [RestrictedCustomerArea]
        [Route("/customers/confirm")]
        public IActionResult Confirm([FromForm] PurchaseForm model)
        {
            _logger.LogInformation("Confirm Loaded");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("PreSet Purchase");

                var exchange = "0.00001";
                if (model.Currency == "PGK")
                {
                    exchange = _RDS.GetExchangeRate("PGK", "AUD");
                   
                }
                else if (model.Currency == "FJD")
                {
                    exchange = _RDS.GetExchangeRate("FJD", "AUD");
                }
                
                var charge = ((model.Amount * Convert.ToDecimal(1.05)) / Convert.ToDecimal(exchange)).ToString("0.##");
                model.ExchangeRate = Convert.ToDecimal(exchange);
                model.ChargeAmount = Convert.ToDecimal(charge);

                HttpContext.Session.SetString("Amount", charge);
                HttpContext.Session.SetString("Value", model.Amount.ToString("0.##"));

                // Set the Purchase
                var save = _RDS.SetPurchase(model, false, null, "AUD");

                return View(model);
            }
            else
            {
                return View("Buy", model);
            }
        }

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedCustomerArea]
        public async Task<IActionResult> Charge([FromForm] StripePayment charge)
        {
            _logger.LogInformation("Charge Loaded");
            try
            {
                _logger.LogInformation("Charge - A: " + HttpContext.Session.GetString("Amount") + " | " + charge.Amount.ToString());
                var time = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff");

                if (!string.IsNullOrEmpty(charge.stripeToken) && HttpContext.Session.GetString("Amount") == charge.Amount.ToString())
                {
                    _logger.LogInformation("Charge: B");
                    // Get User
                    var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                    string customerRef = "";
                    if (!string.IsNullOrEmpty(user.StripeCustomerRef))
                    {
                        _logger.LogInformation("Charge: C");
                        // Update Customer
                        var update = new CustomerUpdateOptions
                        {
                            Source = charge.stripeToken,
                        };
                        var serviceUpdate = new CustomerService();
                        var updateResponse = serviceUpdate.Update(user.StripeCustomerRef, update);

                        customerRef = user.StripeCustomerRef;

                        _logger.LogInformation("Charge: C: " + customerRef);
                    }
                    else
                    {
                        _logger.LogInformation("Charge: D");
                        // Create Customer
                        var customer = new CustomerCreateOptions
                        {
                            Email = user.Email,
                            Name = user.Firstname + " " + user.Lastname,
                            Phone = user.PhoneNumber,
                            Source = charge.stripeToken,
                        };
                        var services = new CustomerService();
                        var created = services.Create(customer);

                        _logger.LogInformation("Charge: D Result: " + JsonConvert.SerializeObject(created));

                        customerRef = created.Id;

                        // Save Customer Reference
                        var setCustomerReference = await _Identity.SetStripeCustomerRef(customerRef);

                        _logger.LogInformation("Charge: D: " + customerRef);
                    }
                    _logger.LogInformation("Charge: E");

                    // Create Payment
                    var options = new ChargeCreateOptions
                    {
                        Amount = Convert.ToInt32(charge.Amount * 100),
                        Currency = "aud",
                        Customer = customerRef
                    };
                    var service = new ChargeService();
                    var payment = service.Create(options);

                    _logger.LogInformation("Charge: G | " + payment.Id + " | " + charge.Amount + " | " + Convert.ToDecimal(HttpContext.Session.GetString("Value")));

                    // Update Purchase
                    var save = _RDS.UpdatePurchase(null, null, payment.Id, Convert.ToDecimal(HttpContext.Session.GetString("Value")));

                    _logger.LogInformation("Charge: H");

                    // Save and send Token
                    if (save > 0)
                    {
                        _logger.LogInformation("Charge: 2");
                        // Send the SMS
                        var token = _RDS.GetToken(save);

                        _logger.LogInformation("Charge: 3");

                        if (token != null && !string.IsNullOrEmpty(token.Code))
                        {
                            _logger.LogInformation("Send SMS: " + token.Code);

                            var expiry = DateTime.ParseExact(token.ExpiryTime.ToString(), "yyyyMMddHHmmssffff", CultureInfo.InvariantCulture).ToString("dd MMM yyyy");

                            var sb = new StringBuilder()
                                .Append("Hello, a FunTokenz.com user has sent you tokens for use with our partners.\n\n")
                                .Append("You have been given: " + token.Currency + token.Value + "\n\n")
                                .Append("Your token code is: " + token.Code + "\n\n")
                                .Append("Valid Till: " + expiry);
                            var message = sb.ToString();

                            AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.EUCentral1);
                            PublishRequest pubRequest = new PublishRequest();
                            pubRequest.Message = message;
                            pubRequest.PhoneNumber = token.Phone;
                            pubRequest.MessageAttributes.Add("AWS.SNS.SMS.SenderID", new MessageAttributeValue { StringValue = "FunTokenz", DataType = "String" });
                            pubRequest.MessageAttributes.Add("AWS.SNS.SMS.SMSType", new MessageAttributeValue { StringValue = "Transactional", DataType = "String" });
                            var pubResponse = await snsClient.PublishAsync(pubRequest);

                            _logger.LogInformation("Charge: 4");

                        }

                        _logger.LogInformation("Finalise - Save Is True");
                        return View("Success");
                    }

                    _logger.LogInformation("Charge: I");

                }
            }
            catch (StripeException exc)
            {
                _logger.LogError("Stripe Error: " + exc);

            }
            _logger.LogInformation("Charge: J");

            return View("Failed");

        }


        


        [HttpPost]
        [MustBeSignedIn]
        [RestrictedCustomerArea]
        [Route("/customers/get_phones")]
        public JsonResult GetPhoneByCountryCode([FromBody] Phone phone)
        {
            List<ApplicationUser> mListUsers = new List<ApplicationUser>();
            mListUsers = (from a in _userManager.Users.Where(a => a.Type == "C" && a.CountryCode == phone.country_code) select a).ToList();
            return Json(mListUsers);
        }

    }
}
