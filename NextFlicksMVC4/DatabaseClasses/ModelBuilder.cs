using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextFlicksMVC4;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.Controllers;
using System.Timers;
using NextFlicksMVC4.Helpers;
using System.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NextFlicksMVC4.DatabaseClasses
{
    public static class ModelBuilder
    {
        /// <summary>
        /// returns a list of MwGs
        /// </summary>
        /// <param name="db"></param>
        /// <param name="qry">Make sure that the query returns movietogenres.genre_id, movietogenres.movie_id, genres.genre_string </param>
        /// <param name="genre_params"></param>
        /// <returns></returns>
        public static List<MoviesController.MovieWithGenreViewModel> CreateMovieWithGenreViewModelList(MovieDbContext db, string genre_params)
        {
            //select moviewithgenresviewmodel that match the genre string
            string qry = @"
    SELECT movietogenres.genre_id, 
           movietogenres.movie_id, 
           genres.genre_string 
FROM   movietogenres 
       INNER JOIN genres 
               ON movietogenres.genre_id = genres.genre_id 
WHERE  ( movietogenres.movie_id IN (SELECT DISTINCT movies.movie_id AS movieid 
                                    FROM   movies 
                                           INNER JOIN movietogenres AS 
                                                      MovieToGenres_1 
                                                   ON movies.movie_id = 
                                                      MovieToGenres_1.movie_id 
                                           INNER JOIN genres AS Genres_1 
                                                   ON MovieToGenres_1.genre_id = 
                                                      Genres_1.genre_id 
                                    WHERE  ( Genres_1.genre_string LIKE {0} + 
                                             '%' )) ) ";


            //var qwe = from m in db.Movies
            //          join g in db.MovieToGenres on g.movie_id equals m.movie_id
            //          where m.movie_id == 1
            //          select new Movie() {movie_ID = m.movie_id};

            var res = db.Database.SqlQuery<MoviesController.MovieToGenreViewModel>(qry,
                                                                  genre_params);
            List<MoviesController.MovieToGenreViewModel> movie_list = res.ToList();

            //loop over MtG and fill a dict with the associated strings to movie
            Dictionary<int, List<string>> dict_movId_genStr =
                new Dictionary<int, List<string>>();

            //fo each model, add its genre string to a key of movie id, so there's one movie ID to several genre strings
            foreach (MoviesController.MovieToGenreViewModel movieToGenreViewModel in movie_list)
            {
                //if dict does not contain an entry for the key, make one, and inst
                // a list for it too
                if (!dict_movId_genStr.ContainsKey(movieToGenreViewModel.movie_id))
                {
                    dict_movId_genStr[movieToGenreViewModel.movie_id] =
                        new List<string>(); }

                dict_movId_genStr[movieToGenreViewModel.movie_id].Add(
                    movieToGenreViewModel.genre_string);
            }

            //a list of all movies that are in the dict of movie_id[genre_str]
            var matched_movies =
                db.Movies.Where(
                    movie => dict_movId_genStr.Keys.Contains(movie.movie_ID))
                  .ToList();
            var matched_boxarts =
                db.BoxArts.Where(
                    boxart => dict_movId_genStr.Keys.Contains(boxart.movie_ID))
                  .ToList();


            //add the genres and Movie to the ViewModel
            Trace.WriteLine("Creating ViewModel with Genres and Movies");
            var MwG_list = new List<MoviesController.MovieWithGenreViewModel>();
            foreach (var movie in matched_movies)
            {
                MwG_list.Add(new MoviesController.MovieWithGenreViewModel
                                 {
                                     movie = movie,
                                     genre_strings =
                                         dict_movId_genStr[movie.movie_ID]
                                 });
            }
            //add the BoxArts to the viewmodel
            Trace.WriteLine("Adding the boxArts");
            foreach (BoxArt matchedBoxart in matched_boxarts)
            {
                var viewModel =
                    MwG_list.First(
                        movie => movie.movie.movie_ID == matchedBoxart.movie_ID);
                viewModel.boxart = matchedBoxart;
            }
            Trace.WriteLine("Done adding boxarts in Genres");



            return MwG_list;
        }

        /// <summary>
        /// Takes a list of movies and finds the boxart and genres to return a 
        /// List of MovieWithGenreViewModel
        /// </summary>
        /// <param name="movie_list"></param>
        /// <returns>a list of MovieWithGenreViewModels </returns>
        public static List<MoviesController.MovieWithGenreViewModel> CreateListOfMtGVM(MovieDbContext db,
            List<Movie> movie_list)
        {
            //find all MtGs with movie_id matching movie_list
            var MtG_list = GetListOfMatchingMTGs(db, movie_list);
            //find all genre strings from a list of MtGs
            var movie_to_genre_string_dict = GetDictOfMovieIdsToGenreStrings(db, MtG_list);
            
            List<BoxArt>  boxart_list = new List<BoxArt>();
            List<int> movie_id_list= new List<int>();
            movie_id_list = movie_list.Select(item2 => item2.movie_ID).ToList();
            boxart_list = db.BoxArts.Where(
                item => movie_id_list.Contains(item.movie_ID)).ToList();

            ////TODO: more harcoding that needs to be solved
            //boxart_list.Add(new BoxArt
            //                    {
            //                        movie_ID = 1,

            //                        boxart_38 = "6597/166597",
            //                        boxart_64 = "5818/275818",
            //                        boxart_110 = "4232/494232",
            //                        boxart_124 = "6242/626242",
            //                        boxart_150 = "9783/729783",
            //                        boxart_166 = "8873/828873",
            //                        boxart_88 = "5054/385054",
            //                        boxart_197 = "8251/938251",
            //                        boxart_176 = "3400/1373400",
            //                        boxart_284 = "0277/1060277",
            //                        boxart_210 = "4412/1004412" 
            //                    });

            //add all the genre definitions and boxarts to the appropriate movie and return that
            var MwG_list =
                movie_list.Select(
                    movie => new MoviesController.MovieWithGenreViewModel
                                 {
                                     movie = movie,
                                     genre_strings =
                                         movie_to_genre_string_dict[
                                             movie.movie_ID],
                                     boxart =
                                         boxart_list.First(
                                             item =>
                                             item.movie_ID == movie.movie_ID)
                                 }).ToList();


            Trace.WriteLine(MtG_list.Count.ToString());


            return MwG_list;
        }

        public static Dictionary<int, List<string>> GetDictOfMovieIdsToGenreStrings(MovieDbContext db,
                                                                 List<MovieToGenre> MtG_list)
        {
            //find all the genre strings for the MtGs from the list
            var genreid_list = MtG_list.Select(item => item.genre_ID).ToList();
            Dictionary<int, string> genre_definitions = new Dictionary<int, string>();

            //find the definitions loop over all the genre ids, and if it's not
            // already defined, find the string name for it
            foreach (int id in genreid_list) {
                if (!genre_definitions.ContainsKey(id)) {
                    string definition =
                        db.Genres.Where(item => item.genre_ID == id)
                          .Select(item => item.genre_string).First();
                    genre_definitions[id] = definition;
                }
            }

            //associate movies with lists of genre defs
            Dictionary<int, List<string>> movie_to_genre_string_dict =
                new Dictionary<int, List<string>>();
            foreach (MovieToGenre movieToGenre in MtG_list) {
                //if dict does not contain an entry for the key, make one, and inst
                // a list for it too
                if (!movie_to_genre_string_dict.ContainsKey(movieToGenre.movie_ID)) {
                    movie_to_genre_string_dict[movieToGenre.movie_ID] =
                        new List<string>();
                }

                movie_to_genre_string_dict[movieToGenre.movie_ID].Add(
                    genre_definitions[movieToGenre.genre_ID]);
            }

            ////todo: fix TERRIBLE solution to missing values
            ////hard code the missing value
            //movie_to_genre_string_dict[1] = new List<string> { "Cult Movies", "Horror Movies", "Cult Horror Movies", "Satanic Stories", "Supernatural Horror Movies" };

            return movie_to_genre_string_dict;
        }

        /// <summary>
        /// returns a list of MtGs that match the ids of the movie list you pass
        /// </summary>
        /// <param name="db"></param>
        /// <param name="movie_list"></param>
        /// <returns></returns>
        public static List<MovieToGenre> GetListOfMatchingMTGs(MovieDbContext db, List<Movie> movie_list)
        {
            var movie_ids = movie_list.Select(movie => movie.movie_ID)
                                      .ToList();
            var mtg_res = db.MovieToGenres.Where(
                mtg => movie_ids.Contains(mtg.movie_ID));

            //this is a list of MovieToGenres that I'll use to make that dict of ID to strings
            var MtG_list = mtg_res.ToList();
            return MtG_list;
        }
    }
}