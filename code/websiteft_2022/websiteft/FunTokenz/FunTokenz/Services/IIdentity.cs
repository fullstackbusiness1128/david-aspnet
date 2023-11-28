using FunTokenz.Models.Business;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunTokenz.Services
{
    public interface IIdentity
    {
        Task<bool> EmailExist(string Email);
        Task<bool> UpdatePassword(UpdatePassword password);

        Task<bool> SetStripeCustomerRef(string reference);

        Task<decimal> UpdateResellerBalance(decimal Amount);

        Task<decimal> ChargeResellerBalance(decimal Amount);

        Task<decimal> UpdateResellerProfit(decimal profit);

        bool IsUniquePhonenumber(string countryCode, string phonenumber);

        
    }
}