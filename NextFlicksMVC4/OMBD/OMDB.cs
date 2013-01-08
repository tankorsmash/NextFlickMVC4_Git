using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LumenWorks.Framework.IO.Csv;

namespace NextFlicksMVC4.OMBD
{
    public static class Omdb
    {

        //return all below
        public static OmdbEntry GetOmdbEntryForMovie(string title,
                                               string year = null,
                                               string response_type = "xml",
                                               string tomatoes = "true")
        {
            Trace.WriteLine("creating Omdb entry");
            var xml = GetOmbdbTitleInfoFromApi(title, year, response_type, tomatoes);
            var xDoc = GetXmlDocumentFromOmdbResponse(xml);
            OmdbEntry omdbEntry = CreateOmdbEntryFromXmlDocument(xDoc);

            Trace.WriteLine(" done creating Omdb entry");

            return omdbEntry;
        }

        //retrieve api data
        private static string GetOmbdbTitleInfoFromApi(string title,
                                               string year = null,
                                               string response_type = "xml",
                                               string tomatoes = "true")
        {
            //build url
            string base_url = @"http://www.omdbapi.com/";
            string url_params = string.Format(
                "?t={0}&y={1}&tomatoes={2}&r={3}",
                NetFlixAPI.OAuth1a.UpperCaseUrlEncode(title),
                year, tomatoes,
                response_type);
            string full_url = string.Format("{0}{1}", base_url, url_params);

            //Trace.WriteLine(full_url);
            System.Threading.Thread.Sleep(1000);
            Trace.WriteLine("  GetResponse'ing");
            HttpWebRequest web = (HttpWebRequest) WebRequest.Create(full_url);
            web.KeepAlive = true;
            web.UserAgent = "Quickflicks API by Tankor Smash";
            //set timeout to half a second
            //web.Timeout = 3000;
            var response = web.GetResponse();

            Stream objStream;
            objStream = response.GetResponseStream();
            Trace.WriteLine("  GetResponseStream'ing");

            StreamReader objReader;
            objReader = new StreamReader(objStream);

            Trace.WriteLine("  ReadToEnd'ing");
            string string_response = objReader.ReadToEnd();


            //Trace.WriteLine(string_response);

            return string_response;

        }
        

        //convert api string to xml
        private static XmlDocument GetXmlDocumentFromOmdbResponse(string xml_string)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml_string);

