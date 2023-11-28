using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMS.Models.BusinessModels;
using CMS.Models.DashboardViewModels;
using CMS.Models.ViewModels;
using CMS.Models.ActivityViewModels;
using Microsoft.AspNetCore.Http;
using CMS.Models.UserViewModels;

namespace CMS.Services.RDS
{
    public interface IRDS
    {
        ActivityViewModel GetTransactions(ResultOptions options);

        ActivityViewModel GetTransactions(ResultOptions options, string userid);

        decimal GetTotalResellerActiveFunds();
        decimal GetTotalResellerBulkPurchased();
        decimal GetTotalResellerSales();
        decimal GetTotalResellerBuyBacks();

        int GetTotalCustomerTokens();
        decimal GetTotalCustomerPurchased();
        decimal GetTotalCustomerPurchasedFT();

        Task<bool> ToggleDocumentStatus(Int32 Id, Int32 Status);

    }
}