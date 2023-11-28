using Microsoft.AspNetCore.Http;
using FunTokenz.Models.Business;
using FunTokenz.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Amazon.Runtime.Internal.Util;

namespace FunTokenz.Models.View
{

    public class GeneralView : ViewModelBase
    {
        public string Title { get; set; }



    }

    /* FOR GET */
    public class PurchasesView
    {
        public PurchaseListing PurchaseRecords { get; set; }

        public PurchasesView()
        {
            PurchaseRecords = new PurchaseListing();
        }
    }

    public class SettingsView
    {
        public decimal Balance { get; set; }

        public CustomerListing CustomerRecords { get; set; }

        public RateValues Rates { get; set; }

        public RatesForm RatesForm { get; set; }

        public SettingsView()
        {
            CustomerRecords = new CustomerListing();
            RatesForm = new RatesForm();
            Rates = new RateValues();
        }
    }

    public class TransactionsView
    {
        public decimal Balance { get; set; }

        public TransactionListing TransactionRecords { get; set; }

        public TransactionsView()
        {
            TransactionRecords = new TransactionListing();
        }
    }

    public class ResellerTransactionsView
    {
        public decimal Balance { get; set; }

        public ResellerTransactionListing TransactionRecords { get; set; }

        public ResellerTransactionsView()
        {
            TransactionRecords = new ResellerTransactionListing();
        }
    }


    public class PurchaseView
    {
        public MDBPurchase Purchase { get; set; }

        public PurchaseView()
        {
            Purchase = new MDBPurchase();
        }
    }

    public class CheckSpend
    {
        public Int32 TokenId { get; set; }
        public string UserGuid { get; set; }
        public decimal Amount { get; set; }
        public DebitListing DebitRecords { get; set; }

        public CheckSpend()
        {
            DebitRecords = new DebitListing();
        }
    }

    /* FOR POST */
    public class PurchaseForm
    {
        [Required]
        public string Currency { get; set; }

        [Required]
        [RegularExpression(@"^[1-9]\d*(\.\d+)?$", ErrorMessage = "The Amount is not valid - enter in a format like 20.00")]
        [Range(20.00, 1000.00, ErrorMessage = "The Amount must be between 20.00 and 1000.00")]
        public decimal Amount { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Mobile")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "CountryCode")]
        public string CountryCode { get; set; }

        public string SentToEmail { get; set; }
        public string SentToPhone { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal ChargeAmount { get; set; }

        public class ValidateCountryCode : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext context)
            {
                if (value != null)
                {
                    if (value.ToString().StartsWith("+"))
                    {
                        return ValidationResult.Success;
                    }      
                    return new ValidationResult("The Phone Country Code is not valid.");
                }
                else
                {
                    return new ValidationResult("This display name is already taken");
                }
            }
        }

    }

    /* FOR POST */
    public class ResellerPurchaseForm
    {

        [Required]
        [RegularExpression(@"^[1-9]\d*(\.\d+)?$", ErrorMessage = "The Amount is not valid - enter in a format like 20.00")]
        [Range(1000.00, 10000.00, ErrorMessage = "The Amount must be between 1000.00 and 10000.00")]
        public decimal Amount { get; set; }

        public string SentToEmail { get; set; }
        public string SentToPhone { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal ChargeAmount { get; set; }

        public string ResellerCurrency { get; set; }

    }

    public class CustomerSaleView
    {
        public CustomerListing CustomerRecords { get; set; }

        public CustomerSaleForm Form {get; set;}

        public CustomerSaleView()
        {
            CustomerRecords = new CustomerListing();
            Form = new CustomerSaleForm();
        }
    }

    public class CustomerSaleForm
    {

        [Required]
        [RegularExpression(@"^[1-9]\d*(\.\d+)?$", ErrorMessage = "The Amount is not valid - enter in a format like 20.00")]
        [Range(20.00, 1000.00, ErrorMessage = "The Amount must be between 20.00 and 1000.00")]
        public decimal Amount { get; set; }

        public string UserId { get; set; }
        [EmailAddress]
        public string SentToEmail { get; set; }
        [Phone]
        public string SentToPhone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        [Display(Name = "CountryCode")]
        public string CountryCode { get; set; }
    }

    public class CustomerRedeemView
    {
        public CustomerListing CustomerRecords { get; set; }

