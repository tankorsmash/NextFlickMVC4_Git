using System.Collections.Generic;

namespace NextFlicksMVC4.NetFlixAPI
{
    public class Title{

        //Primary Data
        public string TitleString = "not set";
        public string ReleaseYear = "not set";
        public string RuntimeInSeconds = "not set";
        public string Genres = "not set";
        //Secondary Data
        public string Synopsis = "not set";
        public string ShortSynopsis = "not set";
        public string Links = "not set";
        public string Cast = "not set";
        public string Director = "not set";
        //Rating Data
        public string TvRating = "not set";
        public string AvgRating = "not set";
        public string Awards = "not set";
        public string MaturityLevel = "not set";

        //Format Data
        public string IsMovie = "not set";
        public string WhichSeason = "not set";
        public string FormatAvailability = "not set";
        public string ScreenFormats = "not set";
        public string TitleFormats = "not set";
        public string Language = "not set";
        public string Audio = "not set";
        public string Mono = "not set";
        public string Stereo = "not set";
        public string Dolby = "not set";
        //More Data
        public string Discs = "not set";
        public string Episodes = "not set";
        public string SimilarTitles = "not set";
        public string LinkToPage = "not set";


        //Box Art Links
        public string BoxArt38;
        public string BoxArt64;
        public string BoxArt110;
        public string BoxArt124;
        public string BoxArt150;
        public string BoxArt166;
        public string BoxArt88;
        public string BoxArt197;
        public string BoxArt176;
        public string BoxArt284;
        public string BoxArt210;




        public Title()
        {
            //create class here
        }



    }
}
