using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.userAccount;

namespace NextFlicksMVC4.Views.Movies.ViewModels
{
    public class MovieTagViewModel
    {
        public Movie movie { get; set; }

        [DisplayName("List of Genres")]
        [DataType("CommaList")]
        public List<string> genre_strings { get; set; }

        public BoxArt Boxart { get; set; }

        /* don't need ths one any more as it was a sad once trick pony that didnt do wnat i needed
        [DisplayName("Tags")]
        public List<String> Tags { get; set;}
        */

        //This one keeps the tag as the key and the count of how many times the tagis used in the value
        [DisplayName("Tags")]
        public Dictionary<String, int> TagAndCount { get; set; }


        [DisplayName("Anonymous")]
        public bool Anon { get; set; }
    }
}