using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.OMBD;
using NextFlicksMVC4.Views.Movies.ViewModels;

namespace NextFlicksMVC4.Tests
{
    [TestFixture]
    public class TestTools
    {
        [Test]
        public void TestIEnumToSelectListItem()
        {
            List<SelectListItem> actual =
                Tools.IEnumToSelectListItem(new int[] {1, 2, 3});

            List<SelectListItem> expected = new List<SelectListItem>();
            expected.Add(new SelectListItem {Text = "1", Value = "1"});
            expected.Add(new SelectListItem {Text = "2", Value = "2"});
            expected.Add(new SelectListItem {Text = "3", Value = "3"});

            //This is bad, but unless I implement custom logic just for testing, this'll have to do
            //    Could make it a forloop I guess...
            Assert.AreEqual(expected[0].Text,actual[0].Text);
            Assert.AreEqual(expected[0].Value,actual[0].Value);

            Assert.AreEqual(expected[1].Text,actual[1].Text);
            Assert.AreEqual(expected[1].Value,actual[1].Value);

            Assert.AreEqual(expected[2].Text,actual[2].Text);
            Assert.AreEqual(expected[2].Value,actual[2].Value);
        }
        
        [Test]
        public void TestFilterByYear()
        {
            int correctYear = 1999;
            Constraint is_correct_year = Is.EqualTo(correctYear);

            var res =
                Tools.FilterByYear(new MovieDbContext(), correctYear.ToString())
                     .Take(100);

            //TODO: confirm that mass Asserting like that is supposed to happen. Seems overkill
            foreach (FullViewModel fullViewModel in res) {
                Assert.ByVal(fullViewModel.Movie.year, is_correct_year);
            }


        }
    }
}
