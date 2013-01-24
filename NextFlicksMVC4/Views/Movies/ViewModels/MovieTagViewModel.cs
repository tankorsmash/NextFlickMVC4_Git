using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NextFlicksMVC4.Models;

namespace NextFlicksMVC4.Views.Movies.ViewModels
{
    public class MovieTagViewModel
    {
        public Movie movie { get; set; }

        [DisplayName("List of Genres")]
        [DataType("CommaList")]
        public List<string> genre_strings { get; set; }

        public BoxArt Boxart { get; set; }

//        [DisplayName("Tags")]
//        [DataType("CommaList")]
//        public List<String> Tags { get; set; }
        
        [DisplayName("Tags")]
        public List<MovieTags> Tags { get; set;}

        [DisplayName("Movie Tagged By")]
        public Dictionary<int, List<String>> TaggedBy { get; set; }

        [DisplayName("Anonymous")]
        public bool Anon { get; set; }
    }
}