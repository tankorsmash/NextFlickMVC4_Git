﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.TagModels;

namespace NextFlicksMVC4.Views.Movies.ViewModels
{
    public class FullViewModel
    {
        //public MovieWithGenreViewModel MovieWithGenre { get; set; }

        public Movie Movie { get; set; }

        public IEnumerable<string> Genres { get; set; }

        public BoxArt Boxarts { get; set; }

        public OMBD.OmdbEntry OmdbEntry { get; set; }

        public TagCountViewModel Tags { get; set; }
    }
}