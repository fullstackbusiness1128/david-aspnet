using CMS.Models;
using CMS.Services.RDS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Filters
{
    public class ValidateSessionAttribute : ActionFilterAttribute
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRDS _LLA;
        private readonly IHttpContextAccessor _accessor;

        public ValidateSessionAttribute(UserManager<ApplicationUser> userManager, IRDS LLA, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _LLA = LLA;
            _accessor = httpContextAccessor;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (string.IsNullOrEmpty(context.HttpContext.Session.GetString("Uid") ?? string.Empty))
            {
                byte[] salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                string dt = DateTime.Now.ToString() + Convert.ToBase64String(salt);
                byte[] encodeddt = new UTF8Encoding().GetBytes(dt);
                var sha1 = SHA1.Create();
                var hash = sha1.ComputeHash(encodeddt);
                context.HttpContext.Session.SetString("Uid", Convert.ToBase64String(hash));
            }
        }
    }
}
