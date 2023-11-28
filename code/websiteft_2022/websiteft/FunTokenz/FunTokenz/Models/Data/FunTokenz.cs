using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace FunTokenz.Models.Data
{
    


    public class Transactions
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

    public class FTPurchases
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }
        
        public string UserId{ get; set; }
        public Int64 DatetimeCreated { get; set; }
        public Int64 DatetimeExpires { get; set; }

        public string Code { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string Currency { get; set; }
        public string ChargeCurrency { get; set; }
        public decimal Charged { get; set; }
        public decimal RateUsed { get; set; }

        public bool Activated { get; set; }
        public string PayPalReference { get; set; }
        public string StripeReference { get; set; }
        public string PayPalResult { get; set; }

        public string SentToPhone { get; set; }
        public string SentToEmail { get; set; }

        public string ResellerUserId { get; set; }

        public Int32 FTDebitsId { get; set; }
    }

    public class FTDebits
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }

        public Int32 FTPurchasesId { get; set; }

        public Int64 DatetimeCreated { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal BalanceEnding { get; set; }

        public string ClientIdentifier { get; set; }
        public string ClientUserIdentifier { get; set; }

        public string ResellerUserId { get; set; }
        public decimal ResellerCharged { get; set; }

    }

    public class ResellerCustomers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }
        public string UserId { get; set; }
        public string ResellerUserId { get; set; }
        public Int64 DatetimeCreated { get; set; }
        public Int64 DatetimeLastServiced { get; set; }
    }

    public class FTReseller
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }

        public string UserId { get; set; }
        public Int64 DatetimeCreated { get; set; }
        public Int64 DatetimeModified { get; set; }
        public bool RecordStatus { get; set; }

        public decimal PurchasePercentFee { get; set; }
        public decimal BuybackPercentFee { get; set; }
    }

    public class ExchangeRate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string Against { get; set; }
        public Int64 DatetimeCreated { get; set; }
    }


    public class DBFiles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public Int64 DateCreated { get; set; }
        public Int64 DatetimeModified { get; set; }
        public Int64 DatetimeCancelled { get; set; }
        public bool RecordStatus { get; set; }

        public string Furl { get; set; }
        public string Extension { get; set; }
        public Int32 Status { get; set; } // 0 = Pending, 1 = Declined, 2 = Approved
        public string Size { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string UserId { get; set; }

    }


}
