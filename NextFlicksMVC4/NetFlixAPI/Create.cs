using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.Tracking;
using System.Web;

namespace NextFlicksMVC4.NetFlixAPI
{
    //This class will be passed a catalogtitle tag group(sp?) and then pull out
    //the data and then create a movie instance.
    // 
    //I don't actually know the best way to parse it yet though, but who knows
    public static class Create
    {

        public  static  Regex _regexForBoxArtSize = new Regex(@"(^\d*)pix", RegexOptions.Compiled);

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

            //create movie instance
            Movie movie = new Movie
                              {
                                  short_title = title.TitleString,
                                  year = title.ReleaseYear,
                                  runtime =  title.RuntimeInSeconds,
                                  avg_rating = title.AvgRating,
                                  tv_rating = title.TvRating,
                                  web_page = title.LinkToPage,
                                  current_season = title.WhichSeason,
                                  is_movie = is_a_movie,
                                  maturity_rating = title.MaturityLevel,

                              };
            //CreateMovieBoxartFromTitle(movie, title);

            return movie;

            
        }

        

        //creates a row of box art data and assigns to to the movie id its title was from
        public static BoxArt CreateMovieBoxartFromTitle(Movie movie, Title title)
        {
            BoxArt boxArt = new BoxArt
                                {

                                    movie_ID = movie.movie_ID,
                                    boxart_38 = title.BoxArt38,
                                    boxart_64 = title.BoxArt64,
                                    boxart_110 = title.BoxArt110,
                                    boxart_124 = title.BoxArt124,
                                    boxart_150 = title.BoxArt150,
                                    boxart_166 = title.BoxArt166,
                                    boxart_88 = title.BoxArt88,
                                    boxart_197 = title.BoxArt197,
                                    boxart_176 = title.BoxArt176,
                                    boxart_284 = title.BoxArt284,
                                    boxart_210 = title.BoxArt210

                                };

            return boxArt;

        }

        /// <summary>
        /// iterate over all the genres in Title and add them to the movie ID
        /// </summary>
        /// <param name="movie"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static MovieToGenre CreateMovieMovieToGenre(Movie movie, Genre genre)
        {

            //find the Genre equivalent of genre



            MovieToGenre movieToGenre = new MovieToGenre
                                            {
                                                genre_ID = genre.genre_ID,
                                                movie_ID = movie.movie_ID
                                            };
            return movieToGenre;
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
            AddImageData(createdTitle, catalog_title);
            
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
            createdTitle.ReleaseYear = Convert.ToInt32(year);

            //find the runtime
            var runtime_node = catalog_title.SelectSingleNode("link/delivery_formats/availability/runtime");
            if (runtime_node != null)
            {
                var runtime = runtime_node.InnerText;
                try {
                    createdTitle.RuntimeInSeconds = Convert.ToInt32(runtime);
                }
                catch (Exception ex) {
                    createdTitle.RuntimeInSeconds = 0;
                    Tools.TraceLine("error, could not convert runtime from string {0}", ex);
                }
                //var msg = String.Format("\tRuntime found {0}", runtime);
                //Trace.WriteLine(msg);
            }

            //TODO Genres, need to figure out best way to sort multiple vals
            //find the genres
            //since it's so similar I've set the maturity level here instead of in the rating method
            string path = @"/catalog_title/category[@scheme]";
            var nodes = catalog_title.SelectNodes(path);

            List<String> genre_list = new List<string>();
            foreach (XmlNode node in nodes)
            {
                string label = node.Attributes["label"].Value;
                genre_list.Add(label);
            }

            //since the last one is always maturity level, use that as mat_level
            // but make sure it's a number first, otherwise it's a genre and there's no mat level
            string maturity_level = genre_list[genre_list.Count-1];
            int mat_level;
            if (int.TryParse(maturity_level, out mat_level))
            {
                //if maturity_level is a number, assign it to Title and remove it
                //waste mat_level
                genre_list.Remove(maturity_level);
                try {
                    createdTitle.MaturityLevel = Convert.ToInt32(maturity_level);
                }
                catch (Exception ex) {
                    createdTitle.MaturityLevel = 200;
                    Tools.TraceLine("Could not convert maturity string to int {0}", ex);
                }
            }

            //now we join the genre_list with commas and assign it to Title
            string genres = string.Join(", ", genre_list);
            createdTitle.Genres = genres;


            //create all the Genres found too
            //var db = NextFlicksMVC4.Controllers.MoviesController.db;
            var db = new MovieDbContext();
            foreach (string genre_string in genre_list)
            {
                //look in Genres Table for genre_String that matches and pull that out and add it to ListGenres
                string qry = "select * from Genres where genre_string = {0}";
                var res =db.Genres.SqlQuery(qry, genre_string);
                var selected_genre = res.ToList()[0];
                createdTitle.ListGenres.Add(selected_genre);

            }
            db.Dispose();


            //Trace.WriteLine("added Primary Data to title");

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

            //saving space, strip "http://www.netflix.com/" from web page
            const string link_template = @"http://www.netflix.com/";
            if (link.StartsWith(link_template))
            {
                link = link.Remove(0, link_template.Length);
            }

            createdTitle.LinkToPage = link;
        }

