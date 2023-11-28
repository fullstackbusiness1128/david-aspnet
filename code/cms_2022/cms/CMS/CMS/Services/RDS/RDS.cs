using CMS.Data;
using CMS.Models;
using CMS.Models.DataModels.LLA;
using CMS.Models.UserViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using CMS.Models.ViewModels;
using CMS.Models.DashboardViewModels;
using System.IO;
using HtmlAgilityPack;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using CMS.Models.BusinessModels;
using System.Diagnostics;
using CMS.Services.Cache;
using System.Globalization;
using CMS.Models.ActivityViewModels;
using Microsoft.EntityFrameworkCore;

namespace CMS.Services.RDS
{
    public class RDS : IRDS
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RDS> _logger;
        private readonly IIdentityService _identity;
        private readonly IHttpContextAccessor _accessor;
        private readonly ICacheService _Cache;

        public RDS(ApplicationDbContext context, ILogger<RDS> logger, IHttpContextAccessor httpContextAccessor, ICacheService cache, IIdentityService identity, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _accessor = httpContextAccessor;
            _userManager = userManager;
            _identity = identity;
            _Cache = cache;
        }

        /* GET */

        public ActivityViewModel GetTransactions(ResultOptions options)
        {
            ActivityViewModel model = new ActivityViewModel();
            model.Options = new ResultOptions() { Results = new Results(), Filter = new Filter() };
            model.Options.Results.pageNumber = options.Results.pageNumber;
            model.Options.Results.pageSize = options.Results.pageSize;
            model.Options.Results.searchField = options.Results.searchField;
            model.Options.Results.searchValue = options.Results.searchValue;
            model.Options.Results.sortField = options.Results.sortField;
            model.Options.Results.sortOrder = options.Results.sortOrder;
            model.Options.Filter.DateFrom = options.Filter.DateFrom;
            model.Options.Filter.DateTo = options.Filter.DateTo;
            model.Options.Filter.FilterA = options.Filter.FilterA;
            model.Options.Filter.FilterB = options.Filter.FilterB;
            model.Options.Filter.FilterC = options.Filter.FilterC;

            var offset = (options.Results.pageSize * options.Results.pageNumber) - options.Results.pageSize;

            if (model.Options.Filter.FilterA != null) 
            {
                model.Options.Results.totalResults = (from a in _context.Transactions.Where(w => w.UserId == options.Filter.FilterA) select a).Count();
                model.Items = (from a in _context.Transactions.Where(w => w.UserId == options.Filter.FilterA) join b in _context.Users on a.UserId equals b.Id into bX from bY in bX.DefaultIfEmpty() select new Transaction { Activated = a.Activated, Amount = a.Amount, FTPurchasesId = a.FTPurchasesId, ResellerUserId = a.ResellerUserId, ResellerCharged = a.ResellerCharged == null ? 0 : a.ResellerCharged, Type = a.Type, BalanceEnding = a.BalanceEnding, DatetimeCreated = a.DatetimeCreated, FTDebitsId = a.FTDebitsId == null ? 0 : a.FTDebitsId, UserId = a.UserId, UserEmail = bY.Email, UserName = bY.Firstname + " " + bY.Lastname, UserType = bY.Type }).OrderByDescending(o => o.DatetimeCreated).Skip(offset).Take(options.Results.pageSize).ToList();
            }
            else
            {
                model.Options.Results.totalResults = (from a in _context.Transactions select a).Count();
                model.Items = (from a in _context.Transactions join b in _context.Users on a.UserId equals b.Id into bX from bY in bX.DefaultIfEmpty() select new Transaction { Activated = a.Activated, Amount = a.Amount, FTPurchasesId = a.FTPurchasesId, ResellerUserId = a.ResellerUserId, ResellerCharged = a.ResellerCharged == null ? 0 : a.ResellerCharged, Type = a.Type, BalanceEnding = a.BalanceEnding, DatetimeCreated = a.DatetimeCreated, FTDebitsId = a.FTDebitsId == null ? 0 : a.FTDebitsId, UserId = a.UserId, UserEmail = bY.Email, UserName = bY.Firstname + " " + bY.Lastname, UserType = bY.Type }).OrderByDescending(o => o.DatetimeCreated).Skip(offset).Take(options.Results.pageSize).ToList();

            }
              
            return model;
        }

        public decimal GetTotalResellerActiveFunds()
        {
            var result = (from a in _context.Users select a.ResellerBalance).Sum();
            return result;
        }

