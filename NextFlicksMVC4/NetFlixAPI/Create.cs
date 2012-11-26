using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;

namespace NextFlicksMVC4
{
    //This class will be passed a catalogtitle tag group(sp?) and then pull out
    //the data and then create a movie instance.
    // 
    //I don't actually know the best way to parse it yet though, but who knows
    public static class Create
    {

        //will turn a Title into a Movie which can be used for the site.
        public static Movie CreateMovie(Title title)
        {
            //I can actually see this conversion process actually being
            //difficult to keep up, if the data model changes but for now, it's
            //all we got.

            bool is_a_movie = true;
            //test for isMovie
            if (title.WhichSeason == "0")
            {
                is_a_movie = true;
            }
            else
            {
                is_a_movie = false;
            }


           
            //create empty instance
            Movie movie = new Movie
                              {
                                  short_title = title.TitleString,
                                  year = title.ReleaseYear,
                                  //runtime = title.RuntimeInSeconds,
                                  runtime =  DisplayTools.SecondsToTime(title.RuntimeInSeconds),
                                  avg_rating = title.AvgRating,
                                  tv_rating = title.TvRating,
                                  web_page = title.LinkToPage,
                                  current_season = title.WhichSeason,

                                  is_movie = is_a_movie

                                  
                              };

            //create first set of data that is available to us.

            return movie;

            
        }

        /// <summary>
        /// Takes raw XML from API and returns a List of Titles
        /// </summary>
        /// <param name="xml_from_request"></param>
        /// <returns>A List of Titles</returns>
        public static List<Title> ParseXmlForCatalogTitles(string xml_from_request)
        {
            //Parse the plain old XML to pass to each method
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xml_from_request);
            XmlNodeList titles = xDoc.GetElementsByTagName("catalog_title");

            List<Title> titleList = new List<Title>();

            //For each title in the XML group, send to a file
            foreach (XmlNode title in titles)
            {
                var created_title = CreateTitle(title);
                titleList.Add(created_title);

            }

            return titleList;

        }


        ///a title is what the nextlix api returns, A movie is what the site
        ///uses as a model
        public static Title CreateTitle(XmlNode catalog_title)
        {
            //create an empty Title class
            Title createdTitle = new Title();



            //Add data to the appropriate fields
            AddPrimaryData(createdTitle, catalog_title);
            AddSecondaryData(createdTitle, catalog_title);
            AddRatingData(createdTitle, catalog_title);
            AddFormatData(createdTitle, catalog_title);
            AddGroupData(createdTitle, catalog_title);
            

            return createdTitle;

        }        


        /// <summary>
        /// Adds Title, Art, Year, Runtime, Genres
        /// </summary>
        /// <param name="createdTitle"></param>
        public static void AddPrimaryData(Title createdTitle, XmlNode catalog_title)
        {
            //find the title node, use the short title
            var title_node = catalog_title.SelectNodes("title")[0];
            var short_title = title_node.Attributes["short"].Value;
            var regular_title = title_node.Attributes["regular"].Value;
            createdTitle.TitleString = short_title;

            //TODO: Box Art

            //find the year released
            var release_node = catalog_title.SelectSingleNode("release_year");
            var year = release_node.InnerText;
            createdTitle.ReleaseYear = year;

            //find the runtime
            var runtime_node = catalog_title.SelectSingleNode("link/delivery_formats/availability/runtime");
            if (runtime_node != null)
            {
                var runtime = runtime_node.InnerText;
                createdTitle.RuntimeInSeconds = runtime;
                var msg = String.Format("\tRuntime found {0}", runtime);
                Trace.WriteLine(msg);
            }

            //TODO Genres, need to figure out best way to sort multiple vals

            Trace.WriteLine("added Primary Data to title");
        }

        /// <summary>
        /// Adds Synopsis, Link, Cast
        /// </summary>
        /// <param name="createdTitle"></param>
        public static void AddSecondaryData(Title createdTitle, XmlNode catalog_title)
        {
            //TODO: Synposis, Link, Cast
            var node = catalog_title.SelectSingleNode("link[@title='web page']");
            string link = node.Attributes["href"].Value;
            createdTitle.LinkToPage = link;
        }

        /// <summary>
        /// TV rating and AvgNetflix rating
        /// </summary>
        /// <param name="createdTitle"></param>
        public static void AddRatingData(Title createdTitle, XmlNode catalog_title)
        {
            // TV or MPAA rating
            //can either be in category[scheme] ends with "ratings" either way. It's always first
            string path = "link/delivery_formats/availability/category/category";
            var rating_node = catalog_title.SelectSingleNode(path);
            if (rating_node != null)
            {
                var rating = rating_node.Attributes["term"].Value;
                createdTitle.TvRating = rating;
            }

            var average_rating_node =
                catalog_title.SelectSingleNode("average_rating");
            if (average_rating_node != null)
            {
                var avg_rating = average_rating_node.InnerText;
                createdTitle.AvgRating = avg_rating;
            }
        }

        /// <summary>
        /// Format availabilty, screen_format, language and audio
        /// </summary>
        /// <param name="createdTitle"></param>
        /// <param name="catalog_title"></param>
        public static void AddFormatData(Title createdTitle, XmlNode catalog_title)
        {
            //detect whether or not it's a TV show or a Movie.
            // if WhichSeason is 0, it's not a series
            string path = "season_label";
            var season_label_node = catalog_title.SelectSingleNode(path);
            if (season_label_node != null)
            {
                var season_count = season_label_node.InnerText;
                createdTitle.WhichSeason = season_count;

            }
            else
            {
                createdTitle.WhichSeason = "0";
            }

        }

        /// <summary>
        /// Discs, Episodes, Similar Titles, Link to Overall page?
        /// </summary>
        /// <param name="createdTitle"></param>
        public static void AddGroupData(Title createdTitle, XmlNode catalog_title)
        {

        }
    }
}
