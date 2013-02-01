using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NextFlicksMVC4.Models
{
    public class FeedbackModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Message { get; set; }
    }
}