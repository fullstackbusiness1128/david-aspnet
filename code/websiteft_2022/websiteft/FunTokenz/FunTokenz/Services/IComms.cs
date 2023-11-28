using FunTokenz.Models.Business;
using System.Threading.Tasks;

namespace FunTokenz.Services
{
    public interface IComms
    {
        Task<bool> SendForgotPasswordEmail(EmailForgot model);
        Task<bool> SendPurchaseEmail(EmailPurchase model);
    }
}