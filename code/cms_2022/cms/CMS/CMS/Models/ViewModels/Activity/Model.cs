using CMS.Models.BusinessModels;
using CMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Models.ActivityViewModels
{
    public class ActivityViewModel
    {
        public ResultOptions Options { get; set; }

        public List<Transaction> Items { get; set; }

        public ActivityViewModel()
        {
            Options = new ResultOptions();
        }
    }

    public class Transaction
    {
        public string Type { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public Int32? FTPurchasesId { get; set; }
        public Int32? FTDebitsId { get; set; }
        public Int64 DatetimeCreated { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceEnding { get; set; }
        public string ResellerUserId { get; set; }
        public decimal? ResellerCharged { get; set; }
        public bool Activated { get; set; }
    }

}
