using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.OMBD;
using NextFlicksMVC4.Views.Movies.ViewModels;

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

        public static IEnumerable<String> Genres(int movieID)
        {
            MovieDbContext db = new MovieDbContext();
            List<String> genreList = new List<string>();
 
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
            IEnumerable<String> genreEnum = genreList;
            
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

        public static TagViewModel TagsAndCount(int movieID)
        {
            MovieDbContext db = new MovieDbContext();
            TagViewModel tagView = new TagViewModel(){ TagAndCount = new Dictionary<string, int>()};

            //find the ids of the tags related to the movie
            var tagsForMovie = from MtT in db.UserToMovieToTags
                               where MtT.movie_ID == movieID
                               select MtT.TagId;
            
            //find the tag string based on the id of the strings
            var tagStrings = from tag in db.MovieTags
                             from tag_id in tagsForMovie
                             where tag.TagId == tag_id
                             select tag.Name;

            var movieTags = tagStrings.ToList();

            foreach (string tag in movieTags)
            {
                //count the number of times a tag id shows up in the UserToMovieToTags table
               var tagCount = from tagString in db.MovieTags
                               where tagString.Name == tag
                               from MtT in db.UserToMovieToTags
                               where MtT.TagId == tagString.TagId
                               select MtT.TagId;

                //make sure we havent used the tag string as a key already as we only need each tag + count 1 time
                if (!tagView.TagAndCount.ContainsKey(tag))
                {
                    tagView.TagAndCount.Add(tag, tagCount.Count());
                }
            }

            return tagView; 
        }
    }
}