using CMS.Models.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Models.BusinessModels
{

    public class FTConfig
    {
        public string DBconnectionString { get; set; }
        public string CacheConnectionString { get; set; }
        public string CacheInstance { get; set; }
        public string StripePublishableKey { get; set; }
        public string StripeSecretKey { get; set; }
    }

    public class Key
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public Int32 DocStatus { get; set; }
    }

}
