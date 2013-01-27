using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NextFlicksMVC4.Models.userAccount;

namespace NextFlicksMVC4.Models.TagModels
{
    public class TagDetailViewModel
    {
        public String TagName { get; set; }
        public List<Movie> TaggedMovies { get; set; }
        public Dictionary<Users, int> TaggedByUsers { get; set; } 

    }
}