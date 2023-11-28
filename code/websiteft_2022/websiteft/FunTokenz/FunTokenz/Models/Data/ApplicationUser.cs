using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FunTokenz.Models.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string CountryCode { get; set; }
        public string Mobile { get; set; }
        public string Level { get; set; }
        public string Type { get; set; } // M = Merchant, R = Reseller, C = Customer

        public Int64 DatetimeJoined { get; set; }
        public Int64 DatetimeAccountConfirmed { get; set; }

        public string StripeCustomerRef { get; set; }

        public string CompanyName { get; set; }
        public string CompanyNumber { get; set; }

        public decimal ResellerBalance { get; set; }

        public string ResellerCurrency { get; set; }

        public bool ResellerApproved { get; set; }

        public decimal ResellerProfit { get; set; }
    }
}
