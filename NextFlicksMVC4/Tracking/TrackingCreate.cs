using System;
using System.Web;

namespace NextFlicksMVC4.Tracking
{
    public static class TrackingCreate
    {
        public static UserLog CreateUserLog(HttpRequestBase request)
        {
            var ip = request.UserHostAddress;
            var userAgent = request.UserAgent;
            var rawUrl = request.RawUrl;
            //could be null
            var referrer_raw =
                request.UrlReferrer;
            string referrer_str;
            //if ref_raw != null, ref_str ==raw.to_str() else it's null
            referrer_str = referrer_raw != null ? referrer_raw.ToString() : null;
            var time = DateTime.UtcNow;

            UserLog userLog = new UserLog
                                  {
                                      ip_addr = ip,
                                      raw_url = rawUrl,
                                      //?? means like catching
                                      referrer = referrer_str,
                                      time = time,
                                      useragent = userAgent,
                                  };

            return userLog;
        }
    }
}