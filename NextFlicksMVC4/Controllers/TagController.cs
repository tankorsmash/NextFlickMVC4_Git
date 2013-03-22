using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.TagModels;
using NextFlicksMVC4.Models.userAccount;

namespace NextFlicksMVC4.Controllers
{
    public class TagController : Controller
    {
        //
        // GET: /Tags/

        public ActionResult Index()
        {
            var tagsAndCounts = TagsAndCounts();
            return View(tagsAndCounts);
        }

        public ActionResult Details(string tagName)
        {
            MovieDbContext db = new MovieDbContext();
            TagDetailViewModel tagDetail = new TagDetailViewModel() { TagName = tagName };
            
            //get the tagId off the name, there should only be one tag by the same name
            var tagID = from tag in db.MovieTags
                        where tag.Name == tagName
                        select tag.TagId;
            //thjis checks to see if it can return any result and if it cannot return 404
            if (!tagID.Any())
                return HttpNotFound();

            int t_ID = tagID.First();
            tagDetail.TaggedMovies = MoviesTagged(t_ID);
            tagDetail.TaggedByUsers = TaggedByUsers(t_ID);
            return View(tagDetail);
        }


        private List<Movie> MoviesTagged(int tagID)
        {
            MovieDbContext db = new MovieDbContext();

            var tagToMovieID = from movieId in db.UserToMovieToTags
                               where movieId.TagId == tagID
                               select movieId.movie_ID;

            var movieTitles = from movieTitle in db.Movies
                              from movieID in tagToMovieID
                              where movieTitle.movie_ID == movieID
                              select movieTitle.short_title;

            var movies = from movie in db.Movies
                         from movieID in tagToMovieID
                         where movie.movie_ID == movieID
                         select movie;

            return movies.Distinct().ToList();

        }

        private Dictionary<Users, int>  TaggedByUsers(int tagID)
        {
            MovieDbContext db = new MovieDbContext();
            Dictionary<Users, int> returnDict = new Dictionary<Users,int>();
           
            var tagRows = from rows in db.UserToMovieToTags
                          where rows.TagId == tagID
                          select rows;

            var userRows = from userRow in db.UserToMovieToTags
                           where userRow.TagId == tagID
                           from user in db.Users
                           where user.userID == userRow.UserID
                           group userRow.TagId by user into grouping
                           select grouping;
            foreach(IGrouping<Users,int> result in userRows)
            {
                returnDict.Add(result.Key, result.Count());
            }
           
            return returnDict;
        }

        private TagCountViewModel TagsAndCounts()
        {
            MovieDbContext db = new MovieDbContext();
            TagCountViewModel tagView = new TagCountViewModel() { TagAndCount = new Dictionary<string, int>() };

            //find the ids of the tags related to the movie
            //var tagsForMovie = from MtT in db.UserToMovieToTags
            //                   where MtT.movie_ID == movieID
            //                   select MtT.TagId;

            //find the tag string based on the id of the strings
            var tagStrings = from tag in db.MovieTags
                             //from tag_id in tagsForMovie
                             //where tag.TagId == tag_id
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
            /*
            var allTags = (from tagName in db.MovieTags
                          from tagID in db.UserToMovieToTags
                          where tagName.TagId == tagID.TagId
                          group tagID.TagId by tagName.Name into grouping
                          select grouping).ToDictionary(grp => grp.Key, grp => grp.Key.Count());

            return allTags; */

        }
    }
}
