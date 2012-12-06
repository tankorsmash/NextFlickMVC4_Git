using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using NextFlicksMVC4.Models;

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
    }
}