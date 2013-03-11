using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using NextFlicksMVC4.Models;
using NextFlicksMVC4.NetFlixAPI;

namespace NextFlicksMVC4
{
    public class NetflixCatalog
    {
        // <summary>
        /// Joins lines that don't start with a given string, default "<catalog"
        /// </summary>
        /// <param name="filepath"></param>
        ///<param name="start_string">string to match on the start of the line</param>
        /// <param name="skip_default_xml">whether or not to skip the fist two lines and the last one</param>
        public static void JoinLines(string filepath, string start_string = "<catalog", bool skip_default_xml = true)
        {
            Tools.TraceLine("In JoinLines");
            //keep track of how many malformed lines I have found in a row, 1 = record the line to join the next line to
            // 2 lines found means join lines, write file and recurse through again.
            bool found_broken_lines = false;
            
            Regex startsWith = new Regex(@"^<catalog_title");
            Regex startRoot = new Regex(@"^<catalog_titles>");
            Regex endRoot = new Regex(@"^</catalog_titles>");
            Regex endsWith = new Regex(@"title>$");
            Regex startsWithXML = new Regex(@"^<\?xml");

            //keep a list of new lines that have been meregs. I think i can just cram em in at position 2 over and over again, not 
            //sure the overall order of titles in the xml matters.
            string combinedLines = "";

            string lastline = ""; //for keepign track of the last line in case we need to merge the two
            List<String> malformedLines = new List<string>(); //keep track of all malformed lines in a row and then add them together then write the line
            string fixedLine = "";
            string line = "";
            using (StreamReader reader = new StreamReader(filepath))
            {
                using (StreamWriter writer = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/temp.NFPOX")))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        var trimmedLine = line.Trim();

                        Match matchStart = startsWith.Match(trimmedLine);
                        Match matchXML = startsWithXML.Match(trimmedLine);
                        Match matchEnd = endsWith.Match(trimmedLine);
                        Match matchRootStart = startRoot.Match(trimmedLine);
                        Match matchRootEnd = endRoot.Match(trimmedLine);

                        if ((matchXML.Success || matchRootStart.Success || matchRootEnd.Success) ||
                            (matchStart.Success && matchEnd.Success))
                        {
                            if (malformedLines.Count > 0)
                            {
                                var fixedXml = String.Join(String.Empty, malformedLines.ToArray());
                                writer.WriteLine(fixedXml);
                                malformedLines.Clear();
                            }

                            writer.WriteLine(trimmedLine);
                            lastline = trimmedLine;
                        }

                        else 
                        {
                            found_broken_lines = true;
                            malformedLines.Add(trimmedLine);
                        }
                    }
                }
            }
            //change the files around, overwrite fixedAPI.nfpox so that we can go through the same process again if neccesary
            //if not it will jsut write temp to fixedapi so we are good to go next step.
            var fixedAPI = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/fixedAPI.NFPOX");
            var temp = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/temp.NFPOX");
            var backup = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/backup.NFPOX");
            if (File.Exists(fixedAPI) && File.Exists(temp))
            {
                File.Replace(temp, fixedAPI, backup);
                File.Delete(temp);

            }
            if (found_broken_lines)
            {
                JoinLines(fixedAPI);
            }
            Tools.TraceLine("Out JoinLines");
        }

