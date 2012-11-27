using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextFlicksMVC4
{
    public static class DisplayTools
    {
        public static TimeSpan SecondsToTimeSpan(string seconds)
        {
            if (seconds != "not set")
            {
                //return a time string in dd/hh/mm/ss
                int sec = Int32.Parse(seconds);
                TimeSpan span = new TimeSpan(0, 0, sec);
                var formatted = String.Format("{0}:{1:00}",
                                              (int) span.TotalMinutes,
                                              span.Seconds);
                return span;
                //return formatted;
            }
            else
            {
                return new TimeSpan();
            }
        }
    }
}