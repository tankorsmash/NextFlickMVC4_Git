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

        private Dictionary<Users, int> TaggedByUsers(int tagID)
        {
            MovieDbContext db = new MovieDbContext();
            Dictionary<Users, int> returnDict = new Dictionary<Users,int>();
            /*
            var tagToUser = from user in db.Users
                            from tag in db.UserToMovieToTags
                            where tag.TagId == tagID
                            group tag.TagId by user into grouping
                            select new
                            {
                                user = grouping.Key,
                                count = grouping.Count()
                            };
                            //select user;

            return tagToUser.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as int);
            */
            /* try somthing liek this
              var tags = from tags in db.UserToMoviesToTags
                         Where tag.tagID == tagID
                         select tag;
                         
              var users = from users in db.Users
                           where user.TagId == TagID
                           from tag in tags
                          select users;
             */

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
            /*var userRows = from userRow in db.UserToMovieToTags
                           where userRow.TagId == tagID
                           group userRow.TagId by userRow.UserID into grouping
                           select grouping;

            foreach (IGrouping<int, int> result in userRows)
            {
                var selectUser = from user in db.Users
                                 where user.userID == result.Key
                                 select user;
                returnDict.Add(selectUser.First() , result.Count());
            }*/
            return returnDict;
        }
    }
}