        public CustomerRedeemForm Form { get; set; }

        public CustomerRedeemView()
        {
            CustomerRecords = new CustomerListing();
            Form = new CustomerRedeemForm();
        }
    }

    public class CustomerRedeemForm
    {
        [Required]
        [RegularExpression(@"^[1-9]\d*(\.\d+)?$", ErrorMessage = "The Amount is not valid - enter in a format like 20.00")]
        [Range(5.00, 1000.00, ErrorMessage = "The Amount must be between 20.00 and 1000.00")]
        public decimal Amount { get; set; }

        public string UserId { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string Phone { get; set; }

        [Required]
        public Int32 FunTokenzCode1 { get; set; }
        [Required]
        public Int32 FunTokenzCode2 { get; set; }
        [Required]
        public Int32 FunTokenzCode3 { get; set; }
        [Required]
        public Int32 FunTokenzCode4 { get; set; }
        [Required]
        public Int32 FunTokenzCode5 { get; set; }
        [Required]
        public Int32 FunTokenzCode6 { get; set; }
        [Required]
        public Int32 FunTokenzCode7 { get; set; }
        [Required]
        public Int32 FunTokenzCode8 { get; set; }

        [Display(Name = "CountryCode")]
        public string CountryCode { get; set; }
    }

    public class RateValues
    {
        public Int32 Id { get; set; }

        public decimal Purchase { get; set; }

        public decimal Withdrawal { get; set; }
    }

    public class RatesForm
    {
        public Int32 Id { get; set; }

        [Required]
        [RegularExpression(@"^\d+\.\d{0,2}$", ErrorMessage = "The Purchase Fee is not valid - enter in a format like 20.00")]
        [Range(0.00, 5.00, ErrorMessage = "The Purchase Fee must be between 0.00 and 5.00")]
        public decimal Purchase { get; set; }

        [Required]
        [RegularExpression(@"^\d+\.\d{0,2}$", ErrorMessage = "The Withdrawal Fee is not valid - enter in a format like 20.00")]
        [Range(0.00, 5.00, ErrorMessage = "The Withdrawal Fee must be between 0.00 and 5.00")]
        public decimal Withdrawal { get; set; }
    }

    public class PayPalOutput
    {
        public string JSON { get; set; }
        public string Ref { get; set; }
    }

    public class FunToken
    {
        public string Code { get; set; }
        public Int64 ExpiryTime { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; }
        public string Phone { get; set; }
    }

    public class Spend
    {
        [Required]
        public string Phone { get; set; }

        public string CountryCode { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Code { get; set; }
    }

    public class FunTokenzResponse
    {
        public string Outcome { get; set; }

        public bool Processed { get; set; }
    }

    public class StripePayment
    {
        public string stripeToken { get; set; }
        public decimal Amount { get; set; }
        [Display(Name = "Card Name")]
        public string CardName { get; set; }
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }
        [Display(Name = "Card Month")]
        public string CardMonth { get; set; }
        [Display(Name = "Card Year")]
        public string CardYear { get; set; }
        [Display(Name = "Card CCV")]
        public string CardCCV { get; set; }
    }

    public class Phone
    {
        public string country_code { get; set; }
        public string phonenumber { get; set; }
    }

    public class SettingsModel
    {
        public ApplicationUser User { get; set; }

        //public UpdateSettings Settings { get; set; }

        public UpdatePassword Password { get; set; }

        public FilesModel AccountValidations { get; set; }

        public List<Country> Countries { get; set; }
        public List<City> Cities { get; set; }

        public SettingsModel()
        {
            User = new ApplicationUser();
            //Settings = new UpdateSettings();
            Password = new UpdatePassword();
            AccountValidations = new FilesModel();
            Countries = new List<Country>();
            Cities = new List<City>();
        }
    }

    public class FilesModel
    {
        public List<PNGFile> files { get; set; }

        public string ValidationType { get; set; }

        public FilesModel()
        {
            files = new List<PNGFile>();
        }
    }

    public class PNGFile
    {
        public string URL { get; set; }
        public Int32 Status { get; set; }
        public string Category { get; set; }
        public string MaskedPan { get; set; }
    }

    public class MediaFileItemForm
    {
        public IList<IFormFile> files { get; set; }

        public string Category { get; set; }
        public string MaskedPan { get; set; }   
    }
}
