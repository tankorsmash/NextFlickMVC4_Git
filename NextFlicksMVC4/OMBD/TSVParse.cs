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
            using (CsvReader csvReader = new CsvReader(new StreamReader(filepath), true)) {

                int fieldcount = csvReader.FieldCount;

                string[] headers = csvReader.GetFieldHeaders();

                while (csvReader.ReadNextRecord()) {
                    for (int i = 0; i < fieldcount; i++) {
                        string msg = String.Format("{0} = {1};", headers[i],
                                                   csvReader[i]);
                        Trace.Write(msg);
                    }
                    Trace.WriteLine("\n");
                }
            }
        }

    }
}