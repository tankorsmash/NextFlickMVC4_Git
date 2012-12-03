using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using DotNetOpenAuth;

namespace NextFlicksMVC4.Models
{
    public class Movie
    {
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

        ///commented this out because I have to make a separate look up for genres now
        //[DisplayName("Genres")]
        //public virtual int genre_id { get; set; }

        [DisplayName("Boxart")]
        public virtual int boxart_id { get; set; }

    }

    public class Genre
    {
        [DisplayName("Genre ID")]
        public int genre_ID { get; set; }

        [DisplayName("Genre")]
        public string genre_string { get; set; }
    }

    public class MovieToGenre
    {
        public virtual int movie_ID { get; set; }
        public virtual int genre_ID { get; set; }
    }

    public  class BoxArt
    {
        public int boxart_id { get; set; }

        public virtual int movie_ID { get; set; }

        [DisplayName("Box Art Size 38")]
        public string boxart_38 { get; set; }
        [DisplayName("Box Art Size 64")]
        public string boxart_64 { get; set; }
        [DisplayName("Box Art Size 110")]
        public string boxart_110 { get; set; }
        [DisplayName("Box Art Size 124")]
        public string boxart_124 { get; set; }
        [DisplayName("Box Art Size 150")]
        public string boxart_150 { get; set; }
        [DisplayName("Box Art Size 166")]
        public string boxart_166 { get; set; }
        [DisplayName("Box Art Size 88")]
        public string boxart_88 { get; set; }
        [DisplayName("Box Art Size 197")]
        public string boxart_197 { get; set; }
        [DisplayName("Box Art Size 176")]
        public string boxart_176 { get; set; }
        [DisplayName("Box Art Size 284")]
        public string boxart_284 { get; set; }
        [DisplayName("Box Art Size 210")]
        public string boxart_210 { get; set; }
    }


    

    public class MovieDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<BoxArt> BoxArts { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieToGenre> MovieToGenres { get; set; }
    }


}
