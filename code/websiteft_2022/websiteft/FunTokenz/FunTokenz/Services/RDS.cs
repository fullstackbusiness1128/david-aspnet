using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FunTokenz.Data;
using FunTokenz.Models.Business;
using FunTokenz.Models.Data;
using FunTokenz.Models.View;
using System.Threading;


namespace FunTokenz.Services
{
    public class RDS : IRDS
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<RDS> _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IS3 _S3;

        public RDS(ApplicationDbContext context, ILogger<RDS> logger, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IS3 S3)
        {
            _context = context;
            _logger = logger;
            _accessor = httpContextAccessor;
            _userManager = userManager;
            _S3 = S3;
        }

        /* GET */

        public PurchasesView GetPurchases(Options options)
        {
            PurchasesView model = new PurchasesView();
            model.PurchaseRecords.Options = new Options() { Results = new Results(), Filter = new Models.Business.Filter() };
            model.PurchaseRecords.Options.Results.pageNumber = options.Results.pageNumber;
            model.PurchaseRecords.Options.Results.pageSize = options.Results.pageSize;
            model.PurchaseRecords.Options.Results.searchField = options.Results.searchField;
            model.PurchaseRecords.Options.Results.searchValue = options.Results.searchValue;
            model.PurchaseRecords.Options.Results.sortField = options.Results.sortField;
            model.PurchaseRecords.Options.Results.sortOrder = options.Results.sortOrder;
            model.PurchaseRecords.Options.Filter.DateFrom = options.Filter.DateFrom;
            model.PurchaseRecords.Options.Filter.DateTo = options.Filter.DateTo;
            model.PurchaseRecords.Options.Filter.FilterA = options.Filter.FilterA;
            model.PurchaseRecords.Options.Filter.FilterB = options.Filter.FilterB;
            model.PurchaseRecords.Options.Filter.FilterC = options.Filter.FilterC;
            model.PurchaseRecords.Options.Filter.FilterD = options.Filter.FilterD;
            

            var offset = (options.Results.pageSize * options.Results.pageNumber) - options.Results.pageSize;

            model.PurchaseRecords.Options.Results.totalResults = (from a in _context.FTPurchases.Where(a => a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Activated == true) select a).Count();
            model.PurchaseRecords.Purchases = (from a in _context.FTPurchases.Where(a => a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Activated == true) select new MDBPurchase { Code = a.Code, Currency = a.Currency, DatetimeCreated = a.DatetimeCreated, DatetimeExpires = a.DatetimeExpires, PurchaseAmount = a.PurchaseAmount, Id = a.Id, SentToEmail = a.SentToEmail, SentToPhone = a.SentToPhone, UserId = a.UserId, DebitRecords = new DebitListing() { Debits = (from b in _context.FTDebits.Where(w => w.FTPurchasesId == a.Id) select new MDBDebit() { DebitAmount = b.DebitAmount }).OrderByDescending(b => b.DatetimeCreated).ToList() } }).OrderByDescending(o => o.DatetimeCreated).Skip(offset).Take(options.Results.pageSize).ToList();

            return model;
        }

