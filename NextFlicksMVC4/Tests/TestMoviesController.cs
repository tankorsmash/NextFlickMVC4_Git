using System;
using System.Collections.Generic;
using NUnit.Framework;
using NextFlicksMVC4.Controllers;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;


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

        [Test]
        public void Testasd()
        {
            
        }


    }
}