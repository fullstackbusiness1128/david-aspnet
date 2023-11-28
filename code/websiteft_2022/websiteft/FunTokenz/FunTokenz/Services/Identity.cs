using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using FunTokenz.Data;
using FunTokenz.Models.Business;
using FunTokenz.Models.Data;
using FunTokenz.Models.View;

namespace FunTokenz.Services
{
    public class Identity : IIdentity
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _SignInManager;
        private readonly ILogger<Identity> _loggerN;
        private readonly IHttpContextAccessor _accessor;

        public Identity(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> SignInManager, ILogger<Identity> logger, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _SignInManager = SignInManager;
            _loggerN = logger;
            _accessor = httpContextAccessor;
        }

        /* GET */

        public async Task<bool> EmailExist(string Email)
        {
            try
            {
                var a = (from b in _userManager.Users where b.Email.ToLower() == Email.ToLower() && b.Id != _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value select b.Email);

                ApplicationUser u = await _userManager.FindByEmailAsync(Email);
                if (u != null || a.Count() >= 1)
                {
                    return true;
                }
            }
            catch (Exception exc)
            {
                _loggerN.LogError(_accessor.HttpContext.Session.GetString("Uid") + "|" + _accessor.HttpContext.TraceIdentifier + " | General Exception: " + exc);
            }
            return false;
        }

        public async Task<bool> UpdatePassword(UpdatePassword password)
        {
            var updateUser = await _userManager.GetUserAsync(_accessor.HttpContext.User);
            var remove = await _userManager.RemovePasswordAsync(updateUser);
            var update = await _userManager.AddPasswordAsync(updateUser, password.Password);

            if (update.Succeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> SetStripeCustomerRef(string reference)
        {
            var updateUser = await _userManager.GetUserAsync(_accessor.HttpContext.User);

            if (!string.IsNullOrEmpty(reference))
            {
                updateUser.StripeCustomerRef = reference;
            }

            IdentityResult uresult = await _userManager.UpdateAsync(updateUser);
            if (uresult.Succeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<decimal> UpdateResellerBalance(decimal Amount)
        {
            _loggerN.LogInformation("Updating Balance");
            var updateUser = await _userManager.GetUserAsync(_accessor.HttpContext.User);

            updateUser.ResellerBalance = (updateUser.ResellerBalance + Amount);

            _loggerN.LogInformation("Updating Balance: " + updateUser.ResellerBalance);

            IdentityResult uresult = await _userManager.UpdateAsync(updateUser);
            if (uresult.Succeeded)
            {
                _loggerN.LogInformation("Updating Balance Succeded");
                return updateUser.ResellerBalance;
            }
            else
            {
                _loggerN.LogInformation("Updating Balance Failed");
                return updateUser.ResellerBalance;
            }
        }


        public async Task<decimal> UpdateResellerProfit(decimal profit)
        {
            var updateUser = await _userManager.GetUserAsync(_accessor.HttpContext.User);

            updateUser.ResellerProfit = (updateUser.ResellerProfit + profit);

            IdentityResult uresult = await _userManager.UpdateAsync(updateUser);
            if (uresult.Succeeded)
            {
                _loggerN.LogInformation("Updating ResellerProfit Succeded");
                return updateUser.ResellerProfit;
            }
            else
            {
                _loggerN.LogInformation("Updating ResellerProfit Failed");
                return updateUser.ResellerProfit;
            }
        }

        public async Task<decimal> ChargeResellerBalance(decimal Amount)
        {
            _loggerN.LogInformation("Updating Balance");
            var updateUser = await _userManager.GetUserAsync(_accessor.HttpContext.User);

            updateUser.ResellerBalance = (updateUser.ResellerBalance - Amount);

            _loggerN.LogInformation("Updating Balance: " + updateUser.ResellerBalance);

            IdentityResult uresult = await _userManager.UpdateAsync(updateUser);
            if (uresult.Succeeded)
            {
                _loggerN.LogInformation("Updating Balance Succeded");
                return updateUser.ResellerBalance;
            }
            else
            {
                _loggerN.LogInformation("Updating Balance Failed");
                return updateUser.ResellerBalance;
            }
        }


        public bool IsUniquePhonenumber(string countryCode, string phonenumber) 
        {
            var a =  (from b in _userManager.Users where b.CountryCode == countryCode && b.PhoneNumber == phonenumber select b.Email).Count();

            if (a > 0)
                return true;
            else
                return false;
        
        }


    }
}
