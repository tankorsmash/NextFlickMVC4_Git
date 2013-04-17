using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NextFlicksMVC4.Views.Movies.ViewModels;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.OMBD;

namespace NextFlicksMVC4
{
    public static class MovieSearch
    {
        public static IQueryable<FullViewModel> ByTitle(string searchTerm, MovieDbContext db)
        {
            //var db = new MovieDbContext();

            IQueryable<FullViewModel> res;

            var movies = from movie in db.Movies
                         where movie.short_title.Contains(searchTerm)
                         select movie;
            res = CreateFullView(movies, db);
            return res;
        }
        
        public static IQueryable<FullViewModel> ByGenre(string searchTerm, MovieDbContext db)
        {
            IQueryable<FullViewModel> res;

            //get genres that match search term
            var genres = from genre in db.Genres
                         where genre.genre_string.Contains(searchTerm.ToLower())
                         select genre;
            //get genre to movie ids
            var genToMov = from gtm in db.MovieToGenres
                           from genre in genres
                           where gtm.genre_ID == genre.genre_ID
                           select gtm;
            //use genre to movie ids to collect list of movies
            var movies = from movie in db.Movies
                         from gtm in genToMov
                         where movie.movie_ID == gtm.movie_ID
                         select movie;

            res = CreateFullView(movies, db);
            return res;
        }
        
        public static IQueryable<FullViewModel> ByTag(string searchTerm, MovieDbContext db)
        {
            IQueryable<FullViewModel> res;

            var tags = from tag in db.MovieTags
                         where tag.Name.Contains(searchTerm)
                         select tag;
            var tagsToMovs = from ttm in db.UserToMovieToTags
                             from tag in tags
                             where ttm.TagId == tag.TagId
                             select ttm;
            var movies = from movie in db.Movies
                         from ttm in tagsToMovs
                         where ttm.movie_ID == movie.movie_ID
                         select movie;
            res = CreateFullView(movies, db);
            return res;
        }
        /*
        public static IQueryable<FullViewModel> ByRottenTomatoMeter(string searchTerm)
        {
            var db = new MovieDbContext();
            IQueryable<FullViewModel> res;

            var rtMeter = from rt in db.OmdbEntries
                         where rt.t_meter >= searchTerm
                         select rt;
            return res;
        }

        public static IQueryable<FullViewModel> ByRating(string searchTerm)
        {
            var db = new MovieDbContext();

            IQueryable<FullViewModel> res;

            var movies = from movie in db.Movies
                         where movie.short_title.Contains(searchTerm)
                         select movie;
            return res;
        }

        public static IQueryable<FullViewModel> ByYear(string searchTerm)
        {
            var db = new MovieDbContext();

            IQueryable<FullViewModel> res;

            var movies = from movie in db.Movies
                         where movie.short_title.Contains(searchTerm)
                         select movie;
            return res;
        }

        public static IQueryable<FullViewModel> ByStars(string searchTerm)
        {
            var db = new MovieDbContext();

            IQueryable<FullViewModel> res;

            var movies = from movie in db.Movies
                         where movie.short_title.Contains(searchTerm)
                         select movie;
            return res;
        }
        */

        public static IQueryable<FullViewModel> CreateFullView(IQueryable<Movie> movies, MovieDbContext db)
        {
           // var db = new MovieDbContext();

            var movieID_genreString_grouping = from mtg in db.MovieToGenres
                                               join genre in db.Genres on
                                                   mtg.genre_ID equals
                                                   genre.genre_ID
                                               group genre.genre_string by
                                                   mtg.movie_ID;


            //a default empty element
            OmdbEntry defaultOmdbEntry = new OmdbEntry
            {
            };

           
            var nitvmQuery =
                //left outer join so that all movies get selected even if there's no omdb match
                from movie in movies
                join omdb in db.Omdb on
                    movie.movie_ID equals omdb.movie_ID into mov_omdb_matches
                from mov_omdb_match in mov_omdb_matches.DefaultIfEmpty()
                //match the boxarts
                from boxart in db.BoxArts
                where movie.movie_ID == boxart.movie_ID
                //match the genres string
                from grp in movieID_genreString_grouping
                where grp.Key == movie.movie_ID
                //match the genre id

                //create the NITVM
                select new FullViewModel
                {
                    Movie = movie,
                    Boxarts = boxart,
                    Genres = grp,
                    Genre_IDs = (List<int>)(from mtg in db.MovieToGenres
                                            where mtg.movie_ID == movie.movie_ID
                                            select mtg.genre_ID),
                    //OmdbEntry = (mov_omdb_match == null) ? mov_omdb_match.movie_ID= movie.movie_ID: mov_omdb_match
                    OmdbEntry = mov_omdb_match
                };

            return nitvmQuery;
        }
    }
}