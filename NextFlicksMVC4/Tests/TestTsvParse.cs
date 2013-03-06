using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using NextFlicksMVC4.OMBD;

namespace NextFlicksMVC4.Tests
{
    [TestFixture]
    public class TestTsvParse
    {

        [Test]
        public void TestUpdateImdbEntryWithRtEntry()
        {

            var imdb_param = new OmdbEntry();
            var rt_param = new OmdbEntry
                               {
                                   t_Consensus = "asd",
                                   t_Fresh = 123,
                                   t_Image = "asd",
                                   t_Meter = 123,
                                   t_Rating = 1.2f,
                                   t_Reviews = 123,
                                   t_Rotten = 123,
                                   t_UserMeter = 1234,
                                   t_UserRating = 12.4f,
                                   t_UserReviews = 12345
                               };

            TSVParse.UpdateImdbEntryWithRtEntry(imdb_param, rt_param);

            OmdbEntry expected = new OmdbEntry
                                     {
                                         t_Consensus = "asd",
                                         t_Fresh = 123,
                                         t_Image = "asd",
                                         t_Meter = 123,
                                         t_Rating = 1.2f,
                                         t_Reviews = 123,
                                         t_Rotten = 123,
                                         t_UserMeter = 1234,
                                         t_UserRating = 12.4f,
                                         t_UserReviews = 12345
                                     };

            Assert.AreEqual(imdb_param, expected);
            Assert.AreNotEqual(imdb_param, new OmdbEntry());
        }
    }
}
