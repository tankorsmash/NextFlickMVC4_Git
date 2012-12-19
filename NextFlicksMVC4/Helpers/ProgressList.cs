using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace NextFlicksMVC4.Helpers
{
    public static class ProgressList
    {

        public static List<int> CreateListOfCheckpointInts(int totalNumber, int divisions)
        {
            List<int> checkpoints = new List<int>();

            //get the number that divides the totalNumber by divisions
            int divisor = totalNumber / divisions;

            //now use that number to get modulo'd and add that number to the
            //list
            for (int i = 0; i <= totalNumber; i++)
            {
                try {
                    if (i%divisor == 0) {
                        checkpoints.Add(i);
                    }
                }
                catch (DivideByZeroException ex) {
                    Trace.WriteLine("div by 0 exception");
                }
            }

            return checkpoints;


        }

    }
}
