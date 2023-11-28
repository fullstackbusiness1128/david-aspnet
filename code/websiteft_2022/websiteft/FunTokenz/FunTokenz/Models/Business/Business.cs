using Newtonsoft.Json;
using FunTokenz.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FunTokenz.Models.Business
{

    public class ContentSection
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }

    public class ViewModelBase
    {

        public ContentSection ContentSection { get; set; }

        public ViewModelBase()
        {
            ContentSection = new ContentSection();
        }
    }


    public class Options
    {
        public Results Results { get; set; }
        public Filter Filter { get; set; }

        public Options()
        {
            Results = new Results();
            Filter = new Filter();
        }
    }

    public class Results
    {
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public string sortOrder { get; set; }
        public string sortField { get; set; }
        public string searchValue { get; set; }
        public string searchField { get; set; }
        public double totalResults { get; set; }
    }

    public class Filter
    {
        public string GroupBy { get; set; }
        public Int64 DateFrom { get; set; }
        public Int64 DateTo { get; set; }
        public string FilterA { get; set; }
        public string FilterB { get; set; }
        public string FilterC { get; set; }
        public string FilterD { get; set; }
    }

    public class KeyPair
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    /* * */

    public class TransactionListing
    {
        public Options Options { get; set; }

        public List<Transaction> Transactions { get; set; }

        public TransactionListing()
        {
            Transactions = new List<Transaction>();
        }
    }

    public class ResellerTransactionListing
    {
        public Options Options { get; set; }

        public List<ResellerTransactionItem> ResellerTransactionItems { get; set; }

        public ResellerTransactionListing()
        {
            ResellerTransactionItems = new List<ResellerTransactionItem>();
        }
    }


    public class ResellerTransactionItem
    {
        public string Type { get; set; }
        public string UserId { get; set; }
        public Int32 FTPurchasesId { get; set; }
        public Int32 FTDebitsId { get; set; }
        public Int64 DatetimeCreated { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceEnding { get; set; }
        public string ResellerUserId { get; set; }
        public decimal ResellerCharged { get; set; }
    }

    public class CustomerListing
    {
        public Options Options { get; set; }

        public List<Customer> Customers { get; set; }

        public CustomerListing()
        {
            Customers = new List<Customer>();
        }
    }

    public class Transaction
    {
        public Int64 DatetimeCreated { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal Charged { get; set; }
        public decimal RateUsed { get; set; }
        public string Currency { get; set; }
    }

    public class Customer
    {
        public Int64 DatetimeCreated { get; set; }
        public string UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string CountryCode { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }

    public class PurchaseListing
    {
        public Options Options { get; set; }

        public List<MDBPurchase> Purchases { get; set; }

        public PurchaseListing()
        {
            Purchases = new List<MDBPurchase>();
        }
    }

    public class DebitListing
    {
        public Options Options { get; set; }

        public List<MDBDebit> Debits { get; set; }

        public DebitListing()
        {
            Debits = new List<MDBDebit>();
        }
    }

    public class MDBPurchase
    {
        public Int32 Id { get; set; }

        public string UserId { get; set; }
        public Int64 DatetimeCreated { get; set; }
        public Int64 DatetimeExpires { get; set; }

        public string Code { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string Currency { get; set; }

        public string SentToPhone { get; set; }
        public string SentToEmail { get; set; }

        public DebitListing DebitRecords { get; set; }

        public MDBPurchase()
        {
            DebitRecords = new DebitListing();
        }
    }

    public class MDBDebit
    {
        public Int32 Id { get; set; }
        public Int32 FTPurchasesId { get; set; }
        public Int64 DatetimeCreated { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal BalanceEnding { get; set; }
        public string ClientIdentifier { get; set; }
        public string ClientUserIdentifier { get; set; }
    }

    /* * */

    public class EmailPurchase
    {
        public string EmailAddressTo { get; set; }
        public string NameTo { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Code { get; set; }
        public Int64 ExpiryDate { get; set; }
    }

    public class EmailForgot
    {
        public string EmailAddressTo { get; set; }
        public string NameTo { get; set; }
        public string Link { get; set; }
    }

    /* * */

    public class UpdatePassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

    /* * */

    public class FTConfig
    {
        public string DBconnectionString { get; set; }
        public string CacheConnectionString { get; set; }
        public string CacheInstance { get; set; }
        public string CardPayTerminalCode { get; set; }
        public string CardPayPassword { get; set; }
        public string CardPayURL { get; set; }
        public string FunTokenzURL { get; set; }
        public string S3Bucket { get; set; }
        public string SQSComms { get; set; }
        public string TippingCompsURL { get; set; }
        public string BetConstructCasinoURLAuth { get; set; }
        public string CrazyBillionsCDN { get; set; }
        public string CrazyBillionsLaunch { get; set; }
        public string KironPlayURL { get; set; }
        public string UniversalRaceURL { get; set; }
    }

    public class Country
    {
        public Int64 Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class City
    {
        public Int64 Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string CountryIdentifier { get; set; }
    }

}
