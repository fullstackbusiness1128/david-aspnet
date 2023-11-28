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
using System.Security.Cryptography;
using System.Linq;


namespace FunTokenz.Controllers
{

    [Authorize]
    [ValidateSessionAttribute]
    public class ResellersController : BaseController
    {

        private readonly ILogger<ResellersController> _logger;
        private readonly IIdentity _Identity;
        private readonly IRDS _RDS;
        private readonly IComms _Comms;
        private readonly UserManager<ApplicationUser> _userManager;

        public ResellersController(
                ILogger<ResellersController> logger,
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
        [Route("/resellers/home")]
        public IActionResult Home()
        {
            ResellerTransactionsView model = new ResellerTransactionsView();
            model.TransactionRecords = _RDS.GetResellerTransactions(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 5 } });
            return View(model);
        }

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/home")]
        public IActionResult Home([FromBody] Results result)
        {
            ResellerTransactionsView model = new ResellerTransactionsView();
            model.TransactionRecords = _RDS.GetResellerTransactions(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = result.pageNumber, pageSize = result.pageSize } });
            return PartialView("_Transactions", model.TransactionRecords);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/resellers/faq")]
        public IActionResult FAQ()
        {
            GeneralView view = new GeneralView();
            view.Title = "Resellers FAQ";
            view.ContentSection = new ContentSection();
            return View("General", view);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/resellers/awaiting-approval")]
        public IActionResult AwaitingApproval()
        {
            
            SettingsModel model = new SettingsModel();
            var result = _userManager.FindByIdAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Task.WhenAll(result);
            model.User = result.Result;
            model.AccountValidations = _RDS.GetFiles();

            return View(model);
        }


        /* * */

        [HttpGet]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/buy")]
        public IActionResult Buy(Int32 token)
        {
            _logger.LogInformation("Reseller Buy Loaded");
            ResellerPurchaseForm model = new ResellerPurchaseForm();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/confirm")]
        public IActionResult Confirm([FromForm] ResellerPurchaseForm model)
        {
            _logger.LogInformation("Reseller Confirm Loaded");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Reseller PreSet Purchase");

                var exchange = "";
                if (HttpContext.Session.GetString("ResellerCurrency") == "PGK")
                {
                    exchange = _RDS.GetExchangeRate("PGK", "AUD");
                }
                else if (HttpContext.Session.GetString("ResellerCurrency") == "FJD")
                {
                    exchange = _RDS.GetExchangeRate("FJD", "AUD");
                }

                var charge = ((model.Amount * Convert.ToDecimal(1.05)) / Convert.ToDecimal(exchange)).ToString("0.##");
                model.ExchangeRate = Convert.ToDecimal(exchange);
                model.ChargeAmount = Convert.ToDecimal(charge);
                model.ResellerCurrency = HttpContext.Session.GetString("ResellerCurrency");

                HttpContext.Session.SetString("Amount", charge);
                HttpContext.Session.SetString("Value", model.Amount.ToString("0.##"));

                // Set the Purchase
                var save = _RDS.SetResellerPurchase(model);

                return View(model);
            }
            else
            {
                return View("Buy", model);
            }
        }

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        public async Task<IActionResult> Charge([FromForm] StripePayment charge)
        {
            _logger.LogInformation("Reseller Charge called: ");

            try
            {
                var time = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff");

                if (!string.IsNullOrEmpty(charge.stripeToken) && HttpContext.Session.GetString("Amount") == charge.Amount.ToString())
                {
                    // Get User
                    var user = await _userManager.FindByIdAsync(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

                    string customerRef = "";
                    if (!string.IsNullOrEmpty(user.StripeCustomerRef))
                    {
                        // Update Customer
                        var update = new CustomerUpdateOptions
                        {
                            Source = charge.stripeToken,
                        };
                        var serviceUpdate = new CustomerService();
                        var updateResponse = serviceUpdate.Update(user.StripeCustomerRef, update);

                        customerRef = user.StripeCustomerRef;
                    }
                    else
                    {
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

                        customerRef = created.Id;

                        // Save Customer Reference
                        var setCustomerReference = await _Identity.SetStripeCustomerRef(customerRef);

                    }

                    // Create Payment
                    var options = new ChargeCreateOptions
                    {
                        Amount = Convert.ToInt32(charge.Amount * 100),
                        Currency = "aud",
                        Customer = customerRef
                    };
                    var service = new ChargeService();
                    var payment = service.Create(options);

                    // Update Purchase
                    var save = _RDS.UpdateResellerPurchase(null, null, payment.Id, Convert.ToDecimal(HttpContext.Session.GetString("Value")));

                    _logger.LogInformation("Reseller Purchase Finalised: " + save);

                    var balance = await _Identity.UpdateResellerBalance(Convert.ToDecimal(HttpContext.Session.GetString("Value")));
                    HttpContext.Session.SetString("ResellerBalance", balance.ToString());

                    _logger.LogInformation("Reseller Balance Updated: " + balance);

                    return View("Success");

                }
            }
            catch (StripeException exc)
            {
                _logger.LogError("Stripe Error: " + exc);

            }
            _logger.LogInformation("Charge: J");

            return View("Failed");

        }

        /* Sale */

        [HttpGet]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/sale")]
        public IActionResult Sale(Int32 token)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var current_user = _userManager.FindByIdAsync(userId);

            if (current_user.Result.ResellerBalance == 0)
            {
                ModelState.AddModelError("", "Your balance is zero.");
                ResellerTransactionsView m = new ResellerTransactionsView();
                m.TransactionRecords = _RDS.GetResellerTransactions(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 5 } });
                return View("Home", m);
            }


            _logger.LogInformation("Reseller Sale Loaded");
            CustomerSaleView model = new CustomerSaleView();
            model.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 2500 } });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/sale-confirm")]
        public IActionResult SaleConfirm([FromForm] CustomerSaleForm model)
        {
            _logger.LogInformation("Reseller Sale Confirm Loaded");


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var current_user = _userManager.FindByIdAsync(userId);

            if (ModelState.IsValid)
            {

                if (current_user.Result.ResellerBalance >= model.Amount)
                {

                    if (!string.IsNullOrEmpty(model.UserId) || (!string.IsNullOrEmpty(model.FirstName) && !string.IsNullOrEmpty(model.LastName) && !string.IsNullOrEmpty(model.SentToPhone) && !string.IsNullOrEmpty(model.SentToEmail) && !string.IsNullOrEmpty(model.CountryCode)))
                    {

                        _logger.LogInformation("Reseller PreSet Sale");

                        var rates = _RDS.GetRates();
                        Task.WhenAll(rates);
                        var charge = (model.Amount + (model.Amount * (Convert.ToDecimal(rates.Result.Purchase) / 100))).ToString("0.##");
                        HttpContext.Session.SetString("SaleAmount", charge);
                        HttpContext.Session.SetString("SaleValue", model.Amount.ToString("0.##"));


                        if (!string.IsNullOrEmpty(model.UserId))
                        {
                            var user = _userManager.FindByIdAsync(model.UserId);
                            Task.WhenAll(user);

                            model.FirstName = user.Result.Firstname;
                            model.LastName = user.Result.Lastname;
                            model.SentToEmail = user.Result.Email;
                            model.SentToPhone = user.Result.PhoneNumber;
                        }

                        return View(model);

                    }
                    else
                    {

                        ModelState.AddModelError("CustomerSaleForm", "Please input a user.");
                        CustomerSaleView view = new CustomerSaleView();
                        view.Form = model;
                        view.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 2500 } });

                        return View("Sale", view);
                    }

                }
                else
                {
                    ModelState.AddModelError("CustomerSaleForm", "Your balance is not enough.");
                    CustomerSaleView view = new CustomerSaleView();
                    view.Form = model;
                    view.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 2500 } });

                    return View("Sale", view);
                }

            }
            else
            {
                CustomerSaleView view = new CustomerSaleView();
                view.Form = model;
                view.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 2500 } });

                return View("Sale", view);
            }






        }

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/sale-process")]
        public async Task<IActionResult> SaleProcess(CustomerSaleForm model)
        {
            _logger.LogInformation("Reseller Charge called: ");

            var time = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff");

            if (ModelState.IsValid)
            {
                if (model.Amount == Convert.ToDecimal(HttpContext.Session.GetString("SaleValue")))
                {
                    if (!string.IsNullOrEmpty(model.UserId))
                    {

                        // Charge the Reseller
                        var charge = await _Identity.ChargeResellerBalance(Convert.ToDecimal(HttpContext.Session.GetString("SaleValue")));
                        var registerspend = _RDS.SpendResellerFunds(Convert.ToDecimal(HttpContext.Session.GetString("SaleValue")), charge, Convert.ToDecimal(HttpContext.Session.GetString("SaleAmount")));

                        // Get User
                        var user = await _userManager.FindByIdAsync(model.UserId);

                        // Award to user
                        var save = _RDS.SetPurchase(new PurchaseForm() { Amount = Convert.ToDecimal(HttpContext.Session.GetString("SaleValue")), ChargeAmount = Convert.ToDecimal(HttpContext.Session.GetString("SaleAmount")), ExchangeRate = 0, CountryCode = user.CountryCode, Currency = HttpContext.Session.GetString("ResellerCurrency"), Phone = model.SentToPhone }, true, user.Id, HttpContext.Session.GetString("ResellerCurrency"));
                        var mapCustomer = _RDS.MapCustomer(user.Id);

                        var profit = Convert.ToDecimal(HttpContext.Session.GetString("SaleAmount")) - Convert.ToDecimal(HttpContext.Session.GetString("SaleValue"));

                        var pro = await _Identity.UpdateResellerProfit(profit);

                        return View("SaleSuccess", model);
                    }
                    else if (!string.IsNullOrEmpty(model.FirstName) && !string.IsNullOrEmpty(model.LastName) && !string.IsNullOrEmpty(model.SentToPhone) && !string.IsNullOrEmpty(model.SentToEmail) && !string.IsNullOrEmpty(model.CountryCode))
                    {
                        // Charge the Reseller
                        var charge = await _Identity.ChargeResellerBalance(Convert.ToDecimal(HttpContext.Session.GetString("SaleValue")));
                        var registerspend = _RDS.SpendResellerFunds(Convert.ToDecimal(HttpContext.Session.GetString("SaleValue")), charge, Convert.ToDecimal(HttpContext.Session.GetString("SaleAmount")));

                        // Find User
                        var user = await _userManager.FindByEmailAsync(model.SentToEmail);

                        if (user == null || string.IsNullOrEmpty(user.Email))
                        {
                            // Create User
                            var bytes = new byte[4];
                            var rng = RandomNumberGenerator.Create();
                            rng.GetBytes(bytes);
                            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
                            var code = String.Format("{0:D8}", random);

                            var create = new ApplicationUser { Type = "C", CompanyName = null, CompanyNumber = null, UserName = model.SentToEmail, Email = model.SentToEmail, PhoneNumber = model.SentToPhone, Mobile = model.SentToPhone, CountryCode = model.CountryCode, DatetimeJoined = Convert.ToInt64(DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff")), Firstname = model.FirstName, Lastname = model.LastName, EmailConfirmed = false };
                            var result = await _userManager.CreateAsync(create, code);

                            user = await _userManager.FindByEmailAsync(model.SentToEmail);


                            var profit = Convert.ToDecimal(HttpContext.Session.GetString("SaleAmount")) - Convert.ToDecimal(HttpContext.Session.GetString("SaleValue"));

                            var pro = await _Identity.UpdateResellerProfit(profit);
                        }

                        // Award to user
                        var save = _RDS.SetPurchase(new PurchaseForm() { Amount = Convert.ToDecimal(HttpContext.Session.GetString("SaleValue")), ChargeAmount = Convert.ToDecimal(HttpContext.Session.GetString("SaleAmount")), ExchangeRate = 0, CountryCode = user.CountryCode, Currency = HttpContext.Session.GetString("ResellerCurrency"), Phone = model.SentToPhone }, true, user.Id, HttpContext.Session.GetString("ResellerCurrency"));

                        var mapCustomer = _RDS.MapCustomer(user.Id);


                       


                        return View("SaleSuccess", model);

                    }
                }
            }
            CustomerSaleView view = new CustomerSaleView();
            view.Form = model;
            view.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 2500 } });

            return View("Sale", view);

        }


        /* Redeem */

        [HttpGet]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/redeem")]
        public IActionResult Redeem(Int32 token)
        {
            _logger.LogInformation("Reseller Redeem Loaded");
            CustomerRedeemView model = new CustomerRedeemView();
            model.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 2500 } });

            return View(model);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/redeem-confirm")]
        public IActionResult RedeemConfirm([FromForm] CustomerRedeemForm model)
        {
            _logger.LogInformation("Reseller Redeem Confirm Loaded");
            if (ModelState.IsValid)
            {
                var code = model.FunTokenzCode1.ToString() + model.FunTokenzCode2.ToString() + model.FunTokenzCode3.ToString() + model.FunTokenzCode4.ToString() + model.FunTokenzCode5.ToString() + model.FunTokenzCode6.ToString() + model.FunTokenzCode7.ToString() + model.FunTokenzCode8.ToString();
                var user = _userManager.FindByIdAsync(model.UserId);
                Task.WhenAll(user);



                if (user.Result != null)
                {
                    // Checks if Code Exists, Is Activated and Is Within Expiry Period - Returns Owner and Debits to check the Amount Available
                    var check = _RDS.CheckSpend(user.Result.PhoneNumber, user.Result.CountryCode, code);

                    if (check != null && !string.IsNullOrEmpty(check.UserGuid) && check.Amount >= model.Amount)
                    {
                        _logger.LogInformation("Reseller Withdrawal - Check 1");
                        if (check.DebitRecords != null)
                        {
                            _logger.LogInformation("Reseller Withdrawal - Check 2");
                            if (check.Amount >= (check.DebitRecords.Debits.Sum(s => s.DebitAmount) + model.Amount))
                            {

                                var rates = _RDS.GetRates();
                                Task.WhenAll(rates);
                                var charge = (model.Amount - (model.Amount * (Convert.ToDecimal(rates.Result.Withdrawal) / 100))).ToString("0.##");
                                HttpContext.Session.SetString("RedeemAmount", charge);
                                HttpContext.Session.SetString("RedeemValue", model.Amount.ToString("0.##"));

                                if (!string.IsNullOrEmpty(model.UserId))
                                {
                                    model.Email = user.Result.Email;
                                    model.Phone = user.Result.PhoneNumber;
                                    model.CountryCode = user.Result.CountryCode;
                                }
                                return View(model);

                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("FunTokenzCode1", "The token provided is not valid or funds are not available to withdraw");
                    }
                }
                else
                {
                    ModelState.AddModelError("FunTokenzCode1", "The user doesn't exist");
                }





            }
            CustomerRedeemView view = new CustomerRedeemView();
            view.Form = model;
            view.CustomerRecords = _RDS.GetResellerCustomers(new Options() { Filter = new Models.Business.Filter(), Results = new Results() { pageNumber = 1, pageSize = 2500 } });
            return View("Redeem", view);
        }

        [HttpPost]
        [MustBeSignedIn]
        [RestrictedResellerArea]
        [RestrictedResellerValidatedArea]
        [Route("/resellers/redeem-process")]
        public async Task<IActionResult> RedeemProcess(CustomerRedeemForm model)
        {
            _logger.LogInformation("Reseller Charge called: ");

            var time = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff");

            if (ModelState.IsValid)
            {
                var code = model.FunTokenzCode1.ToString() + model.FunTokenzCode2.ToString() + model.FunTokenzCode3.ToString() + model.FunTokenzCode4.ToString() + model.FunTokenzCode5.ToString() + model.FunTokenzCode6.ToString() + model.FunTokenzCode7.ToString() + model.FunTokenzCode8.ToString();

                // Checks if Code Exists, Is Activated and Is Within Expiry Period - Returns Owner and Debits to check the Amount Available
                var check = _RDS.CheckSpend(model.Phone, model.CountryCode, code);

                if (check != null && !string.IsNullOrEmpty(check.UserGuid) && check.Amount >= model.Amount)
                {
                    _logger.LogInformation("Reseller Withdrawal - Check 1");
                    if (check.DebitRecords != null)
                    {
                        _logger.LogInformation("Reseller Withdrawal - Check 2");
                        if (check.Amount >= (check.DebitRecords.Debits.Sum(s => s.DebitAmount) + model.Amount))
                        {
                            _logger.LogInformation("Reseller Withdrawal - Check 3");

                            // OK - Spend from customers token
                            var spend = _RDS.SpendCustomerToken(check.TokenId, model.Amount, check.Amount - check.DebitRecords.Debits.Sum(s => s.DebitAmount) - model.Amount);

                            // OK - Deposit into Reseller Balance
                            var save = _RDS.SetResellerBuyBack(new ResellerPurchaseForm() { Amount = model.Amount, ChargeAmount = Convert.ToDecimal(HttpContext.Session.GetString("RedeemAmount")), ResellerCurrency = HttpContext.Session.GetString("ResellerCurrency") }, spend);

                            var balance = await _Identity.UpdateResellerBalance(model.Amount);
                            HttpContext.Session.SetString("ResellerBalance", balance.ToString());

                            _logger.LogInformation("Reseller Balance Updated: " + balance);


                            var profit = model.Amount - Convert.ToDecimal(HttpContext.Session.GetString("RedeemAmount"));
                            var pro = await _Identity.UpdateResellerProfit(profit);

                            return View("RedeemSuccess");
                        }
                    }
                }
            }

            return View("RedeemFailed");

        }
    }
}
