using CMS.Models.ActivityViewModels;
using CMS.Models.BusinessModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Models.DashboardViewModels
{
    public class DashboardModelView
    {
        public ActivityViewModel Activities { get; set; }

        public Int32 TotalResellersActive { get; set; }
        public Int32 TotalResellers { get; set; }
        public Int32 TotalMerchants { get; set; }
        public Int32 TotalCustomers { get; set; }

        public Decimal TotalResellerActiveFunds { get; set; }
        public Decimal TotalResellerBulkPurchased { get; set; }
        public Decimal TotalResellerSales { get; set; }
        public Decimal TotalResellerBuyBacks { get; set; }

        public Decimal TotalCustomerTokens { get; set; }
        public Decimal TotalCustomerPurchased { get; set; }
        public Decimal TotalCustomerPurchasedFT { get; set; }

        public String Userid { get; set; }


        public DashboardModelView()
        {
            Activities = new ActivityViewModel(); 
        }
    }

}
