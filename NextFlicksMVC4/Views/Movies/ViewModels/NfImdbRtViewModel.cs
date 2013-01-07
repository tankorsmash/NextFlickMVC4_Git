using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextFlicksMVC4.Views.Movies.ViewModels
{
    public class NfImdbRtViewModel
    {
        public MovieWithGenreViewModel MovieWithGenre { get; set; }

        public OMBD.OmdbEntry OmdbEntry { get; set; }
    }
}