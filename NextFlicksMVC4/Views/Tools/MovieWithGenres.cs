using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace NextFlicksMVC4.Views.Tools
{

    [Obsolete("I don't actually think this was ever used...", true)]
    public class MovieWithGenres
    {


        [Key]
        public int movie_ID { get; set; }

        [DisplayName("Title")]
        public string short_title { get; set; }

        [DisplayName("Year Release")]
        public string year { get; set; }

        [DisplayName("Runtime")]
        public TimeSpan runtime { get; set; }

        [DisplayName("Netflix Rating")]
        public string avg_rating { get; set; }

        [DisplayName("MPAA Rating")]
        public string tv_rating { get; set; }

        public string web_page { get; set; }

        [DisplayName("Ssn")]
        public string current_season { get; set; }

        [DisplayName("Is a Movie")]
        public bool is_movie { get; set; }


        [DisplayName("Maturity Rating")]
        public string maturity_rating { get; set; }

        //commented this out because I have to make a separate look up for genres now
        //[DisplayName("Genres")]
        //public virtual int genre_ID { get; set; }

        //[DisplayName("Boxart")]
        //public virtual int boxart_ID { get; set; }

    }
}