using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FunTokenz.Filter
{
    public class ValidateSessionAttribute : ActionFilterAttribute
    {
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
