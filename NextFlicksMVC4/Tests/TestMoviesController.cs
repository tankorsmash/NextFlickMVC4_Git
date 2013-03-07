using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using NextFlicksMVC4;
using NextFlicksMVC4.DatabaseClasses;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Models.userAccount;
using NextFlicksMVC4.NetFlixAPI;
using System.Timers;
using NextFlicksMVC4.Helpers;
using System.Data.SqlClient;
using NextFlicksMVC4.Filters;
using NextFlicksMVC4.OMBD;
using NextFlicksMVC4.Views.Movies.ViewModels;
using WebMatrix.WebData;
using NUnit.Framework;

namespace NextFlicksMVC4.Tests
{
    [TestFixture]
    public class TestMoviesController
    {
    
        [Test]
        public void AddXtoY()
        {
            int x = 1;
            int y = 100;
            int z = x + y;

            Assert.AreEqual(z, 101);
        }


        [Test]
        public void TestGetAllParamNames()
        {
            List<string> result = Tools.GetAllParamNames("FindPageOfMovies");
            List<string> expected = new List<string> {"res","page", "movie_count","db","verbose"};
            Assert.AreEqual(result, expected);

            List<string> result2 = Tools.GetAllParamNames("FindPageOfMovies");
            List<string> expected2 = new List<string>();
            Assert.AreNotEqual(result2, expected2);
        } 


    

        [Test(Description = "Testing testing testing")]
        public void TestCreateMovie()
        {
            Title title = new Title
                              {
                                  TitleString = "Test Title",
                                  ReleaseYear = 2009,
                                  RuntimeInSeconds = 3600,
                                  AvgRating = "3",
                                  WhichSeason = "0",
                                  MaturityLevel =200,
                                  TvRating = "not set",
                                  LinkToPage = "www.netflix.com/test",
                                  IsMovie = "true"
                              };

            Movie result = Create.CreateMovie(title);

            Movie expected = new Movie
                                 {
                                     short_title = "Test Title",
                                     year = 2009,
                                     runtime = 3600,
                                     avg_rating = "3",
                                     is_movie = true,
                                     current_season = "0",
                                     maturity_rating = 200,
                                     movie_ID = 0,
                                     tv_rating= "not set",
                                     web_page = "www.netflix.com/test",
                                 };


            Tools.TraceLine("result hash: {0}\nexpected hash: {1}", result.GetHashCode(), expected.GetHashCode());
            Assert.AreEqual(result, expected);

        }

        [Test]
        public void TestMoviesInMoviesTable()
        {
            //want to make sure the db connection string works and points to the proper table, but I don't know how
            
        }

        [Test]
        public void TestMatchMovieIdToOmdbEntry()
        {

            MovieDbContext db = new MovieDbContext();

            //Tools.TraceLine(db.Database.Connection.ConnectionString);

            int first_movie_id = db.Movies.First().movie_ID;
            OmdbEntry expected_omdbentry = db.Omdb.First(omdb => omdb.movie_ID == first_movie_id);

            OmdbEntry result = Tools.MatchMovieIdToOmdbEntry(first_movie_id);
            
            Assert.AreEqual(expected_omdbentry, result);



        }

        /// <summary>
        /// Test for confirm that Movie.Equals(same_Movie) works
        /// </summary>
        [Test]
        public void TestMovieEquals()
        {

            Movie first_movie = new Movie
                                    {
                                        year = 1999,
                                        maturity_rating = 200,
                                        runtime = 12345
                                    };

            Movie second_movie = new Movie
                                    {
                                        year = 1999,
                                        maturity_rating = 200,
                                        runtime = 12345
                                    };

            Assert.AreEqual(first_movie, second_movie);
            Assert.AreNotEqual(first_movie, new Movie());
            Assert.AreNotEqual(second_movie, new Movie());
        }

        /// <summary>
        /// Test for confirm that Omdb.Equals(same_Omdb) works
        /// </summary>
        [Test]
        public void TestOmdbEquals()
        {

            OmdbEntry first_movie = new OmdbEntry
                                    {
                                        //ignore because I'm not sure when a movie_id will or wont be set
                                        //movie_ID = ,
                                        title = "movie called",
                                        year = 1999,
                                        i_Rating = "asd",
                                        i_Votes = "400",
                                        i_ID = "asd",
                                        t_Meter = 2,
                                        t_Image = "asd",
                                        //ignore cause of float
                                        //t_Rating = 22,
                                        t_Reviews = 123,
                                        t_Fresh = 23,
                                        t_Rotten = 23,
                                        t_Consensus = "rotten",
                                        t_UserMeter = 233,
                                        //ignore cause of float
                                        //t_UserRating = 2.4f,
                                        t_UserReviews = 232,
                                    };

            OmdbEntry second_movie = new OmdbEntry
                                    {
                                        //ignore because I'm not sure when a movie_id will or wont be set
                                        //movie_ID = ,
                                        title = "movie called",
                                        year = 1999,
                                        i_Rating = "asd",
                                        i_Votes = "400",
                                        i_ID = "asd",
                                        t_Meter = 2,
                                        t_Image = "asd",
                                        //ignore cause of float
                                        //t_Rating = 22,
                                        t_Reviews = 123,
                                        t_Fresh = 23,
                                        t_Rotten = 23,
                                        t_Consensus = "rotten",
                                        t_UserMeter = 233,
                                        //ignore cause of float
                                        //t_UserRating = 2.4f,
                                        t_UserReviews = 232,
                                    };
                                    //{
                                    //    year = 1999,
                                    //    i_ID = "200",
                                    //    i_Votes = "12345"
                                    //};
            Assert.AreEqual(first_movie, second_movie);

            OmdbEntry bogus_movie = new OmdbEntry
                                        {
                                            t_Consensus = "asd",
                                        };

            Assert.AreNotEqual(first_movie, bogus_movie);
            Assert.AreNotEqual(second_movie, bogus_movie);
        }

        /// <summary>
        /// Tests Tools.StringToDouble
        /// </summary>
        [Test]
        public void TestStringToDouble()
        {

            //should work
            string first_string = "1.2";
            double first_double = Tools.StringToDouble(first_string);
            double first_result = 1.2;

            //should work
            string second_string = "0.0";
            double second_double = Tools.StringToDouble(second_string);
            double second_result = 0.0;

            //should not work, return 0.0
            string third_string = "as";
            double third_double = Tools.StringToDouble(third_string);
            double third_result = 0.0;

            Assert.AreEqual(first_double, first_result);
            Assert.AreEqual(second_double, second_result);
            Assert.AreEqual(third_double, third_result);

        }


        [Test]
        public void TestFullDbQuery()
        {
            MovieDbContext db = new MovieDbContext();
            IQueryable<FullViewModel> qry = Tools.GetFullDbQuery(db);

            Assert.IsNotNull(qry);

        }




    }
}
