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

                int fieldcount = csvReader.FieldCount;

                //csvReader.Delimiter = "\t";
                

                string[] headers = csvReader.GetFieldHeaders();

                int count = 0;
                while (csvReader.ReadNextRecord()) {
                    for (int i = 0; i < fieldcount; i++) {
                        string msg = String.Format("{0}\r{1};", headers[i],
                                                   csvReader[i]);
                        //Trace.Write(msg);
                    }
                    //Trace.WriteLine("\n");
                    Trace.WriteLine(count.ToString());
                    count++;
                }
            }
        }

    }
}