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

        //retrieve api data
        public static string GetOmbdbTitleInfo(string title,
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
        public static XmlDocument GetXmlDocumentFromOmdbResponse(string xml_string)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml_string);

            return xmlDocument;

        }

        //serve create object from xmlDoc
        public static OmdbEntry CreateOmdbEntryFromXmlDocument(
            XmlDocument xmlDocument)
        {
            return new OmdbEntry();
        }
    }

    public class OmdbEntry
    {
        public string title;
        public string year;

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
