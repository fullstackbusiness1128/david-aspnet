using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CMS.Models;
using CMS.Models.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Models.ViewModels;
using System.Linq.Expressions;
using CMS.Models.BusinessModels;
using CMS.Models.DataModels.LLA;
using System.Linq.Dynamic.Core;
using CMS.Data;
using System.Runtime.InteropServices;

namespace CMS.Services
{
    public class IdentityService : IIdentityService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public IdentityService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<ApplicationUser> GetUser(ApplicationUser au)
        {
            var auser = await _userManager.FindByIdAsync(au.Id);
            return auser;
        }

        public UserViewModel GetUsers(ResultOptions options)
        {
            UserViewModel users = new UserViewModel();
            users.Results = options;

            IQueryable<ApplicationUser> results;

            Expression<Func<ApplicationUser, bool>> whereClause = (a => a.Id != null);
            whereClause = (a => a.Type == options.Filter.FilterA );

            if (options.Results.searchValue != null)
            {
                results = _userManager.Users.Where(whereClause).Where(w => w.Email.Contains(options.Results.searchValue) || w.Firstname.Contains(options.Results.searchValue) || w.Lastname.Contains(options.Results.searchValue));
                users.Results.Results.totalResults = _userManager.Users.Where(whereClause).Where(w => w.Email.Contains(options.Results.searchValue) || w.Firstname.Contains(options.Results.searchValue) || w.Lastname.Contains(options.Results.searchValue)).Count();
            }
            else
            {
                results = _userManager.Users.Where(whereClause);
                users.Results.Results.totalResults = _userManager.Users.Where(whereClause).Count();
            }

            switch (options.Results.sortField + options.Results.sortOrder)
            {
                case "Firstnameasc": results = results.OrderBy(c => c.Firstname); break;
                case "Firstnamedesc": results = results.OrderByDescending(c => c.Firstname); break;
                case "Lastnameasc": results = results.OrderBy(c => c.Lastname); break;
                case "Lastnamedesc": results = results.OrderByDescending(c => c.Lastname); break;
                case "Emailasc": results = results.OrderBy(c => c.Email); break;
                case "Emaildesc": results = results.OrderByDescending(c => c.Email); break;
                case "Levelasc": results = results.OrderBy(c => c.Level); break;
                case "Leveldesc": results = results.OrderByDescending(c => c.Level); break;
                default: results = results.OrderBy(c => c.Lastname); break;
            }

            results = results.Skip((options.Results.pageNumber - 1) * options.Results.pageSize).Take(options.Results.pageSize);
            //users.User = results.Select(u => new UserItem
            //{
            //    Firstname = u.Firstname,
            //    Lastname = u.Lastname,
            //    Email = u.Email,
            //    Id = u.Id,
            //    Type = u.Type,
            //    ResellerApproved = u.ResellerApproved,
            //    ResellerBalance = u.ResellerBalance
            //})
            //.ToList();

            users.User = (from a in results
                          join b in _context.DBFiles.Where(w=>w.RecordStatus == true) on a.Id equals b.UserId into bY
                          from bX in bY.OrderByDescending(o => o.DatetimeCreated).Take(1).DefaultIfEmpty()
                          select new UserItem {
                              Firstname = a.Firstname,
                              Lastname = a.Lastname,
                              Email = a.Email,
                              Id = a.Id,
                              Type = a.Type,
                              ResellerApproved = a.ResellerApproved,
                              ResellerBalance = a.ResellerBalance,
                              DocumentFurl = bX.Furl == null ? "" : bX.Furl,
                              DocumentId = bX.Id == null ? 0 : bX.Id,
                              DocumentStatus = bX.Status == null ? 0 : bX.Status,
                          }).ToList();


            return users;

        }

        public async Task<bool> ToggleResellerStatus(Key key)
        {
            var user = await _userManager.FindByIdAsync(key.Id);
            if(user.ResellerApproved == false)
            {
                user.ResellerApproved = true;
            }
            else if(user.ResellerApproved == true){
                user.ResellerApproved = false;
            }
            var update = await _userManager.UpdateAsync(user);
            return true;
        }


        public Int32 GetResellersActive()
        {
            var result = (from a in _userManager.Users.Where(w => w.Type == "R" && w.ResellerApproved == true) select a.Id).Count();
            return result;
        }

        public Int32 GetResellersTotal()
        {
            var result = (from a in _userManager.Users.Where(w => w.Type == "R") select a.Id).Count();
            return result;
        }

        public Int32 GetMerchantsTotal()
        {
            var result = (from a in _userManager.Users.Where(w => w.Type == "M") select a.Id).Count();
            return result;
        }

        public Int32 GetCustomersTotal()
        {
            var result = (from a in _userManager.Users.Where(w => w.Type == "C") select a.Id).Count();
            return result;
        }


        public Task<DBFiles> DocStatus(Key key)
        {
            throw new NotImplementedException();
        }
    }
}