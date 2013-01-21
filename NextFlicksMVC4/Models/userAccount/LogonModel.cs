using System;
using System.ComponentModel.DataAnnotations;

namespace NextFlicksMVC4.Models.userAccount
{
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
