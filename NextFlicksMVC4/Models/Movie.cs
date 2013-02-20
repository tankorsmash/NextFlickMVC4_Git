using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using NextFlicksMVC4.Models.userAccount;
using NextFlicksMVC4.Tracking;

namespace NextFlicksMVC4.Models
{
    public class Movie
    {

        [Key]
        public int movie_ID { get; set; }

        [DisplayName("Title")]
        public string short_title { get; set; }

        [DisplayName("Year Release")]
        public int year { get; set; }

        [DisplayName("Runtime")]
        [DataType("Runtime")]
        public int runtime { get; set; }

        [DisplayName("Netflix Rating")]
        public string avg_rating { get; set; }

        [DisplayName("MPAA Rating")]
        public string tv_rating { get; set; }

        [DataType("NetflixWebPage")]
        public string web_page { get; set; }

        [DisplayName("Ssn")]
        
        public string current_season { get; set; }

        [DisplayName("Is a Movie")]
        public bool is_movie { get; set; }


        [DisplayName("Maturity Rating")]
        public int maturity_rating { get; set; }


    }

    public class Genre
    {
        [DisplayName("Genre ID")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int genre_ID { get; set; }

        [DisplayName("Genre")]
        public string genre_string { get; set; }
    }

    public class MovieToGenre
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int movie_to_genre_ID { get; set; }

        public virtual int movie_ID { get; set; }

        public virtual int genre_ID { get; set; }
    }

    public  class BoxArt
    {
        [Key]
        public int boxart_ID { get; set; }

        public virtual int movie_ID { get; set; }

        [DisplayName("Box Art Size 38")]
        [DataType("BoxArtUrl")]
        public string boxart_38 { get; set; }

        [DisplayName("Box Art Size 64")]
        [DataType("BoxArtUrl")]
        public string boxart_64 { get; set; }

        [DisplayName("Box Art Size 110")]
        [DataType("BoxArtUrl")]
        public string boxart_110 { get; set; }

        [DisplayName("Box Art Size 124")]
        [DataType("BoxArtUrl")]
        public string boxart_124 { get; set; }

        [DisplayName("Box Art Size 150")]
        [DataType("BoxArtUrl")]
        public string boxart_150 { get; set; }

        [DisplayName("Box Art Size 166")]
        [DataType("BoxArtUrl")]
        public string boxart_166 { get; set; }

        [DisplayName("Box Art Size 88")]
        [DataType("BoxArtUrl")]
        public string boxart_88 { get; set; }

        [DisplayName("Box Art Size 197")]
        [DataType("BoxArtUrl")]
        public string boxart_197 { get; set; }

        [DisplayName("Box Art Size 176")]
        [DataType("BoxArtUrl")]
        public string boxart_176 { get; set; }

        [DisplayName("Box Art Size 284")]
        [DataType("BoxArtUrl")]
        public string boxart_284 { get; set; }

        [DisplayName("Box Art Size 210")]
        [DataType("BoxArtUrl")]
        public string boxart_210 { get; set; }

    }

    public class MovieTag
    {
        [Key]
        public int TagId { get; set; }
        public string Name { get; set; }
    }

    public class UserToMovieToTags
    {
        [Key]
        public int UtMtY_ID { get; set; }
        public virtual int UserID { get; set; }
        public virtual int TagId { get; set; }
        public virtual int movie_ID { get; set; }
    }

    public  class  UtMtTisAnon
    {
        [Key]
        public int UtMtTiA_ID { get; set; }
        public virtual int UtMtT_ID { get; set; }
        public bool IsAnon { get; set; }

    }
   

    public class MovieDbContext : DbContext
    {
        public MovieDbContext() 
            : base("MovieDbContext")
        {
        }
        
        public DbSet<Movie> Movies { get; set; }
        public DbSet<BoxArt> BoxArts { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieToGenre> MovieToGenres { get; set; }
        public DbSet<OMBD.OmdbEntry> Omdb { get; set; }
        public DbSet<MovieTag> MovieTags { get; set; }
        public DbSet<UserToMovieToTags> UserToMovieToTags { get; set; }
        public DbSet<UtMtTisAnon> UtMtTisAnon { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }

        
    }


}
