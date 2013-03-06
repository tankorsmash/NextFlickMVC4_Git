using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using LumenWorks.Framework.IO.Csv;
using System.Diagnostics;
using System.Net;
using NextFlicksMVC4.Controllers;
using NextFlicksMVC4.Models;

namespace NextFlicksMVC4.OMBD
{
    public static class TSVParse
    {

        public static void DownloadOmdbZip(string url = @"http://www.beforethecode.net/projects/OMDb/download.aspx?e=tankorsmash@gmail.com", string outputPath = @"omdb.zip")
        {
            Tools.TraceLine("start download:\n{0}", url);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, outputPath);
            }
            //doesn't work
            //Tools.TraceLine("start download:\n{0}", url);
            //using (WebClient client = new WebClient()) {
                
            //    client.DownloadFile(url, outputPath);
            //}
            //Tools.TraceLine("done download");

            /* tankorsmash code below
            Uri uri = new Uri(url);

            //download the omdbzip
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(uri);
            web.KeepAlive = true;
            web.AutomaticDecompression = DecompressionMethods.GZip |
                                         DecompressionMethods.Deflate; 
            //web.UserAgent = @"Mozilla/5.0 Windows NT 6.1 WOW64 AppleWebKit/537.11 KHTML, like Gecko Chrome/23.0.1271.97 Safari/537.11";
            web.UserAgent = @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";

            Tools.TraceLine("Starting GetResponse with\n{0}", url);
            var resp = web.GetResponse();

            Tools.TraceLine("Starting GetStream");
            Stream responseStream;
            responseStream = resp.GetResponseStream();

            //store the returned data in memory
            StreamReader responseReader = new StreamReader(responseStream);

            string line;
            int line_limit = 10000000;
            int line_count = 0;


            //write the file from the response
            Tools.WriteTimeStamp("Starting to write file from the respone data");
            using (StreamWriter file = new StreamWriter(outputPath))
            {
                while ((line = responseReader.ReadLine()) != null && !(line_count > line_limit))
                {
                    file.WriteLine(line);
                    //Tools.TraceLine("returned: {0}", line);
                    //string msg = String.Format("Line number {0} written",
                    //                           line_count.ToString());
                    //Tools.TraceLine(msg);
                    line_count += 1;

                }
                file.Close();
            } */
            Tools.WriteTimeStamp();
            Tools.TraceLine("Successfully wrote and closed to {0}", outputPath);

        }

        public static void OptimizedPopulateOmdbTableFromTsv(
            string imdb_filepath,
            string tom_filepath)
        {

            ///The plan is to parse a the imdb file for a given amount of
            ///entries, then save them to the db, repeat until 300k movies are
            ///in there
            ///
            ///Then go over the RT data and add that data to the IMDB data found
            ///in the just created OmdbEntry table.



            //get the IMDB data saved to the db
            // missing the RottenTomatoes data, that happens next
            //OptimizedImdbTsvParse(imdb_filepath);

            //add the RT data
            //read 5000 movies until all the IMDB movies are parsed
            //5000 is about 180MB, 10000 was around 240MB 15000 can be as high as 300MB "
            int num_of_RT_movies_per_loop = 5000;
            using (
                CsvReader tom_csvReader =
                    new CsvReader(new StreamReader(tom_filepath), true, '\t',
                                  '~', '`', '~', ValueTrimmingOptions.None)) {

                while (tom_csvReader.ReadNextRecord()) {

                    //loop through the imdbtsv, creating a omdbentry for the first 5000 items
                    List<OmdbEntry> new_tom_omdb_entries = new List<OmdbEntry>();
                    for (int i = 0; i < num_of_RT_movies_per_loop; i++) {
                        //read the row and create an omdb from it
                        //parse the current TSV row
                        var entry =
                            Omdb.CreateOmdbEntryFromTsvRecord(
                                tomReader: tom_csvReader);
                        // add entry to a list
                        new_tom_omdb_entries.Add(entry);

                        //if nothing left to read, break out of the loop
                        if (tom_csvReader.ReadNextRecord() == false) {
                            Tools.TraceLine(
                                "ReadNextRecord was false, breaking out of loop to save it");
                            break;
                        }
                    }

                    //find all existing IMDB entries in db
                    MovieDbContext db = new MovieDbContext();
                    db.Configuration.AutoDetectChangesEnabled = true;
                    //find all existing OE that match the omdb_ids of the listed ones
                    Tools.TraceLine("items in db.Omdb {0}", db.Omdb.Count());

                    //get the omdb_ids of the new RT omdbentrys
                    List<int> tom_omdb_ids_to_match =
                        new_tom_omdb_entries.Select(omdb => omdb.ombd_ID)
                                            .ToList();
                    //match the ids to the exist IMDB omdbentries
                    var res = (from imdb in db.Omdb
                              where tom_omdb_ids_to_match.Contains(imdb.ombd_ID)
                              select imdb);
                    //Tools.TraceLine("items in res {0}", res.Count());
                    List<OmdbEntry> matched_existing_imdb_omdbentrys= res.ToList();


                    //alter the existing IMDB entries and save the changes...
                    foreach (
                        OmdbEntry matchedExistingImdbOmdbentry in matched_existing_imdb_omdbentrys ) {
                        //the RT omdb tha matches the imdb entry for the omdb_id
                        var matching_RT_data =
                            new_tom_omdb_entries.First(
                                item =>
                                item.ombd_ID ==
                                matchedExistingImdbOmdbentry.ombd_ID);

                        //update the imdb entry
                        matchedExistingImdbOmdbentry.t_Image =
                            matching_RT_data.t_Image;
                        matchedExistingImdbOmdbentry.t_Meter =
                            matching_RT_data.t_Meter;
                        matchedExistingImdbOmdbentry.t_Image =
                            matching_RT_data.t_Image;
                        matchedExistingImdbOmdbentry.t_Rating =
                            matching_RT_data.t_Rating;
                        matchedExistingImdbOmdbentry.t_Reviews =
                            matching_RT_data.t_Reviews;
                        matchedExistingImdbOmdbentry.t_Fresh =
                            matching_RT_data.t_Fresh;
                        matchedExistingImdbOmdbentry.t_Rotten =
                            matching_RT_data.t_Rotten;
                        matchedExistingImdbOmdbentry.t_Consensus =
                            matching_RT_data.t_Consensus;
                        matchedExistingImdbOmdbentry.t_UserMeter =
                            matching_RT_data.t_UserMeter;
                        matchedExistingImdbOmdbentry.t_UserRating =
                            matching_RT_data.t_UserRating;
                        matchedExistingImdbOmdbentry.t_UserReviews =
                            matching_RT_data.t_UserReviews;

                        //Tools.TraceLine("{0}\n{1}\n***********",
                        //                matchedExistingImdbOmdbentry.ombd_ID,
                        //                matching_RT_data.ombd_ID);
                    }
                    db.SaveChanges();
                    db.Dispose();
                    Tools.TraceLine("Updated existing IMDB OmdbEntrys, done saving");

                    //modify all those existing entries with listed data
                    //var merged_omdbs =
                    //    MergeTwoOmdbEntryLists(small_omdbEntry_list,
                    //                           matched_existing_omdbentrys);


                }


            }
        }

        private static void OptimizedImdbTsvParse(string imdb_filepath)
        {
            //read 5000 movies until all the IMDB movies are parsed
            //5000 is about 180MB, 10000 was around 240MB 15000 can be as high as 300MB "
            int num_of_movies_per_loop = 5000;
            using (
                CsvReader imdb_csvReader =
                    new CsvReader(new StreamReader(imdb_filepath), true, '\t',
                                  '~', '`', '~', ValueTrimmingOptions.None)) {
                while (imdb_csvReader.ReadNextRecord()) {
                    //loop through the imdbtsv, creating a omdbentry for the first 500 items
                    List<OmdbEntry> small_omdbEntry_list = new List<OmdbEntry>();
                    for (int i = 0; i < num_of_movies_per_loop; i++) {
                        //read the row and create an omdb from it
                        var entry =
                            Omdb.CreateOmdbEntryFromTsvRecord(
                                imdbReader: imdb_csvReader);
                        small_omdbEntry_list.Add(entry);

                        //if nothing left to read, break out of the loop
                        if (imdb_csvReader.ReadNextRecord() == false) {
                            Tools.TraceLine(
                                "ReadNextRecord was false, breaking out of loop to save it");
                            break;
                        }
                    }

                    //save the small_omdbEntry_list to db
                    MovieDbContext db = new MovieDbContext();
                    db.Configuration.AutoDetectChangesEnabled = false;
                    foreach (OmdbEntry omdbEntry in small_omdbEntry_list) {
                        db.Omdb.Add(omdbEntry);
                    }

                    Tools.TraceLine(
                        "saving omdbs. # of omdbs in table before save: {0}",
                        db.Omdb.Count());
                    db.SaveChanges();
                    db.Configuration.AutoDetectChangesEnabled = true;
                }

                Tools.TraceLine("Done saving IMDB OmdbEntrys");
            }
        }


        /// <summary>
                /// Parses omdb.txt and tomatoes.txt and returns a list of completed OmdbEntrys
                /// </summary>
                /// <param name="imdb_filepath">full path to omdb.txt from the OMDB API</param>
                /// <param name="tom_filepath">full path to tomatoes.txt from the OMDB API</param>
                /// <returns></returns>
            public static
            List<OmdbEntry> ParseTSVforOmdbData(string imdb_filepath,
                                                          string tom_filepath)
        {
            Tools.TraceLine("In ParseTSVforOmdbData");

            //TODO: make sure the files exist
            string full_i_path = Path.GetFullPath(imdb_filepath);
            string full_t_path = Path.GetFullPath(tom_filepath);
            Tools.TraceLine(
                "  Absolute IMDB path: {0}\n  Absolute Tomatoes path: {1}",
                full_i_path, full_t_path);

            var start_time = Tools.WriteTimeStamp("  Parsing the TSV");

            List<OmdbEntry> complete_list = new List<OmdbEntry>();

            Tools.WriteTimeStamp("  imdb start");
            var imdb_entries = ParseTSVforImdbData(imdb_filepath);
            Tools.WriteTimeStamp("  tomatoes start");
            var tom_entries = ParseTSVforTomatoesData(tom_filepath);

            Tools.TraceLine("  Starting to merge entries");
            Tools.WriteTimeStamp("  merge start");
            //combine the two lists
            complete_list = MergeTwoOmdbEntryLists(imdb_entries, tom_entries);
            Tools.WriteTimeStamp("  merge end");

            var complete_time = Tools.WriteTimeStamp("  done merging at");
            var duration = complete_time - start_time;
            var duration_msg = string.Format("  Took {0} to complete", duration);
            Tools.TraceLine(duration_msg);


            Tools.TraceLine("Out ParseTSVforOmdbData");
             return complete_list;

        }

        /// <summary>
        /// takes two lists of partial omdbentrys and then combines them
        /// </summary>
        /// <param name="imdb_list">a list of partial omdbentrys with their RT data missing</param>
        /// <param name="tom_list">a list of partial omdbentrys with their imdb data missing</param>
        /// <returns></returns>
    public static List<OmdbEntry> MergeTwoOmdbEntryLists(List<OmdbEntry> imdb_list,
                                    List<OmdbEntry> tom_list)
    {

        //left outer join two lists, one imdb data , one RT data
        var res = from imdb in imdb_list
                  join tom in tom_list on 
                  imdb.ombd_ID equals tom.ombd_ID into matched
                  from match in matched.DefaultIfEmpty()
                  select new OmdbEntry
                             {
                                 ombd_ID = imdb.ombd_ID,
                                 title = imdb.title,
                                 year = imdb.year,

                                 i_ID = imdb.i_ID,
                                 i_Rating = imdb.i_Rating,
                                 i_Votes = imdb.i_Votes,

                                 //if the joined omdb doesn't exist, fill in null
                                 t_Image = (match == null? "N/A" :match.t_Image),
                                 t_Consensus = (match == null ? "N/A": match.t_Consensus),
                                 t_Fresh = (match == null ? 0 : match.t_Fresh),
                                 t_Meter = (match == null ? 0: match.t_Meter),
                                 t_Rating = (match == null ? 0f : match.t_Rating),
                                 t_Reviews = (match == null ? 0 : match.t_Reviews),
                                 t_Rotten = (match == null ? 0 : match.t_Rotten),
                                 t_UserMeter = (match == null ? 0: match.t_UserMeter),
                                 t_UserRating = (match == null ? 0f: match.t_UserRating),
                                 t_UserReviews = (match == null ? 0 : match.t_UserReviews)

                             };

        List<OmdbEntry> complete_list = res.ToList();

        string msg = string.Format("Total entries in complete list: {0}",
                                   complete_list.Count);
        Tools.TraceLine(msg);
        return complete_list;

        }


        /// <summary>
        /// returns a list of OmdbEntrys that can be combines with tomatoes data to form a complete ombd entry
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static List<OmdbEntry> ParseTSVforImdbData(string filepath )
        {
            Tools.TraceLine("In ParseTSVforImdbData");
            Tools.TraceLine(" Start Parse for Imdb");

            using (
                CsvReader csvReader = new CsvReader(new StreamReader(filepath),
                                                    true, '\t', '~', '`', '~',
                                                    ValueTrimmingOptions.None))
            {
                List<OmdbEntry> omdbEntry_list;
                omdbEntry_list = new List<OmdbEntry>();

                int count = 0;
                //loop over all the rows until fail, adding the created entry to list
                while (csvReader.ReadNextRecord())
                {
                    var entry = Omdb.CreateOmdbEntryFromTsvRecord(imdbReader: csvReader);
                    omdbEntry_list.Add(entry);
                    //Tools.TraceLine(entry.title);
                    //Tools.TraceLine(count.ToString());
                    count++;
                }

                Tools.TraceLine(" Done Parsing for Imdb");
                Tools.TraceLine("out ParseTSVforImdbData");
                return omdbEntry_list;
            }
        }

        /// <summary>
        /// returns a list of OmdbEntrys that can be combines with imdb data to form a complete ombd entry
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static List<OmdbEntry> ParseTSVforTomatoesData(string filepath)
        {

                Tools.TraceLine("Start Parsing for Tomatoes");
            List<OmdbEntry> omdbEntry_list;
            //omdbEntry_list = existing_list ?? new List<OmdbEntry>();
            omdbEntry_list =  new List<OmdbEntry>();

            using (
                CsvReader csvReader = new CsvReader(new StreamReader(filepath),
                                                    true, '\t', '~', '`', '~',
                                                    ValueTrimmingOptions.None)) {
                int count = 0;
                //loop over all the rows until fail, adding the created entry to list
                while (csvReader.ReadNextRecord()) {
                    var entry =
                        Omdb.CreateOmdbEntryFromTsvRecord(tomReader: csvReader);
                    omdbEntry_list.Add(entry);
                    //Tools.TraceLine(count.ToString());
                    count++;
                }
                Tools.TraceLine("Done Parsing for Tomatoes");
                return omdbEntry_list;
            }
        }



    }
}