        /// <summary>
        /// Parse the Netflix Catalog (fixedAPI.nfpox) to get a list of all current genres
        /// </summary>
        /// <param name="filepath"></param>
        public static void UpdateGenreList(string filepath)
        {
            Tools.WriteTimeStamp("start update genres");
            Dictionary<int, string> genres = new Dictionary<int, string>();
            using (XmlReader xmlReader = XmlReader.Create(filepath))
            {
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "category"))
                    {

                        string possible_genre_url =
                            xmlReader.GetAttribute("scheme");

                        if (possible_genre_url.Contains("genres"))
                        {
                            //gotta split url for the id
                            int genre_id =
                                Convert.ToInt32(possible_genre_url.Split('/').Last());
                            string genre_string = xmlReader.GetAttribute("label");
                            //so long as the group isn't in the list, add it it
                            if (!genres.ContainsKey(genre_id))
                            {
                                genres.Add(genre_id, genre_string);
                            }

                        }
                    }
                }
            }
            var genresPath = System.Web.HttpContext.Current.Server.MapPath("~/dbfiles/genres.NFPOX");
            using (StreamWriter writer = new StreamWriter(genresPath, append: false))
            {
                foreach (KeyValuePair<int, string> kvp in genres)
                {
                    writer.WriteLine(kvp.Key + " " + kvp.Value);
                }
            }
            Tools.WriteTimeStamp("end update genres");

        }

        public static void BuildMoviesBoxartGenresTables(string filepath)
        {
            /*
             * 1: get a list of hashes form all movies in db to check for duplicates
             * 2: open file to read line by line
             * 3: check each line and build a movie out of it
             * 4: check to see if new movie hash is in list of all hashes from DB( if so discard and move on, else add it to the db)
             * 5: keep track of movies added so we can then move intoa new loop and tie it to box art and genre data
             *     5a: possibly just keep trakc of the movie IDs instead of the entire object to pull the movie back up in the next loop
             * 
             * 
             * 
             */
            Tools.TraceLine("In BuildMoviesBoxartGenresTables");
            Tools.TraceLine("  starting Full Action");

            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;

            var start_time = Tools.WriteTimeStamp("starting data read");
            //get alist of hashes for all movies in the db so we can make sure not to add a duplicate;
            List<int> dbHashes = new List<int>();
            //keep a list of all movie ids so we can reference them later from the db in another loop and build the genres and box arts later
            List<int> newMovies = new List<int>();

            foreach (Movie dbMovie in db.Movies)
            {
                dbHashes.Add(dbMovie.GetHashCode());
            }

            // Go line by line, and parse it for Movie files write movies when we hit 1000 or so.
            int count = 0;
            using (StreamReader reader = new StreamReader(filepath))
            {
                Tools.TraceLine("  Starting to read");

                string line = reader.ReadLine();
                line = line.Trim();
                //try {
                while (line != null)
                {
                    if (!line.StartsWith("<catalog_title>"))
                    {
                        Tools.TraceLine("  Invalid line of XML, probably CDATA or something, make sure all the lines start with '<cata' \n{0}", line);
                    }
                    else
                    {
                        //parse line for a title, which is what NF returns
                        List<Title> titles =
                            Create.ParseXmlForCatalogTitles(line);

                        //if there was a title to parse add it to the db
                        if (titles != null)
                        {
                            Movie movie =
                                Create.CreateMovie(titles[0]);

                            if (!dbHashes.Contains(movie.GetHashCode()))
                            {
                                //add to DB and dict
                                count++;
                                db.Movies.Add(movie);
                                db.SaveChanges(); //save changes so we can get the movie ID, I hope this doesnt slow me down too bad!
                                //create and add boxarts to db
                                var boxArt = CreateMovieBoxart(movie.movie_ID, titles[0]);
                                db.BoxArts.Add(boxArt);

                                //create and save movies to genres
                                AddGenres(movie.movie_ID, titles[0]);
                            }
                        }
                        else
                        {
                            Tools.TraceLine("  Failed on line {0}\n{1}", count, line);
                        }

                    }
                    if (count % 1000 == 0)
                    {
                        db.SaveChanges();
                    }
                    line = reader.ReadLine();
                }
            }

            //any unsaved changes save them
            db.SaveChanges();

            Tools.TraceLine(" Done Saving! Check out Movies/index for a table of the stuff");
            
            var end_time = Tools.WriteTimeStamp("  Done everything");

            TimeSpan span = end_time - start_time;
            Tools. TraceLine("  It took this long:");
            Tools.TraceLine(span.ToString());

            Tools.TraceLine("Out BuildMoviesBoxartGenresTables");
        }


       
        public static void AddGenres(int movieID, Title title)
        {
            MovieDbContext db = new MovieDbContext();
            db.Configuration.AutoDetectChangesEnabled = false;
            //genres to database
            foreach (Genre genre in title.ListGenres)
            {
                MovieToGenre movieToGenre = CreateMovieMovieToGenre(movieID, genre);
                db.MovieToGenres.Add(movieToGenre);
            }
            db.SaveChanges();
        }

        public static MovieToGenre CreateMovieMovieToGenre(int movieID, Genre genre)
        {
            //find the Genre equivalent of genre
            MovieToGenre movieToGenre = new MovieToGenre
            {
                genre_ID = genre.genre_ID,
                movie_ID = movieID
            };
            return movieToGenre;
        }

        public static BoxArt CreateMovieBoxart(int movieID, Title title)
        {
            BoxArt boxArt = new BoxArt
            {
                movie_ID = movieID,
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
    }
}