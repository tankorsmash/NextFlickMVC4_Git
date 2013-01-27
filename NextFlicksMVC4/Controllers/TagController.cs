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

        public ActionResult Index(string tagName)
        {
            MovieDbContext db = new MovieDbContext();
            TagDetailViewModel tagDetail = new TagDetailViewModel() { TagName = tagName };
            
            //get the tagId off the name, there should only be one tag by the same name
            var tagID = from tag in db.MovieTags
                        where tag.Name == tagName
                        select tag.TagId;
            if (tagID == null)
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

            return movies.ToList();

        }

        private Dictionary<Users, int> TaggedByUsers(int tagID)
        {
            MovieDbContext db = new MovieDbContext();
            Dictionary<Users, int> returnDict = new Dictionary<Users,int>();

            var tagToUser = from user in db.Users
                            from tag in db.UserToMovieToTags
                            where tag.TagId == tagID
                            select user;
           // foreach (Users user in tagToUser)
            ///{
                //count the number of times a tag id shows up in the UserToMovieToTags table
            var TagCount = from tag in db.UserToMovieToTags
                           where tag.TagId == tagID
                           from userID in tagToUser
                           where userID.userID == tag.UserID
                           select tag.TagId;
            var count =  TagCount.Count();
                var userCount = from tag in db.UserToMovieToTags
                                where tag.TagId == tagID
                                from userID in tagToUser
                                where userID.userID == tag.UserID
                                //select userID.Username; 
                               group tag.TagId by userID into grouping
                               select grouping;
                     
            //}
                foreach (var item in userCount)
                {
                    //var tempDict = item.ToDictionary();
                }
                
            return null;
        }
    }
}
