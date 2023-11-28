using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string Firstname { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string Lastname { get; set; }

        [Required]
        [Display(Name = "User Level")]
        public string Level { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Facebook Link")]
        public string LinkFacebook { get; set; }

        [Display(Name = "Instagram Link")]
        public string LinkInstagram { get; set; }

        [Display(Name = "Twitter Link")]
        public string LinkTwitter { get; set; }

        [Display(Name = "Youtube Link")]
        public string LinkYoutube { get; set; }

        [Display(Name = "Website Link")]
        public string LinkUrl { get; set; }
    }
}
