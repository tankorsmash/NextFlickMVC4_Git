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