            return xmlDocument;

        }

        /// <summary>
        /// Creates a OmdbEntry either from a imdb or tomatoes TSV.
        ///  Send either one or both reader, and a premade entry
        ///  if you want to fill it out
        /// </summary>
        /// <param name="imdbReader"></param>
        /// <param name="tomReader"></param>
        /// <param name="premadeEntry"></param>
        /// <returns></returns>
        public static OmdbEntry CreateOmdbEntryFromTsvRecord(
            CsvReader imdbReader = null, CsvReader tomReader = null, OmdbEntry premadeEntry = null)
        {

            //if there's a imdbReader, use its contents to fill the first half of a OE
            OmdbEntry omdbEntry;
            if (imdbReader != null) {
                omdbEntry = new OmdbEntry
                                {
                                    ombd_ID = Convert.ToInt32( imdbReader["ID"]),

                                    title = imdbReader["title"],
                                    year = imdbReader["year"],

                                    i_Votes = imdbReader["imdbVotes"],
                                    i_Rating = imdbReader["imdbRating"],
                                    i_ID = imdbReader["imdbID"],
                                };
            }

            //if there's a premade entry, we want to fill that with tom data, or create a new one to fill
            else {
                if (premadeEntry != null) {
                    omdbEntry = premadeEntry;
                }
                else {
                    omdbEntry = new OmdbEntry();
                }
            }
            //so long as there's a tom reader, fill the OEntry with relevant data
            if (tomReader != null)
            {
                omdbEntry.ombd_ID = Convert.ToInt32(tomReader["ID"]);
                omdbEntry.t_Image = tomReader["Image"];
                omdbEntry.t_Rating = tomReader["Rating"];
                omdbEntry.t_Meter = tomReader["Meter"];
                omdbEntry.t_Reviews = tomReader["Reviews"];
                omdbEntry.t_Fresh = tomReader["Fresh"];
                omdbEntry.t_Rotten = tomReader["Rotten"];
                omdbEntry.t_Consensus = tomReader["Consensus"];
                omdbEntry.t_UserMeter = tomReader["userMeter"];
                omdbEntry.t_UserRating = tomReader["userRating"];
                omdbEntry.t_UserReviews = tomReader["userReviews"];
            }

            return omdbEntry;
        }

        //serve create object from xmlDoc
        private static OmdbEntry CreateOmdbEntryFromXmlDocument(
            XmlDocument xmlDocument)
        {

            if (xmlDocument.SelectSingleNode(@"/root/@response").InnerText == "False") {
                return new OmdbEntry();
            }

            OmdbEntry omdbEntry;
            omdbEntry = new OmdbEntry
                            {
                                title =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@title")
                                               .InnerText,
                                year =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@year").InnerText,
                                i_Votes = 
                                    xmlDocument.SelectSingleNode(
                                    "/root/movie/@imdbVotes").InnerText,
                                i_Rating = 
                                    xmlDocument.SelectSingleNode(
                                    "/root/movie/@imdbRating").InnerText,
                                i_ID = 
                                    xmlDocument.SelectSingleNode(
                                    "/root/movie/@imdbID").InnerText,
                                t_Meter =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoMeter")
                                               .InnerText,
                                t_Image =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoImage")
                                               .InnerText,
                                t_Rating =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoRating")
                                               .InnerText,
                                t_Reviews =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoReviews")
                                               .InnerText,
                                t_Fresh =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoFresh")
                                               .InnerText,
                                t_Rotten =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoRotten")
                                               .InnerText,
                                t_Consensus =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoConsensus")
                                               .InnerText,
                                t_UserMeter =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoUserMeter")
                                               .InnerText,
                                t_UserRating =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoUserRating")
                                               .InnerText,
                                t_UserReviews =
                                    xmlDocument.SelectSingleNode(
                                        "/root/movie/@tomatoUserReviews")
                                               .InnerText,
                            };

            return omdbEntry;
        }
    }

    public class OmdbEntry
    {
        [Key]
        public int ombd_ID { get; set; }

        [DisplayName("Movie Title")]
        public string title { get; set; }
        [DisplayName("Release Year")]
        public string year { get; set; }

        [DisplayName("IMDB Rating")]
        public string i_Rating { get; set; }
        [DisplayName("IMDB Votes")]
        public string i_Votes { get; set; }
        [DisplayName("IMDB ID")]
        public string i_ID { get; set; }

        [DisplayName("Rotten Tomatoes Meter")]
        public string t_Meter { get; set; }
        [DisplayName("Rotten Tomatoes Image")]
        public string t_Image { get; set; }
        [DisplayName("Rotten Tomatoes Rating")]
        public string t_Rating { get; set; }
        [DisplayName("Rotten Tomatoes Reviews")]
        public string t_Reviews { get; set; }
        [DisplayName("Rotten Tomatoes Fresh")]
        public string t_Fresh { get; set; }
        [DisplayName("Rotten Tomatoes Rotten")]
        public string t_Rotten { get; set; }
        [DisplayName("Rotten Tomatoes Consensus")]
        public string t_Consensus { get; set; }
        [DisplayName("Rotten Tomatoes UserMeter")]
        public string t_UserMeter { get; set; }
        [DisplayName("Rotten Tomatoes UserRating")]
        public string t_UserRating { get; set; }
        [DisplayName("Rotten Tomatoes UserReviews")]
        public string t_UserReviews { get; set; }
    }
}
