using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.IO;

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
            var xml = GetOmbdbTitleInfo(title, year, response_type, tomatoes);
            var xDoc = GetXmlDocumentFromOmdbResponse(xml);
            OmdbEntry omdbEntry = CreateOmdbEntryFromXmlDocument(xDoc);


            return omdbEntry;
        }

        //retrieve api data
        private static string GetOmbdbTitleInfo(string title,
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

            HttpWebRequest web = (HttpWebRequest) WebRequest.Create(full_url);
            var response = web.GetResponse();

            Stream objStream;
            objStream = response.GetResponseStream();

            StreamReader objReader;
            objReader = new StreamReader(objStream);

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

        //serve create object from xmlDoc
        private static OmdbEntry CreateOmdbEntryFromXmlDocument(
            XmlDocument xmlDocument)
        {

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
        public string title;
        public string year;

        public string i_Rating;
        public string i_Votes;
        public string i_ID;

        public string t_Meter;
        public string t_Image;
        public string t_Rating;
        public string t_Reviews;
        public string t_Fresh;
        public string t_Rotten;
        public string t_Consensus;
        public string t_UserMeter;
        public string t_UserRating;
        public string t_UserReviews;
    }
}