        /// <summary>
        /// TV rating and AvgNetflix rating, maturity rating
        /// </summary>
        /// <param name="createdTitle"></param>
        public static void AddRatingData(Title createdTitle, XmlNode catalog_title)
        {
            // TV or MPAA rating
            //can either be in category[scheme] ends with "ratings" either way. It's always first
            const string path = "link/delivery_formats/availability/category/category";
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

            //maturity rating is set in the same place Genres are set
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
            const string path = "season_label";
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

        public static void AddImageData(Title createdTitle, XmlNode catalog_title)
        {
            const string query = @"/catalog_title/link/box_art/link";
            XmlNodeList nodes = catalog_title.SelectNodes(query);
            if (nodes != null)
                foreach (XmlNode node in nodes)
                {
                    string href = node.Attributes["href"].Value;
                    string title = node.Attributes["title"].Value;

                    //find the first bit of the title
                    Match match = _regexForBoxArtSize.Match(title);

                    //this is the size of the jpg boxart. We just have to assign the proper link to the proper size
                    string size = match.Groups[1].Value;

                    //in the interest of saving time and space, I'm going to strip off most of the url data of the jpg locations, as I think it'll save us 20 out of the 90 megs of data
                    // after this, I might go after the genres, or move the synopses to another table.
                    //TODO: Use regex instead of StartsWith
                    const string url_template0 = @"http://cdn-0.nflximg.com/images/";
                    const string url_template1 = @"http://cdn-1.nflximg.com/images/";
                    if (href.StartsWith(url_template0))
                    {
                        //remove the beginning
                        href = href.Remove(0, url_template0.Length);
                        //remove the .jpg because they're all jpgs
                        href = href.Replace(".jpg", "");

                    }
                    else if (href.StartsWith(url_template1))
                    {
                        //remove the beginning
                        href = href.Remove(0, url_template1.Length);
                        //remove the .jpg because they're all jpgs
                        href = href.Replace(".jpg", "");

                    }

                    //add the string to the list<string>
                    createdTitle.BoxArtList.Add(href);

                    switch (size)
                    {
                        case "38":
                                createdTitle.BoxArt38 = href;
                                break;
                        case "64":
                                createdTitle.BoxArt64 = href;
                                break;
                        case "110":
                                createdTitle.BoxArt110 = href;
                                break;
                        case "124":
                                createdTitle.BoxArt124 = href;
                                break;
                        case "150":
                                createdTitle.BoxArt150 = href;
                                break;
                        case "166":
                                createdTitle.BoxArt166 = href;
                                break;
                        case "88":
                                createdTitle.BoxArt88 = href;
                                break;
                        case "197":
                                createdTitle.BoxArt197 = href;
                                break;
                        case "176":
                                createdTitle.BoxArt176 = href;
                                break;
                        case "284":
                                createdTitle.BoxArt284 = href;
                                break;
                        case "210":
                                createdTitle.BoxArt210 = href;
                                break;


                    }
                }
        }
    }
}
