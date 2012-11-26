using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using DotNetOpenAuth;

namespace NextFlicksMVC4.Models
{
    public class Movie
    {
        public int ID { get; set; }

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

        public  Movie()
        {
        }
    }

    

    public class MovieDBContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
    }


}
