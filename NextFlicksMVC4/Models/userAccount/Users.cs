using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using DevOne.Security.Cryptography.BCrypt;

namespace NextFlicksMVC4.Models.userAccount
{
    public class Users
    {

       
        [Key]
        public int userID { get; set; }

        [Required]
        [Display(Name = "Email")]
        public String email { get; set; }
        
        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public String password { get; set; }

        

        public int admin { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [Display(Name = "User Name")]
        [RegularExpression(@"(\S)+", ErrorMessage = " White Space is not allowed in User Names")]
        [ScaffoldColumn(false)]
        public String Username { get; set;}

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [Display(Name = "First Name")]
        public String firstName { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3)]
        [Display(Name = "Last Name")]
        public String lastName { get; set; }

        //try the bcrpyt version of encrypting the passwords
        const String ConstantSalt = "fg809rTyu099#!"; //use this to salt password hashes
        
        public String SetHash(string password)
        {

            String mySalt = BCryptHelper.GenerateSalt(10);
            String firstSalt = password + ConstantSalt;
            String myHash = BCryptHelper.HashPassword(firstSalt, mySalt);
            return myHash;
        }
        public bool CheckHash(string maybePwd, string dbPwd)
        {
            String maybeSalt = maybePwd + ConstantSalt;
            return BCryptHelper.CheckPassword(maybeSalt, dbPwd);
        }


        //old method of hashing passwords
        public String SetPassword(string pwd)
        {
            string password = GetHashedPassword(pwd);
            return password;
        }
        

        private String GetHashedPassword(string pwd)
        {
            using (var sha = SHA256.Create())
            {
                var computedHash = sha.ComputeHash(Encoding.Unicode.GetBytes(pwd + ConstantSalt));
                return Convert.ToBase64String(computedHash);
            }
        }

        public bool ValidatePassword(string maybePwd)
        {
            if (password == null)
                return true;
            return password == GetHashedPassword(maybePwd);
        }

        public class UserDbContext : DbContext
        {
            public DbSet<Users> Users { get; set; }
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
            [StringLength(15, MinimumLength = 3)]
            [Display(Name = "First Name")]
            public String firstName { get; set; }

            [Required]
            [StringLength(15, MinimumLength = 3)]
            [Display(Name = "Last Name")]
            public String lastName { get; set; }

            [Required]
            [Display(Name = "Email")]
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
       
    }
}