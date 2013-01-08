using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using LumenWorks.Framework.IO.Csv;
using System.Diagnostics;

namespace NextFlicksMVC4.OMBD
{
    public static class TSVParse
    {

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


            var complete_time = Tools.WriteTimeStamp("completely done at");
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

                if (selected_tom_entry != null) {
                    //Tools.Tools.MergeWithSlow(omdbEntry, selected_tom_entry);
                    OMBD.Omdb.MergeImdbWithTomatoesOmdbEntry(omdbEntry,
                                                             selected_tom_entry);
                    complete_list.Add(omdbEntry);
                }

                Trace.WriteLine(current_id);
            }

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
