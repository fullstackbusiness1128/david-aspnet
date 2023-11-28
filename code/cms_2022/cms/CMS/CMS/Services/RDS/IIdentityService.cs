using System;
using System.Threading.Tasks;
using CMS.Models;
using CMS.Models.BusinessModels;
using CMS.Models.DataModels.LLA;
using CMS.Models.UserViewModels;
using CMS.Models.ViewModels;

namespace CMS.Services
{
    public interface IIdentityService
    {
        Task<ApplicationUser> GetUser(ApplicationUser au);
        UserViewModel GetUsers(ResultOptions options);

        Task<bool> ToggleResellerStatus(Key key);
        Task<DBFiles> DocStatus(Key key);
        
        

        Int32 GetResellersActive();

        Int32 GetResellersTotal();

        Int32 GetCustomersTotal();

        Int32 GetMerchantsTotal();
    }
}