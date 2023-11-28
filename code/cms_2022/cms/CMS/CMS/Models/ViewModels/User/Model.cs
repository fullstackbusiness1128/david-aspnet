using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using CMS.Models.ViewModels;

namespace CMS.Models.UserViewModels
{
    public class UserViewModel
    {
        public ResultOptions Results { get; set; }

        public List<UserItem> User { get; set; }

        public UserViewModel()
        {
            Results = new ResultOptions();
            User = new List<UserItem>();
        }
    }

    public class UserItem
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public bool ResellerApproved { get; set; }
        public decimal ResellerBalance { get; set; }
        public Int32 DocumentStatus { get; set; } // 0: pending, 1: approved, 2: cancelled
        public string DocumentFurl { get; set; }
        public Int32 DocumentId { get; set; }

    }

    public class MDBFiles
    {
        public Int32 Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public Int64 DatetimeCreated { get; set; }
        public Int64 DatetimeModified { get; set; }
        public bool RecordStatus { get; set; }
        public string Furl { get; set; }
        public string Extension { get; set; }
        public Int32 Status { get; set; } // 0 = Pending, 1 = Declined, 2 = Approved
        public string Size { get; set; }
        public string Category { get; set; }
        public string UserId { get; set; }

    }
}
