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
using NextFlicksMVC4.Views.Movies.ViewModels;

namespace NextFlicksMVC4.DatabaseClasses
{
    public static class ModelBuilder
    {
        /// <summary>
        /// OBSOLETE returns a list of MwGs
        /// </summary>
        /// <param name="db"></param>
        /// <param name="qry">Make sure that the query returns movietogenres.genre_id, movietogenres.movie_id, genres.genre_string </param>
        /// <param name="genre_params"></param>
        /// <returns></returns>
        [Obsolete("this uses SQL, please use a different method that uses Linq", true)]
        private  static List<MovieWithGenreViewModel> CreateMovieWithGenreViewModelList(MovieDbContext db, string genre_params)
        {

            // linq to sql example
            //var query = from movie in db.Movies
            //            where movie.movie_ID == 1
            //            select movie;

            //var genre_ids = db.Genres.Where(
            //    item => item.genre_string.ToLower().StartsWith("action"))
            //  .Select(item => item.genre_ID);
            //var movies_ids_matching_the_genre = db.MovieToGenres.Where(item => genre_ids.Contains(item.genre_ID)).Select(item => item.movie_ID);


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

            var res = db.Database.SqlQuery<MovieToGenreViewModel>(qry,
                                                                  genre_params);
            List<MovieToGenreViewModel> movie_list = res.ToList();

            //loop over MtG and fill a dict with the associated strings to movie
            Dictionary<int, List<string>> dict_movId_genStr =
                new Dictionary<int, List<string>>();

            //fo each model, add its genre string to a key of movie id, so there's one movie ID to several genre strings
            foreach (MovieToGenreViewModel movieToGenreViewModel in movie_list)
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
            var MwG_list = new List<MovieWithGenreViewModel>();
            foreach (var movie in matched_movies)
            {
                MwG_list.Add(new MovieWithGenreViewModel
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
        public static List<MovieWithGenreViewModel>
            CreateListOfMwGVM(MovieDbContext db,
            List<Movie> movie_list)
        {
            //find all MtGs with movie_id matching movie_list
            var MtG_list = GetListOfMatchingMTGs(db, movie_list);
            //find all genre strings from a list of MtGs
            var movie_to_genre_string_dict = GetDictOfMovieIdsToGenreStrings(db, MtG_list);
            
            //select all boxarts that match a movie_id from the list
            var boxart_list = GetListOfBoxarts(db, movie_list);

            //add all the genre definitions and boxarts
            //  to the appropriate movie and return that

            ///fill a list with new MwGVMs based on the movie, genre_strings and boxarts
            var MwG_list =
                movie_list.Select(
                    movie => new MovieWithGenreViewModel
                                 {
                                     //movie
                                     movie = movie,
                                     genre_strings = getDictValueOrDefault(
                                         movie_to_genre_string_dict,
                                             movie.movie_ID),
                                     //boxart
                                     boxart =
                                         boxart_list.FirstOrDefault(

                                             item =>
                                             item.movie_ID == movie.movie_ID)
                                 }).ToList();


            //Trace.WriteLine(MtG_list.Count.ToString());


            return MwG_list;
        }

        //returns either the value in the dict, or the preset default value if the key isn't found
        public static List<String> getDictValueOrDefault(
            Dictionary<int, List<string>> movie_to_genre_string_dict,
            int movie_id, string defaultString = "Not_Found")
        {
            try {
                return movie_to_genre_string_dict[movie_id];
            }

            catch (KeyNotFoundException exception) {
                return new List<string> {defaultString};
            }
        } 

        public static List<BoxArt> GetListOfBoxarts(MovieDbContext db, List<Movie> movie_list)
        {
            List<BoxArt> boxart_list = new List<BoxArt>();
            List<int> movie_ids = new List<int>();
            movie_ids = movie_list.Select(item2 => item2.movie_ID).ToList();
            if (movie_ids.Count <= 50000) {
                boxart_list = db.BoxArts.Where(
                    item => movie_ids.Contains(item.movie_ID)).ToList();
            }
            else {
                Tools.TraceLine("Too many ids for Boxarts{0}", movie_ids.Count);

                //first 50 000 
                List<int> first50000 = movie_ids.Take(50000).ToList();
                var first_res =
                    db.BoxArts.Where(
                        MtG => first50000.Contains(MtG.movie_ID));
                var first_list = first_res.ToList();

                //remainder
                var rem_ids = movie_ids.Count - 50000;
                List<int> remaining_id_list = movie_ids.Take(rem_ids).ToList();
                var rem_res =
                    db.BoxArts.Where(
                        MtG => remaining_id_list.Contains(MtG.movie_ID));
                var rem_list = rem_res.ToList();

                //combine and join
                boxart_list = new List<BoxArt>();
                boxart_list.AddRange(first_list);
                boxart_list.AddRange(rem_list);

            }
            return boxart_list;
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

            List<MovieToGenre> MtG_list;
            //if there's few enough movie_ids business as usual
            if (movie_ids.Count <= 50000) {
                var mtg_res = db.MovieToGenres.Where(
                    mtg => movie_ids.Contains(mtg.movie_ID));

                MtG_list = mtg_res.ToList();
            }
            //there's too many ids to work through at a time, split the calls up a
                //and join em later
            else {
                Tools.TraceLine("Too many ids for MtGs {0}", movie_ids.Count);

                //first 50 000 
                List<int> first50000 = movie_ids.Take(50000).ToList();
                var first_res =
                    db.MovieToGenres.Where(
                        MtG => first50000.Contains(MtG.movie_ID));
                var first_list = first_res.ToList();

                //remainder
                var rem_ids = movie_ids.Count - 50000;
                List<int> remaining_id_list = movie_ids.Take(rem_ids).ToList();
                var rem_res =
                    db.MovieToGenres.Where(
                        MtG => remaining_id_list.Contains(MtG.movie_ID));
                var rem_list = rem_res.ToList();

                //combine and join
                MtG_list = new List<MovieToGenre>();
                MtG_list.AddRange(first_list);
                MtG_list.AddRange(rem_list);

            }

            //this is a list of MovieToGenres that I'll use to make that dict of ID to strings
            //List<MovieToGenre> MtG_list;
            //try {
            //    MtG_list = mtg_res.ToList();
            //}
            //catch (System.Data.EntityCommandExecutionException) {
            //    var first5000 = mtg_res.Take(5000).ToList();
            //    var leftover = mtg_res.Take(mtg_res.Count() - 5000).ToList();

            //    MtG_list = new List<MovieToGenre>();
            //    MtG_list.AddRange(first5000);
            //    MtG_list.AddRange(leftover);
            //}

            return MtG_list;
        }
    }
}