        public ResellerTransactionListing GetResellerTransactions(Options options)
        {
            ResellerTransactionListing model = new ResellerTransactionListing();
           
            model.Options = new Options() { Results = new Results(), Filter = new Models.Business.Filter() };
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
            model.Options.Filter.FilterD = options.Filter.FilterD;

            var offset = (options.Results.pageSize * options.Results.pageNumber) - options.Results.pageSize;

            _logger.LogInformation("Get Reseller Transactions");

            _logger.LogInformation("Get Reseller Transactions | " + _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            model.Options.Results.totalResults = (from a in _context.Transactions.Where(a => (a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Type == "Credit") || (a.ResellerUserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Type == "Debit" && a.FTPurchasesId == 0)) select a).Count();

            _logger.LogInformation("Get Reseller Transactions A | " + model.Options.Results.totalResults);

            model.ResellerTransactionItems = (from a in _context.Transactions.Where(a => (a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Type == "Credit") || (a.ResellerUserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Type == "Debit" && a.FTPurchasesId == 0)) select new ResellerTransactionItem { Amount = a.Amount, FTPurchasesId = a.FTPurchasesId, ResellerUserId = a.ResellerUserId, ResellerCharged = a.ResellerCharged, Type = a.Type, BalanceEnding = a.BalanceEnding, DatetimeCreated = a.DatetimeCreated, FTDebitsId = a.FTDebitsId, UserId = a.UserId }).OrderByDescending(o => o.DatetimeCreated).Skip(offset).Take(options.Results.pageSize).ToList();


            //model.ResellerTransactionItems = (from a in _context.Transactions.Where(a => (a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Type == "Credit") || (a.ResellerUserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Type == "Debit" && a.FTPurchasesId == 0)) select new ResellerTransactionItem { Amount = 1200, FTPurchasesId = 35, ResellerUserId = "8ab92896-9946-4bf9-b1de-889260e8d2ef", ResellerCharged = 44, Type = a.Type, BalanceEnding = 35, DatetimeCreated = 202209022208549330, FTDebitsId = 222, UserId = "8ab92896-9946-4bf9-b1de-889260e8d2ef" }).OrderByDescending(o => o.DatetimeCreated).Skip(offset).Take(options.Results.pageSize).ToList();



            _logger.LogInformation("Get Reseller Transactions B | " + model.ResellerTransactionItems.Count());

            return model;
        }

        public TransactionListing GetResellerPurchases(Options options)
        {
            TransactionListing model = new TransactionListing();
            model.Options = new Options() { Results = new Results(), Filter = new Models.Business.Filter() };
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
            model.Options.Filter.FilterD = options.Filter.FilterD;

            var offset = (options.Results.pageSize * options.Results.pageNumber) - options.Results.pageSize;

            _logger.LogInformation("Get Reseller Purchases");

            _logger.LogInformation("Get Reseller Purchases | " + _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            model.Options.Results.totalResults = (from a in _context.FTPurchases.Where(a => a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.StripeReference != null) select a).Count();

            _logger.LogInformation("Get Reseller Purchases A | " + model.Options.Results.totalResults);

            model.Transactions = (from a in _context.FTPurchases.Where(a => a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.StripeReference != null) select new Transaction { PurchaseAmount = a.PurchaseAmount, Charged = a.Charged, Currency = a.Currency, DatetimeCreated = a.DatetimeCreated, RateUsed = a.RateUsed }).OrderByDescending(o => o.DatetimeCreated).Skip(offset).Take(options.Results.pageSize).ToList();


            _logger.LogInformation("Get Reseller Purchases B | " + model.Transactions.Count());

            return model;
        }

        public TransactionListing GetResellerSales(Options options)
        {
            TransactionListing model = new TransactionListing();
            model.Options = new Options() { Results = new Results(), Filter = new Models.Business.Filter() };
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
            model.Options.Filter.FilterD = options.Filter.FilterD;

            var offset = (options.Results.pageSize * options.Results.pageNumber) - options.Results.pageSize;

            model.Options.Results.totalResults = (from a in _context.FTPurchases.Where(a => a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.StripeReference != null) select a).Count();
            model.Transactions = (from a in _context.FTPurchases.Where(a => a.ResellerUserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.StripeReference != null) select new Transaction { PurchaseAmount = a.PurchaseAmount, Charged = a.Charged, Currency = a.Currency, DatetimeCreated = a.DatetimeCreated, RateUsed = a.RateUsed }).OrderByDescending(o => o.DatetimeCreated).Skip(offset).Take(options.Results.pageSize).ToList();

            return model;
        }

        public async Task<RateValues> GetRates()
        {
            RateValues model = new RateValues();
            var rates = (from a in _context.FTReseller.Where(w => w.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && w.RecordStatus == true) select new RateValues { Id = a.Id, Purchase = a.PurchasePercentFee, Withdrawal = a.BuybackPercentFee }).FirstOrDefault();
            if(rates !=null && rates.Purchase >= 0 && rates.Withdrawal >= 0)
            {
                model.Id = rates.Id;
                model.Withdrawal = rates.Withdrawal;
                model.Purchase = rates.Purchase;
            }
            return model;
        }

        public PurchaseView GetPurchase(Int32 Id)
        {
            PurchaseView model = new PurchaseView();
            model.Purchase = (from a in _context.FTPurchases.Where(a => a.Id == Id && a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && a.Activated == true) select new MDBPurchase { Code = a.Code, Currency = a.Currency, DatetimeCreated = a.DatetimeCreated, DatetimeExpires = a.DatetimeExpires, PurchaseAmount = a.PurchaseAmount, Id = a.Id, SentToEmail = a.SentToEmail, SentToPhone = a.SentToPhone, UserId = a.UserId, DebitRecords = new DebitListing() { Debits = (from b in _context.FTDebits.Where(w => w.FTPurchasesId == a.Id) select new MDBDebit() { DebitAmount = b.DebitAmount }).OrderByDescending(b => b.DatetimeCreated).ToList() } }).FirstOrDefault();
            return model;
        }

        public String GetExchangeRate(string Code, string Against)
        {
            var result = (from a in _context.ExchangeRate.Where(a => a.Code == Code && a.Against == Against).OrderByDescending(o => o.DatetimeCreated).Take(1) select a.Value).FirstOrDefault();
            return result;
        }

        public CheckSpend CheckSpend(string PhoneNumber, string CountryCode, string Code)
        {
            // Check Time
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");
            var timecheck = Convert.ToInt64(basetime.AddYears(-2).ToString("yyyyMMddHHmmssffff"));

            CheckSpend model = new CheckSpend();
            // Phone Code is not known in PNGBet
            //model = (from a in _context.FTPurchases.Where(a => a.Code == Code && a.Activated == true && a.DatetimeExpires > timecheck) join b in _context.Users.Where(w=>w.CountryCode == CountryCode && w.PhoneNumber == PhoneNumber) on a.UserId equals b.Id select new CheckSpend { TokenId = a.Id, Amount = a.PurchaseAmount, UserGuid = b.Id, DebitRecords = new DebitListing() { Debits = (from c in _context.FTDebits.Where(w => w.FTPurchasesId == a.Id) select new MDBDebit() { DebitAmount = c.DebitAmount }).OrderByDescending(c => c.DatetimeCreated).ToList() } }).FirstOrDefault();

            model = (from a in _context.FTPurchases.Where(a => a.Code == Code && a.Activated == true && a.DatetimeExpires > Convert.ToInt64(time) && (a.SentToPhone.Replace(" ", "").Contains(PhoneNumber.Replace(" ", "")) || PhoneNumber.Replace(" ", "").Contains(a.SentToPhone.Replace(" ", "")))) join b in _context.Users on a.UserId equals b.Id select new CheckSpend { TokenId = a.Id, Amount = a.PurchaseAmount, UserGuid = b.Id, DebitRecords = new DebitListing() { Debits = (from c in _context.FTDebits.Where(w => w.FTPurchasesId == a.Id) select new MDBDebit() { DebitAmount = c.DebitAmount }).OrderByDescending(c => c.DatetimeCreated).ToList() } }).FirstOrDefault();
            return model;
        }

        /* SET */

        public bool MapCustomer(string UserId)
        {
            var existing = (from a in _context.ResellerCustomers.Where(w => w.ResellerUserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && w.UserId == UserId) select a.Id).FirstOrDefault();
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");

            if (existing > 0)
            {
                var item = new ResellerCustomers() { Id = existing };
                item.DatetimeLastServiced = Convert.ToInt64(time);
                _context.ResellerCustomers.Attach(item);
                _context.Entry(item).Property(x => x.DatetimeLastServiced).IsModified = true;
                _context.SaveChanges();
            }
            else
            {
                var entry = new ResellerCustomers()
                {
                    DatetimeCreated = Convert.ToInt64(time),
                    UserId = UserId,
                    ResellerUserId = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    DatetimeLastServiced = Convert.ToInt64(time)
                };
                _context.ResellerCustomers.Add(entry);
                _context.SaveChanges();
            }

            return true;
        }

        public decimal SpendToken(Int32 TokenId, Decimal Amount, Decimal EndingBalance)
        {
            // Create Token Purchase
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");
            var expires = basetime.AddYears(2).ToString("yyyyMMddHHmmssffff");

            var entry = new FTDebits()
            {
                DatetimeCreated = Convert.ToInt64(time),
                DebitAmount = Amount,
                FTPurchasesId = TokenId,
                BalanceEnding = EndingBalance,
                ResellerCharged = 0
            };

            _context.FTDebits.Add(entry);
            _context.SaveChanges();
            return entry.Id;
        }

        public decimal SpendResellerFunds(Decimal Amount, Decimal EndingBalance, Decimal ResellerCharged)
        {
            // Create Token Purchase
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");
            
            var entry = new FTDebits()
            {
                DatetimeCreated = Convert.ToInt64(time),
                DebitAmount = Amount,
                FTPurchasesId = 0,
                ResellerUserId = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                BalanceEnding = EndingBalance,
                ResellerCharged = ResellerCharged
            };

            _context.FTDebits.Add(entry);
            _context.SaveChanges();
            return entry.Id;
        }

        public int SpendCustomerToken(Int32 TokenId, Decimal Amount, Decimal EndingBalance)
        {
            // Withdraw From Customer
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");
            var expires = basetime.AddYears(2).ToString("yyyyMMddHHmmssffff");

            var entry = new FTDebits()
            {
                DatetimeCreated = Convert.ToInt64(time),
                DebitAmount = Amount,
                FTPurchasesId = TokenId,
                BalanceEnding = EndingBalance,
                ResellerUserId = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                ResellerCharged = 0
            };

            _context.FTDebits.Add(entry);
            _context.SaveChanges();
            return entry.Id;
        }

        public Int32 SetPurchase(PurchaseForm post, bool reseller, string resellerUserId, string chargeCurrency)
        {
            // Create Token Purchase
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");
            var expires = basetime.AddYears(2).ToString("yyyyMMddHHmmssffff");

            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            var code = String.Format("{0:D8}", random);

            var activated = false;
            string rid = null;
            var ruid = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(reseller == true && !string.IsNullOrEmpty(resellerUserId))
            {
                rid = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                ruid = resellerUserId;
                activated = true;
            }

            var entry = new FTPurchases()
            {
                DatetimeCreated = Convert.ToInt64(time),
                UserId = ruid,
                PurchaseAmount = post.Amount,
                Charged = post.ChargeAmount,
                RateUsed = post.ExchangeRate,
                Code = code,
                SentToPhone = post.CountryCode + " " + post.Phone,
                SentToEmail = null,
                Currency = post.Currency,
                DatetimeExpires = Convert.ToInt64(expires),
                Activated = activated,
                PayPalReference = null,
                PayPalResult = null,
                ResellerUserId = rid,
                FTDebitsId = 0,
                ChargeCurrency = chargeCurrency
            };

            _context.FTPurchases.Add(entry);
            _context.SaveChanges();
            return entry.Id;
        }

        public Int32 SetResellerPurchase(ResellerPurchaseForm post)
        {

            // Create Token Purchase
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");
            var expires = basetime.AddYears(2).ToString("yyyyMMddHHmmssffff");

            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            var code = String.Format("{0:D8}", random);

            var entry = new FTPurchases()
            {
                DatetimeCreated = Convert.ToInt64(time),
                UserId = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                PurchaseAmount = post.Amount,
                Code = code,
                SentToPhone = null,
                SentToEmail = null,
                Currency = post.ResellerCurrency,
                ChargeCurrency = post.ResellerCurrency,
                Charged = post.ChargeAmount,
                RateUsed = post.ExchangeRate,
                DatetimeExpires = 0,
                Activated = false,
                PayPalReference = null,
                PayPalResult = null,
                FTDebitsId = 0,
                ResellerUserId = null
            };

            _context.FTPurchases.Add(entry);
            _context.SaveChanges();
            return entry.Id;
        }

        public Int32 SetResellerBuyBack(ResellerPurchaseForm post, int FTDebitId)
        {

            // Create Token Purchase
            var basetime = DateTime.Now.ToUniversalTime();
            var time = basetime.ToString("yyyyMMddHHmmssffff");
            var expires = basetime.AddYears(2).ToString("yyyyMMddHHmmssffff");

            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            var code = String.Format("{0:D8}", random);

            var entry = new FTPurchases()
            {
                DatetimeCreated = Convert.ToInt64(time),
                UserId = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                PurchaseAmount = post.Amount,
                Code = code,
                SentToPhone = null,
                SentToEmail = null,
                Currency = post.ResellerCurrency,
                ChargeCurrency = post.ResellerCurrency,
                Charged = post.ChargeAmount,
                RateUsed = post.ExchangeRate,
                DatetimeExpires = 0,
                Activated = false,
                PayPalReference = null,
                PayPalResult = null,
                FTDebitsId = FTDebitId
            };

            _context.FTPurchases.Add(entry);
            _context.SaveChanges();
            return entry.Id;
        }

        public Int32 UpdatePurchase(string JSON, string Ref, string stripeRef, decimal Amount)
        {
            try
            {

                var basetime = DateTime.Now.ToUniversalTime();
                var time = Convert.ToInt64(basetime.AddHours(-6).ToString("yyyyMMddHHmmssffff"));
                var existing = (from a in _context.FTPurchases.AsNoTracking().Where(w => w.PurchaseAmount == Amount && w.Activated == false && w.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && w.DatetimeCreated >= time && w.PayPalReference == null && w.PayPalResult == null && w.StripeReference == null) select a.Id).OrderByDescending(o=>o).FirstOrDefault();

                if (existing > 0)
                {
                    var item = new FTPurchases() { Id = existing };
                    item.PayPalResult = JSON;
                    item.PayPalReference = Ref;
                    item.StripeReference = stripeRef;
                    item.Activated = true;
                    _context.FTPurchases.Attach(item);
                    _context.Entry(item).Property(x => x.Activated).IsModified = true;
                    _context.Entry(item).Property(x => x.PayPalResult).IsModified = true;
                    _context.Entry(item).Property(x => x.PayPalReference).IsModified = true;
                    _context.Entry(item).Property(x => x.StripeReference).IsModified = true;
                    _context.SaveChanges();

                    return item.Id;
                }
            }
            catch(Exception exc)
            {
                _logger.LogInformation("Update Purchase Error: "+ exc);
            }
            return 0;
        }

      
        public Int32 UpdateResellerPurchase(string JSON, string Ref, string stripeRef, decimal Amount)
        {
            try
            {

                var basetime = DateTime.Now.ToUniversalTime();
                var time = Convert.ToInt64(basetime.AddHours(-6).ToString("yyyyMMddHHmmssffff"));
                var existing = (from a in _context.FTPurchases.AsNoTracking().Where(w => w.PurchaseAmount == Amount && w.Activated == false && w.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && w.DatetimeCreated >= time && w.PayPalReference == null && w.PayPalResult == null && w.StripeReference == null) select a.Id).OrderByDescending(o => o).FirstOrDefault();

                if (existing > 0)
                {
                    var item = new FTPurchases() { Id = existing };
                    item.PayPalResult = JSON;
                    item.PayPalReference = Ref;
                    item.StripeReference = stripeRef;
                    _context.FTPurchases.Attach(item);
                    _context.Entry(item).Property(x => x.PayPalResult).IsModified = true;
                    _context.Entry(item).Property(x => x.PayPalReference).IsModified = true;
                    _context.Entry(item).Property(x => x.StripeReference).IsModified = true;
                    _context.SaveChanges();

                    return item.Id;
                }
            }
            catch (Exception exc)
            {
                _logger.LogInformation("Update Purchase Error: " + exc);
            }
            return 0;
        }

        public FunToken GetToken(Int32 Id)
        {
            FunToken model = new FunToken();
            try
            {
                var basetime = DateTime.Now.ToUniversalTime();
                var time = Convert.ToInt64(basetime.AddHours(-6).ToString("yyyyMMddHHmmssffff"));
                model = (from a in _context.FTPurchases.AsNoTracking().Where(w => w.Activated == true && w.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && w.DatetimeCreated >= time && w.PayPalReference != null && w.PayPalResult != null && w.Id == Id) select new FunToken() {Code = a.Code, Currency = a.Currency, ExpiryTime = a.DatetimeExpires, Value = a.PurchaseAmount, Phone = a.SentToPhone }).OrderByDescending(o => o).FirstOrDefault();
            }
            catch (Exception exc)
            {
                _logger.LogInformation("Get Token Error: " + exc);
            }
            return model;
        }

        public async Task<bool> ArchiveRate(Int32 Id)
        {
            //Archive Record
            var item = new FTReseller() { Id = Id };
            item.RecordStatus = false;
            item.DatetimeModified = Convert.ToInt64(DateTime.Now.ToUniversalTime().AddSeconds(-1).ToString("yyyyMMddHHmmssffff"));
            _context.FTReseller.Attach(item);
            _context.Entry(item).Property(x => x.RecordStatus).IsModified = true;
            _context.Entry(item).Property(x => x.DatetimeModified).IsModified = true;
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> UpdateRates(RatesForm rate)
        {
            // Get Existing
            var existing = await GetRates();

            _logger.LogInformation("Update rates: " + existing.Id);

            //Archive Prior Record
            if (existing.Id > 0)
            {
                _logger.LogInformation("Archiving Rate: " + existing.Id);
                var archive = await ArchiveRate(existing.Id);
            }

            var time = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff");

            _context.FTReseller.Add(new FTReseller
            {
                DatetimeCreated = Convert.ToInt64(time),
                DatetimeModified = 0,
                RecordStatus = true,
                UserId = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                BuybackPercentFee = rate.Withdrawal,
                PurchasePercentFee = rate.Purchase

            });
            _context.SaveChanges();

            return true;
        }

        public CustomerListing GetResellerCustomers(Options options)
        {
            CustomerListing model = new CustomerListing();
            model.Options = new Options() { Results = new Results(), Filter = new Models.Business.Filter() };
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
            model.Options.Filter.FilterD = options.Filter.FilterD;

            var offset = (options.Results.pageSize * options.Results.pageNumber) - options.Results.pageSize;

            model.Options.Results.totalResults = (from a in _context.ResellerCustomers.Where(a => a.ResellerUserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value) join b in _userManager.Users on a.UserId equals b.Id select new Customer { UserId = b.Id }).Count();
            model.Customers = (from a in _context.ResellerCustomers.Where(a => a.ResellerUserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value) join b in _userManager.Users on a.UserId equals b.Id select new Customer { UserId = b.Id, CountryCode = b.CountryCode, DatetimeCreated = b.DatetimeJoined, Email = b.Email, Firstname = b.Firstname, Lastname = b.Lastname, Mobile = b.Mobile  }).OrderBy(o => o.Lastname).ThenBy(o=>o.Firstname).Skip(offset).Take(options.Results.pageSize).ToList();

            return model;
        }



        public async Task<string> SetMediaFile(IFormFile file, string type, string catgeory)
        {
            try
            {

                string furl = "";
                string newname = "";
                Regex rgx = new Regex("[^a-zA-Z0-9-]");
                var time = DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmssffff");

                var ext = Path.GetExtension(file.FileName);
                var path = Path.GetFileNameWithoutExtension(file.FileName);
                if (path.Length > 64)
                {
                    path = path.Substring(0, 64);
                }

                _logger.LogInformation("Validatation Upload 4: " + ext.ToLower());

                if (ext.ToLower() == ".pdf" || ext.ToLower() == ".xls" || ext.ToLower() == ".doc" || ext.ToLower() == ".xlsx" || ext.ToLower() == ".docx" || ext.ToLower() == ".jpg" || ext.ToLower() == ".jpeg" || ext.ToLower() == ".png" || ext.ToLower() == ".gif" || ext.ToLower() == ".bmp" || ext.ToLower() == ".tiff")
                {
                    newname = Regex.Replace(rgx.Replace(path, "").ToLower() + "-" + time + "-" + _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, "[ -]{2,}", " ").ToLower().Replace(" ", "-");
                    furl = Uri.EscapeUriString(newname + ext);

                    //Add File
                    if (file.Length > 1)
                    {
                        _logger.LogInformation("Validatation Upload 5: " + furl);

                        var exists = (from a in _context.DBFiles.Where(w => w.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value && w.Category == catgeory) select a.Id).FirstOrDefault();
                        _logger.LogInformation("Validatation Upload 6: " + exists);
                        if (exists > 0)
                        {
                            var item = new DBFiles() { Id = exists };
                            item.RecordStatus = false;
                            item.DatetimeModified = Convert.ToInt64(time);
                            _context.DBFiles.Attach(item);
                            _context.Entry(item).Property(x => x.RecordStatus).IsModified = true;
                            _context.Entry(item).Property(x => x.DatetimeModified).IsModified = true;
                            _context.SaveChanges();
                        }

                        _context.DBFiles.Add(new DBFiles
                        {
                            Name = newname,
                            Identifier = furl,
                            Furl = furl,
                            Extension = ext,
                            Size = null,
                            Type = type,
                            Category = catgeory,
                            DateCreated = Convert.ToInt64(time),
                            DatetimeModified = Convert.ToInt64(time),
                            Status = 0,
                            RecordStatus = true,
                            UserId = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value
                        });
                        _context.SaveChanges();

                        //Convert to Base64
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string s = Convert.ToBase64String(fileBytes);

                            
                            //Send to S3
                            var process = await _S3.sendToS3(s, newname, ext);


                        }
                    }
                }

                return newname + ext;
            }
            catch (Exception exc)
            {
                _logger.LogError("Set File: " + exc);
            }
            return null;
        }

        public FilesModel GetFiles()
        {
            FilesModel model = new FilesModel();
            model.files = (from a in _context.DBFiles.Where(a => a.RecordStatus == true && a.UserId == _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value) select new PNGFile() { Status = a.Status, Category = a.Category, URL = a.Furl }).ToList();

            return model;
        }

    }
}
