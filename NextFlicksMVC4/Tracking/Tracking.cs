using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using NextFlicksMVC4.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NextFlicksMVC4.Tracking
{

    public class UserLog
    {
        [Key]
        public int userlog_id { get; set; }

        [DisplayName("IP Address")]
        public string ip_addr { get; set; }

        [DisplayName("Time")]
        public DateTime time { get; set; }

        [DisplayName("User Agent")]
        public string useragent { get; set; }

        [DisplayName("Target Raw URL")]
        public string raw_url { get; set; }

        [DisplayName("Referrer")]
        public string referrer { get; set; }

        //probably need to track cookies or something here too

    }


    public class TrackingDbContext : DbContext
    {
        public DbSet<UserLog> UserLogs { get; set; }
    }
}