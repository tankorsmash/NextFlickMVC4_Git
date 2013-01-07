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

        public static void ParseTSVforOmdbData(string filepath)
        {
            using (CsvReader csvReader = new CsvReader(new StreamReader(filepath), true, '\t', '~', '`', '~', ValueTrimmingOptions.None)) {

                string[] headers = csvReader.GetFieldHeaders();

                int count = 0;
                //count the fields/column name in the TSV
                int fieldcount = csvReader.FieldCount;

                //loop over all the rows until fail
                while (csvReader.ReadNextRecord()) {
                    //for each field in record, assign the value to an object
                    //for (int i = 0; i < fieldcount; i++) {
                    //string msg = String.Format("{0}:{1};", headers[i],
                    //                           csvReader[i]);
                    //Trace.Write(msg);

                    var entry = Omdb.CreateOmdbEntryFromTsvRecord(csvReader);

                    Trace.WriteLine(entry.title);
                //}

                //Trace.WriteLine("\n");
                    Trace.WriteLine(count.ToString());
                    count++;
                }
            }
        }

    }
}