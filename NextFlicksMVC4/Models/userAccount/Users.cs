using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using DevOne.Security.Cryptography.BCrypt;

namespace NextFlicksMVC4.Models.userAccount
{
    [Table("UserProfile")]
    public class Users
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int userID { get; set; }
        
        [Required]
        [StringLength(15, MinimumLength = 3)]
        [Display(Name = "User Name")]
        [RegularExpression(@"(\S)+", ErrorMessage = " White Space is not allowed in User Names")]
        [ScaffoldColumn(false)]
        public String Username { get; set; }

        [Required]
        [Display(Name = "Email")]
        public String email { get; set; }


        public class PasswordChangeModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public class RegistrationViewModel
        {
            /// <summary>
            /// I needed to create a Registration View model so that I could validate the passwords, as User class
            /// did not have a ConfirmPassword field in the database the check while using the model failed.
            /// Created this new model with the needed fields and password checks. once Posted haveto convert 
            /// back to user model instad of registrationview model
            /// </summary>
            [Required]
            [StringLength(15, MinimumLength = 3)]
            [Display(Name = "User Name")]
            [RegularExpression(@"(\S)+", ErrorMessage = " White Space is not allowed in User Names")]
            [ScaffoldColumn(false)]
            public String Username { get; set; }

            [Required]
            [Display(Name = "Email")]
            [Email]
            public String email { get; set; }

            [Required]
            [Display(Name = "Password")]
            [DataType(DataType.Password)]
            public String password { get; set; }
            
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Re-enter Password")]
            [Compare("password", ErrorMessage = "Passwords do not match.")]
            public String comparePassword { get; set; }
        }

        public class LogonModel
        {
            [Required]
            public String Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public String Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }

        }
    }
}