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
        public void TestMergeTwoOmdbEntryLists()
        {
            //create a imdb OE
            OmdbEntry imdb_omdbentry = new OmdbEntry
                                           {
                                               i_ID = "asd",
                                               i_Rating = "asd",
                                               i_Votes = "123"
                                           };
            List<OmdbEntry> imdb_list = new List<OmdbEntry> {imdb_omdbentry};

            //create a RT OE
            OmdbEntry rt_omdbentry = new OmdbEntry
                                         {
                                             t_Consensus = "rotten",
                                             t_Fresh = 123,
                                             t_Image = "asddd",
                                             t_Meter = 123,
                                             t_Reviews = 12355,
                                             t_Rotten = 333,
                                             t_UserMeter = 233,
                                             t_UserReviews = 3
                                         };
            List<OmdbEntry> rt_list = new List<OmdbEntry> {rt_omdbentry};

            //expected OE result
            OmdbEntry expected_omdbentry = new OmdbEntry
                                               {

                                                   i_ID = "asd",
                                                   i_Rating = "asd",
                                                   i_Votes = "123",
                                                   t_Consensus = "rotten",
                                                   t_Fresh = 123,
                                                   t_Image = "asddd",
                                                   t_Meter = 123,
                                                   t_Reviews = 12355,
                                                   t_Rotten = 333,
                                                   t_UserMeter = 233,
                                                   t_UserReviews = 3
                                               };

            List<OmdbEntry> res = TSVParse.MergeTwoOmdbEntryLists(imdb_list,
                                                                  rt_list);
            OmdbEntry resulting_omdbentry = res[0];
            Assert.AreEqual(expected_omdbentry, resulting_omdbentry);
            Assert.AreNotEqual(new OmdbEntry(), resulting_omdbentry);

        }

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
