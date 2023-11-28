using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FunTokenz.Filter;
using FunTokenz.Models.Business;
using FunTokenz.Models.Data;
using FunTokenz.Models.View;
using FunTokenz.Services;
using System.Security.Claims;
using System.Linq.Dynamic.Core;

namespace FunTokenz.Controllers
{
    public class AccessController : Controller
    {
        private readonly ILogger<AccessController> _logger;
        private readonly IRDS _RDS;
        private readonly IIdentity _Identity;
        private readonly IComms _Comms;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccessController(
                ILogger<AccessController> logger, 
                IRDS RDS, 
                IIdentity Identity, 
                IComms Comms,
                UserManager<ApplicationUser> userManager, 
                SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _RDS = RDS;
            _Identity = Identity;
            _Comms = Comms;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("oops")]
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/signout")]
        public async Task<IActionResult> Signout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]   
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/customers/signin")]
        public IActionResult CustomersSignin(string returnUrl = null)
        {
            SignInView model = new SignInView();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/customers/signin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomersSignIn(SignInView model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (result.Succeeded && user.Type == "C")
                {
                    //if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("signin") || returnUrl.Contains("join") || returnUrl.Contains("reset-password"))
                    //{
                    //    return RedirectToLocal(Url.Content("~") + "/customers/home");
                    //}
                    //else
                    //{
                    //    return Redirect(returnUrl);
                    //}

                    return RedirectToLocal(Url.Content("~") + "/customers/home");
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    HttpContext.Session.Clear();

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                    
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [Route("/customers/join")]
        [MustBeSignedOut]
        [AllowAnonymous]
        public IActionResult CustomersJoin(string returnUrl = null)
        {
            JoinView model = new JoinView();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/customers/join")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomersJoin(JoinView model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {

                if (_Identity.IsUniquePhonenumber(model.CountryCode, model.Phone))
                {
                    // If phone number is not unique, something failed, redisplay form
                    ModelState.AddModelError(string.Empty, "Phone number already exists");
                    return View(model);
                }

                var user = new ApplicationUser { Type = "C", CompanyName = model.CompanyName, CompanyNumber = model.CompanyNumber, UserName = model.Email, Email = model.Email, PhoneNumber = model.Phone, CountryCode = model.CountryCode, DatetimeJoined = Convert.ToInt64(DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff")), Firstname = model.FirstName, Lastname = model.LastName, EmailConfirmed = false };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action(nameof(Confirmed), "Access", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(Url.Content("~") + "/customers/home");
                    
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /* * */

        [HttpGet]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/resellers/signin")]
        public IActionResult ResellersSignin(string returnUrl = null)
        {
            SignInView model = new SignInView();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/resellers/signin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResellersSignIn(SignInView model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (result.Succeeded && user.Type == "R")
                {
                    //if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("signin") || returnUrl.Contains("join") || returnUrl.Contains("reset-password"))
                    //{
                    //    return RedirectToLocal(Url.Content("~") + "/customers/home");
                    //}
                    //else
                    //{
                    //    return Redirect(returnUrl);
                    //}

                    if(user.ResellerApproved == true)
                    {
                        return RedirectToLocal(Url.Content("~") + "/resellers/home");
                    }

                    return RedirectToLocal(Url.Content("~") + "/resellers/awaiting-approval");
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    HttpContext.Session.Clear();

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [Route("/resellers/join")]
        [MustBeSignedOut]
        [AllowAnonymous]
        public IActionResult ResellersJoin(string returnUrl = null)
        {
            JoinView model = new JoinView();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/resellers/join")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResellersJoin(JoinView model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {

                
                if (_Identity.IsUniquePhonenumber(model.CountryCode, model.Phone))
                {
                    // If phone number is not unique in database, something failed, redisplay form
                    ModelState.AddModelError(string.Empty, "Phone number already exists");
                    return View(model);
                }

                var user = new ApplicationUser { Type = "R", ResellerApproved = false, ResellerCurrency = model.Currency, CompanyName = model.CompanyName, CompanyNumber = model.CompanyNumber, UserName = model.Email, Email = model.Email, PhoneNumber = model.Phone, CountryCode = model.CountryCode, DatetimeJoined = Convert.ToInt64(DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff")), Firstname = model.FirstName, Lastname = model.LastName, EmailConfirmed = false };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action(nameof(Confirmed), "Access", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(Url.Content("~") + "/resellers/awaiting-approval");
                    
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /* * */

        [HttpGet]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/merchants/signin")]
        public IActionResult MerchantsSignin(string returnUrl = null)
        {
            SignInView model = new SignInView();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/merchants/signin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MerchantsSignIn(SignInView model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (result.Succeeded && user.Type == "M")
                {
                    //if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("signin") || returnUrl.Contains("join") || returnUrl.Contains("reset-password"))
                    //{
                    //    return RedirectToLocal(Url.Content("~") + "/customers/home");
                    //}
                    //else
                    //{
                    //    return Redirect(returnUrl);
                    //}

                    return RedirectToLocal(Url.Content("~") + "/merchants/home");
                    
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    HttpContext.Session.Clear();
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [Route("/merchants/join")]
        [MustBeSignedOut]
        [AllowAnonymous]
        public IActionResult MerchantsJoin(string returnUrl = null)
        {
            JoinView model = new JoinView();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [MustBeSignedOut]
        [Route("/merchants/join")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MerchantsJoin(JoinView model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                if (_Identity.IsUniquePhonenumber(model.CountryCode, model.Phone))
                {
                    // If phone number is not unique in database, something failed, redisplay form
                    ModelState.AddModelError(string.Empty, "Phone number already exists");
                    return View(model);
                }

                var user = new ApplicationUser { Type = "M", CompanyName = model.CompanyName, CompanyNumber = model.CompanyNumber, UserName = model.Email, Email = model.Email, PhoneNumber = model.Phone, CountryCode = model.CountryCode, DatetimeJoined = Convert.ToInt64(DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff")), Firstname = model.FirstName, Lastname = model.LastName, EmailConfirmed = false };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action(nameof(Confirmed), "Access", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(Url.Content("~") + "/merchants/home");

                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /* * */

        [HttpGet]
        [Route("/confirmed")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirmed(string userId, string code)
        {

            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [Route("/reset")]
        [MustBeSignedOut]
        [AllowAnonymous]
        public IActionResult Reset()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/reset")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset([FromForm] ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return View("ResetResult");
                }
                else
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var url = Url.Action(nameof(NewPassword), "Access", new { code = code }, protocol: HttpContext.Request.Scheme);

                    Trace.WriteLine(url);

                    EmailForgot email = new EmailForgot { EmailAddressTo = user.Email, Link = url };
                    var sendResult = await _Comms.SendForgotPasswordEmail(email);
                    return View("ResetResult");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [MustBeSignedOut]
        [Route("/new-password")]
        [AllowAnonymous]
        public IActionResult NewPassword(string code = null)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Code = code;
            return code == null ? View("Error") : View(model);
        }

        [HttpPost]
        [Route("/new-password")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(AccessController.Reset), "Access");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return View("NewPasswordConfirmation");
            }
            AddErrors(result);
            return View(model);
        }

        [HttpGet]
        [MustBeSignedOut]
        [Route("/new-password-confirmed")]
        [AllowAnonymous]
        public IActionResult NewPasswordConfirmation()
        {
            return View();
        }

        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.Code == "InvalidToken")
                {
                    ModelState.AddModelError("Token", "The email entered is not valid for password reset");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                _logger.LogError("|" + HttpContext.Session.GetString("Uid") + "|" + HttpContext.TraceIdentifier + "| Error Code: " + error.Code + " | Error: " + error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
        #endregion
    }
}
