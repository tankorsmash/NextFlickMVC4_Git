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


        public static void DownloadOmdbZip(string url = @"http://www.beforethecode.net/projects/OMDb/download.aspx?e=tankorsmash%40gmail.com", string outputPath = @"omdb.zip")
        {

            //doesn't work
            //Tools.TraceLine("start download:\n{0}", url);
            //using (WebClient client = new WebClient()) {
                
            //    client.DownloadFile(url, outputPath);
            //}
            //Tools.TraceLine("done download");


            //download the omdbzip
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(url);
            web.KeepAlive = true;
            web.AutomaticDecompression = DecompressionMethods.GZip |
                                         DecompressionMethods.Deflate;


            Tools.TraceLine("Starting GetResponse with\n{0}", url);
            var resp = web.GetResponse();

            Trace.WriteLine("Starting GetStream");
            Stream responseStream;
            responseStream = resp.GetResponseStream();

            //store the returned data in memory
            StreamReader responseReader = new StreamReader(responseStream);

            string line;
            int line_limit = 10000000;
            int line_count = 0;


            //write the file from the response
            Tools.WriteTimeStamp("Starting to write");
            using (StreamWriter file = new StreamWriter(outputPath, append: true))
            {
                while ((line = responseReader.ReadLine()) != null && !(line_count > line_limit))
                {
                    file.WriteLine(line);
                    line_count += 1;
                    string msg = String.Format("Line number {0} written",
                                               line_count.ToString());
                    Trace.WriteLine(msg);

                }
                file.Close();
            }
            Tools.WriteTimeStamp();
            Trace.WriteLine("Successfully wrote and closed to {0}", outputPath);






            //extract it
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

            Trace.WriteLine("Starting to merge entries");
            Tools.WriteTimeStamp("merge start");
            complete_list = MergeTwoOmdbEntryLists(imdb_entries, tom_entries);
            Tools.WriteTimeStamp("merge end");

            //save to file so I don't  have to keep recreating objects

            var complete_time = Tools.WriteTimeStamp("done merging at");
            var duration = complete_time - start_time;
            var duration_msg = string.Format("Took {0} to complete", duration);
            Trace.WriteLine(duration_msg);


            return complete_list;

        }

    public static List<OmdbEntry> MergeTwoOmdbEntryLists(List<OmdbEntry> first_list,
                                    List<OmdbEntry> second_list)
        {
            List<OmdbEntry> complete_list = new List<OmdbEntry>();

            foreach (OmdbEntry omdbEntry in first_list) {
                int current_id = omdbEntry.ombd_ID;

                //find matching tomatoes.txt entry
                OmdbEntry selected_tom_entry =
                    second_list.SingleOrDefault(item => item.ombd_ID == current_id);

                //If there IS a matching tomato data
                if (selected_tom_entry != null)
                {
                    var created_entry =
                        OMBD.Omdb.MergeImdbWithTomatoesOmdbEntry(omdbEntry,
                                                                 selected_tom_entry);
                    complete_list.Add(created_entry);
                }
                //if there's no matching query, there's no matching toms data so 
                // just add the imdb  to complete list
                else if (selected_tom_entry == null) {
                    complete_list.Add(omdbEntry);
                }

                Trace.WriteLine(current_id);
            }

            string msg = string.Format("Total entries in complete list: {0}",
                                       complete_list.Count);
            Trace.WriteLine(msg);
            return complete_list;
        }


        /// <summary>
        /// returns a list of OmdbEntrys that can be combines with tomatoes data to form a complete ombd entry
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static List<OmdbEntry> ParseTSVforImdbData(string filepath )
        {

            Trace.WriteLine("Start Parse for Imdb");

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
                    //Trace.WriteLine(entry.title);
                    Trace.WriteLine(count.ToString());
                    count++;
                }

                Trace.WriteLine("Done Parsing for Imdb");
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

                Trace.WriteLine("Start Parsing for Tomatoes");
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
                    Trace.WriteLine(count.ToString());
                    count++;
                }
                Trace.WriteLine("Done Parsing for Tomatoes");
                return omdbEntry_list;
            }
        }



    }
}
