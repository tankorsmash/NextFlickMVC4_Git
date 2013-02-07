using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using NextFlicksMVC4.Models;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Mvc;

namespace NextFlicksMVC4.NetFlixAPI
{
    //parses a text file for id and genres, then fills a table with it
    public static class PopulateGenres
    {

        /// <summary>
        /// return a dict of id to genre both strings.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static Dictionary<string, string> CreateDictofGenres(string filepath)
        {
            var genres = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(filepath))
            {
                //build regex
                //(digits) space (characters)
                string pat = @"(\d+) +(\w.+)";
                Regex regex = new Regex(pat); 

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = regex.Match(line);

                    string id = match.Groups[1].Value;
                    string genre = match.Groups[2].Value;

                    genres[id] = genre;
                }
            }

            return genres;
        }

        /// <summary>
        /// create a Genre model, takes two strings
        /// </summary>
        /// <param name="id_string"></param>
        /// <param name="genre_string"></param>
        /// <returns></returns>
        public static Genre CreateGenreModel(string id_string, string genre_string)
        {
            Genre genre = new Genre
                              {
                                  genre_ID = Int32.Parse(id_string),
                                  genre_string = genre_string
                              };

            return genre;
        }

        public static void PopulateGenresTable()
        {
            Tools.TraceLine("In PopulateGenresTable");

            var db = new MovieDbContext();

            //if genre table is not empty, its probably full and don't do anything

            //var db = NextFlicksMVC4.Controllers.MoviesController.db;
            //if (db.Genres.Count() != 0)
            //{
            //    Trace.WriteLine("Genre table already is not empty, assuming it's full, so no action was taken");
            //    return;
            //}

            //returns a dict of id to genres
            Dictionary<string, string> dict = NetFlixAPI.PopulateGenres.CreateDictofGenres(System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/genres.NFPOX"));
            //create all the genre models
            List<Genre> genres = new List<Genre>();
            foreach (KeyValuePair<string, string> keyValuePair in dict)
            {
                //create genres
                var id = keyValuePair.Key;
                var genre_string = keyValuePair.Value;
                Genre genre = NetFlixAPI.PopulateGenres.CreateGenreModel(id,
                                                                         genre_string);
                //add to list
                genres.Add(genre);

            }

            //add to and save table
            Trace.WriteLine(" starting to add genres to the database");
            foreach (Genre genre in genres)
            {
                db.Genres.Add(genre);
            }


            Trace.WriteLine("  starting to savechanges() ");
            try {
                db.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex) {
                Tools.TraceLine("  caught error while saving Genres Table. It probably already exists:\n***{0}", ex.Message);
            }

            Tools.TraceLine("Out PopulateGenresTable");
        }
    }
}