        public decimal GetTotalResellerBulkPurchased()
        {
            // Left Join as Left Outer Join would also include Customer Credits from a Reseller Purchase
            var result = (from a in _context.Transactions.Where(w => w.Type == "Credit" && w.FTDebitsId == 0) join b in _context.Users.Where(w => w.Type == "R") on a.UserId equals b.Id select a.Amount).Sum();
            return result;
        }

        public decimal GetTotalResellerSales()
        {
            var result = (from a in _context.Transactions.Where(w => w.Type == "Debit" && w.FTPurchasesId == 0) join b in _context.Users.Where(w => w.Type == "R") on a.UserId equals b.Id into bX from bY in bX.DefaultIfEmpty() select a.Amount).Sum();
            return result;
        }

        public decimal GetTotalResellerBuyBacks()
        {
            var result = (from a in _context.Transactions.Where(w => w.Type == "Credit" && w.FTDebitsId > 0) join b in _context.Users.Where(w => w.Type == "R") on a.UserId equals b.Id select a.Amount).Sum();
            return result;
        }

        public int GetTotalCustomerTokens()
        {
            var BoughtFromReseller = (from a in _context.Transactions.Where(w => w.Type == "Credit" && (w.ResellerUserId != null && w.ResellerUserId != "0" && w.ResellerUserId != "")) join b in _context.Users.Where(w => w.Type == "C") on a.UserId equals b.Id select a.Amount).Count();
            var BoughtFromFT = (from a in _context.Transactions.Where(w => w.Type == "Credit" && (string.IsNullOrEmpty(w.ResellerUserId) || w.ResellerUserId == "0")) join b in _context.Users.Where(w => w.Type == "C") on a.UserId equals b.Id select a.Amount).Count();
            return BoughtFromReseller + BoughtFromFT;
        }

        public decimal GetTotalCustomerPurchased()
        {
            var BoughtFromReseller = (from a in _context.Transactions.Where(w => w.Type == "Credit" && (w.ResellerUserId != null && w.ResellerUserId != "0" && w.ResellerUserId != "")) join b in _context.Users.Where(w => w.Type == "C") on a.UserId equals b.Id select a.Amount).Sum();
            var BoughtFromFT = (from a in _context.Transactions.Where(w => w.Type == "Credit" && (string.IsNullOrEmpty(w.ResellerUserId) || w.ResellerUserId == "0")) join b in _context.Users.Where(w => w.Type == "C") on a.UserId equals b.Id select a.Amount).Sum();
            return BoughtFromReseller + BoughtFromFT;
        }

        public decimal GetTotalCustomerPurchasedFT()
        {
            var result = (from a in _context.Transactions.Where(w => w.Type == "Credit" && (string.IsNullOrEmpty(w.ResellerUserId) || w.ResellerUserId == "0")) join b in _context.Users.Where(w => w.Type == "C") on a.UserId equals b.Id select a.Amount).Sum();
            return result;
        }


        public async Task<bool> ToggleDocumentStatus(Int32 Id, Int32 Status)
        {
            // Get Existing Moment to be updated
            var existing = (from a in _context.DBFiles.AsNoTracking().Where(a => a.RecordStatus == true && a.Id == Id) select new MDBFiles { Category = a.Category, DatetimeCreated = a.DatetimeCreated, DatetimeModified = a.DatetimeModified, Extension = a.Extension, Furl = a.Furl, Id = a.Id, Identifier = a.Identifier, Name = a.Name, RecordStatus = a.RecordStatus, Size = a.Size, Status = a.Status, UserId = a.UserId } ).FirstOrDefault();

            if(existing != null)
            { 
                //Archive Existing Record
                var item = new DBFiles() { Id = existing.Id };
                item.RecordStatus = false;
                item.DatetimeModified = Convert.ToInt64(DateTime.Now.ToUniversalTime().AddSeconds(-1).ToString("yyyyMMddHHmmssffff"));
                _context.DBFiles.Attach(item);
                _context.Entry(item).Property(x => x.RecordStatus).IsModified = true;
                _context.Entry(item).Property(x => x.DatetimeModified).IsModified = true;
                _context.SaveChanges();

                // Create New Record
                var time = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff");
                var file = new DBFiles() { Identifier = existing.Identifier, RecordStatus = true, UserId = existing.UserId, DatetimeCreated = Convert.ToInt64(time), Category = existing.Category, DatetimeModified = 0, Extension = existing.Extension, Furl = existing.Furl, Name = existing.Name, Size = existing.Size, Status = Status };
                _context.DBFiles.Add(file);
                _context.SaveChanges();
            }

            return true;
        }


        public ActivityViewModel GetTransactions(ResultOptions options, string userid)
        {
            throw new NotImplementedException();
        }
    }
}