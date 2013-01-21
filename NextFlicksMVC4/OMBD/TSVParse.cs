using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using LumenWorks.Framework.IO.Csv;
using System.Diagnostics;
using System.Net;

namespace NextFlicksMVC4.OMBD
{
    public static class TSVParse
    {


        public static void DownloadOmdbZip(string url = @"http://www.beforethecode.net/projects/OMDb/download.aspx?e=tankorsmash@gmail.com" , string outputPath = @"omdb.zip")
        {

            //doesn't work
            //Tools.TraceLine("start download:\n{0}", url);
            //using (WebClient client = new WebClient()) {
                
            //    client.DownloadFile(url, outputPath);
            //}
            //Tools.TraceLine("done download");

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
            Tools.WriteTimeStamp("Starting to write");
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
            }
            Tools.WriteTimeStamp();
            Tools.TraceLine("Successfully wrote and closed to {0}", outputPath);






        }


        /// <summary>
        /// Parses omdb.txt and tomatoes.txt and returns a list of completed OmdbEntrys
        /// </summary>
        /// <param name="imdb_filepath">full path to omdb.txt from the OMDB API</param>
        /// <param name="tom_filepath">full path to tomatoes.txt from the OMDB API</param>
        /// <returns></returns>
        public static List<OmdbEntry> ParseTSVforOmdbData(string imdb_filepath,
                                                          string tom_filepath)
        {

            var start_time = Tools.WriteTimeStamp("Parsing operation star");

            List<OmdbEntry> complete_list = new List<OmdbEntry>();

            Tools.WriteTimeStamp("imdb start");
            var imdb_entries = ParseTSVforImdbData(imdb_filepath);
            Tools.WriteTimeStamp("tomatoes start");
            var tom_entries = ParseTSVforTomatoesData(tom_filepath);

            Tools.TraceLine("Starting to merge entries");
            Tools.WriteTimeStamp("merge start");
            //combine the two lists
            complete_list = MergeTwoOmdbEntryLists(imdb_entries, tom_entries);
            Tools.WriteTimeStamp("merge end");

            //save to file so I don't  have to keep recreating objects

            var complete_time = Tools.WriteTimeStamp("done merging at");
            var duration = complete_time - start_time;
            var duration_msg = string.Format("Took {0} to complete", duration);
            Tools.TraceLine(duration_msg);


            return complete_list;

        }

    public static List<OmdbEntry> MergeTwoOmdbEntryLists(List<OmdbEntry> first_list,
                                    List<OmdbEntry> second_list)
    {

        //left out join two lists, one imdb data , one RT data
        var res = from imdb in first_list
                  join tom in second_list on 
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

        

        //old way below

            //List<OmdbEntry> complete_list = new List<OmdbEntry>();

            //foreach (OmdbEntry imdbEntry in first_list) {
            //    int current_id = imdbEntry.ombd_ID;

            //    //find matching tomatoes.txt entry
            //    OmdbEntry selected_tom_entry =
            //        second_list.SingleOrDefault(item => item.ombd_ID == current_id);

            //    //If there IS a matching tomato data
            //    if (selected_tom_entry != null)
            //    {
            //        var created_entry =
            //            OMBD.Omdb.MergeImdbWithTomatoesOmdbEntry(imdbEntry,
            //                                                     selected_tom_entry);
            //        complete_list.Add(created_entry);
            //    }
            //    //if there's no matching query, there's no matching toms data so 
            //    // just add the imdb  to complete list
            //    else if (selected_tom_entry == null) {
            //        complete_list.Add(imdbEntry);
            //    }

            //    //Tools.TraceLine(current_id.ToString());

            //}

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

            Tools.TraceLine("Start Parse for Imdb");

            using (
                CsvReader csvReader = new CsvReader(new StreamReader(filepath),
                                                    true, '\t', '~', '`', '~',
                                                    ValueTrimmingOptions.None)) 
            {
            List<OmdbEntry> omdbEntry_list;
            omdbEntry_list = new List<OmdbEntry>();

                int count = 0;
                //loop over all the rows until fail, adding the created entry to list
                while (csvReader.ReadNextRecord()) {
                    var entry = Omdb.CreateOmdbEntryFromTsvRecord(imdbReader:csvReader);
                    omdbEntry_list.Add(entry);
                    //Tools.TraceLine(entry.title);
                    //Tools.TraceLine(count.ToString());
                    count++;
                }

                Tools.TraceLine("Done Parsing for Imdb");
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
