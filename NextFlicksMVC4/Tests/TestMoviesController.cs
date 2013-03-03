﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using NextFlicksMVC4.Controllers;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;
using NextFlicksMVC4.OMBD;


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
            List<string> result = Tools.GetAllParamNames("GetAllParamNames");
            List<string> expected = new List<string> {"methodName"};
  
            Assert.AreEqual(result, expected);
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




    }
}
