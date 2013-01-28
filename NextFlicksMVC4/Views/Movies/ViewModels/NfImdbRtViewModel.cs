using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using NextFlicksMVC4.Models;

namespace NextFlicksMVC4.Views.Movies.ViewModels
{
    public class NfImdbRtViewModel
    {
        //public MovieWithGenreViewModel MovieWithGenre { get; set; }

        public Movie Movie { get; set; }

        [DataType("CommaList")]
        public IEnumerable<string> Genres { get; set; }

        public List<int> Genre_IDs { get; set; } 

        public BoxArt Boxarts { get; set; }

        public OMBD.OmdbEntry OmdbEntry { get; set; }
    }
}