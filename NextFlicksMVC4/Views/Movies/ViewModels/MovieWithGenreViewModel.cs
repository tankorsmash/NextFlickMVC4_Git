using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NextFlicksMVC4.Models;

namespace NextFlicksMVC4.Views.Movies.ViewModels
{
    public class MovieWithGenreViewModel
    {
        public Movie movie { get; set; }

        [DisplayName("List of Genres")]
        [DataType("CommaList")]
        public List<string> genre_strings { get; set; }

        public BoxArt boxart { get; set; }
    }
}