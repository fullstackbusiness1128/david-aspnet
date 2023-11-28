using FunTokenz.Models.Business;
using FunTokenz.Models.View;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace FunTokenz.Services
{
    public interface IRDS
    {
        PurchaseView GetPurchase(int Id);
        PurchasesView GetPurchases(Options options);
        ResellerTransactionListing GetResellerTransactions(Options options);
        TransactionListing GetResellerPurchases(Options options);
        String GetExchangeRate(string Code, string Against);
        int SetPurchase(PurchaseForm post, bool reseller, string resellerUserId, string chargeCurrency);
        Int32 SetResellerPurchase(ResellerPurchaseForm post);
        Int32 UpdatePurchase(string JSON, string Ref, string stripeRef, decimal Amount);

        Int32 UpdateResellerPurchase(string JSON, string Ref, string stripeRef, decimal Amount);

        FunToken GetToken(Int32 Id);
        CheckSpend CheckSpend(string PhoneNumber, string CountryCode, string Code);
        decimal SpendToken(Int32 TokenId, Decimal Amount, Decimal EndingBalance);
        Task<RateValues> GetRates();
        Task<bool> UpdateRates(RatesForm rate);
        Task<bool> ArchiveRate(Int32 Id);

        CustomerListing GetResellerCustomers(Options options);

        decimal SpendResellerFunds(Decimal Amount, Decimal EndingBalance, Decimal ResellerCharged);
        Int32 SpendCustomerToken(Int32 TokenId, Decimal Amount, Decimal EndingBalance);
        Int32 SetResellerBuyBack(ResellerPurchaseForm post, int FTDebitId);

        bool MapCustomer(string UserId);

        Task<string> SetMediaFile(IFormFile file, string type, string category);

        FilesModel GetFiles();
    }
}