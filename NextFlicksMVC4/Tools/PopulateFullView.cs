using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.OMBD;
using NextFlicksMVC4.Views.Movies.ViewModels;
using NextFlicksMVC4.Models.TagModels;

namespace NextFlicksMVC4
{
    public static class PopulateFullView
    {
        public static OmdbEntry Omdb(int movieID)
        {
            MovieDbContext db = new MovieDbContext();
            OmdbEntry omdb = new OmdbEntry();

            omdb = Tools.MatchMovieIdToOmdbEntry(movieID);
            return omdb;
            /* for posterity in case Tools.Cs changes
             *  public static OmdbEntry MatchMovieIdToOmdbEntry(int movie_ID)
                {
                    MovieDbContext db = new MovieDbContext();
                    var omdbEntry = db.Omdb.FirstOrDefault(omdb => omdb.movie_ID == movie_ID);

                   return omdbEntry;
                }*/

        }

        public static IEnumerable<string> Genres(int movieID)
        {
            MovieDbContext db = new MovieDbContext();
            List<string> genreList = new List<string>();
 
            var movieGenres = from mtg in db.MovieToGenres
                              join genre in db.Genres
                                  on mtg.genre_ID equals genre.genre_ID
                              group genre.genre_string by mtg.movie_ID
                                  into gs
                                  where gs.Key == movieID
                                  select gs;
            genreList = movieGenres.First().ToList();
            //put the list in an IEnumerable<String> so it can go into the full view model. Don't want to change the model
            //to take a List<String> because tankorsmash may need it that way for some reason.
            IEnumerable<string> genreEnum = genreList;
            
            return genreEnum;
        }

        public static BoxArt BoxArts(int movieID)
        {
            MovieDbContext db = new MovieDbContext();
            BoxArt boxArts = new BoxArt();

            var boxArtList = db.BoxArts.Where(
                    item => item.movie_ID == movieID).ToList();
            boxArts = boxArtList[0];
            return boxArts;
        }

        public static TagCountViewModel TagsAndCount(int movieID)
        {
            MovieDbContext db = new MovieDbContext();
            TagCountViewModel tagView = new TagCountViewModel(){ TagAndCount = new Dictionary<string, int>()};

            //find the ids of the tags related to the movie
            var tagsForMovie = from MtT in db.UserToMovieToTags
                               where MtT.movie_ID == movieID
                               select MtT.TagId;
            
            //find the tag string based on the id of the strings
            var tagStrings = from tag in db.MovieTags
                             from tag_id in tagsForMovie
                             where tag.TagId == tag_id
                             select tag.Name;

            var tagCounts = from tagString in db.MovieTags
                            where tagStrings.Contains(tagString.Name)
                            from MtT in db.UserToMovieToTags
                            where MtT.TagId == tagString.TagId
                            group MtT.TagId by tagString.Name
                            into grouping
                            select grouping;

            foreach (IGrouping<string, int> result in tagCounts)
            {
                tagView.TagAndCount.Add(result.Key, result.Count());
            }
            

            return tagView; 
        }

        public static string Plot(Movie movie)
        {
            //make a call to omdb and get the plot
            string plot;
            string response_format = "xml";
            var url = String.Format(
                @"http://www.omdbapi.com/?t={0}&y={1}&r={2}",
                movie.short_title, movie.year, response_format);
            //create the webrequest, add gzip encoding.
            HttpWebRequest web = (HttpWebRequest) WebRequest.Create(url);
            web.AutomaticDecompression = DecompressionMethods.GZip |
                                         DecompressionMethods.Deflate;

            //get the response
            var resp = web.GetResponse();
            var stream = resp.GetResponseStream();
            //make sure there's actually a response
            if (stream != null) {
                StreamReader respStream = new StreamReader(stream);
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(respStream.ReadToEnd());

                //get the plot for the story, and then assign it to the FullView
                if (xDoc.InnerXml.Contains("Movie not") != true) {
                    plot =
                        xDoc.GetElementsByTagName("movie")[0].Attributes["plot"]
                            .Value;
                    Tools.TraceLine("The plot is: {0}", plot);
                }
                else {
                    plot = @"N/A";
                    Tools.TraceLine("The plot is: {0}", plot);
                }
            }
            else {
                plot = @"N/A";
            }
            return plot;
        }
    }
}