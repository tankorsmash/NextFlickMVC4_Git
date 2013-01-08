using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using NextFlicksMVC4.Controllers;
using System.Xml;
using System.Diagnostics;

namespace NextFlicksMVC4.Tools
{
    public static class Tools
    {
        public static string GetParamName(MethodInfo method, int index)
        {
            string retVal = String.Empty;

            if (method != null && method.GetParameters().Length > index) {
                retVal = method.GetParameters()[index].Name;
            }

            return retVal;
        }

        /// <summary>
        /// loop over params and save all the names in a list
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static List<string> GetAllParamNames(string methodName)
        {
            List<string> param_names = new List<string>();

            for (int i = 0; i < 15; i++) {
                var info = typeof (MoviesController).GetMethod(methodName);
                string param_name = GetParamName(info, i);
                
                //TODO: Improve method
                //if the param is unnamed, there's probably no more params. Improve it
                if (param_name == "")
                    break;

                param_names.Add(param_name);
            }

            return param_names;

        }

        //public static string GetXmlText(XmlDocument xmlDocument, string xpath, string error_msg = @"N/A")
        //{

        //    string result = xmlDocument.SelectSingleNode(xpath).InnerText;

        //    return result;
        //}

        /// <summary>
        /// Takes a message string and Trace.WriteLines it along with a DateTime, default Now
        /// </summary>
        /// <param name="msg">Message to write</param>
        /// <param name="time">A custom time value</param>
        public static void WriteTimeStamp(string msg, DateTime? time = null)
        {
            if (time == null) { time = DateTime.Now; }

            string to_write = string.Format("{0}: {1}", msg,
                                            time.Value.ToShortTimeString());
            Trace.WriteLine(to_write);

        }

        public static void MergeWithSlow<T>(this T primary, T secondary)
        {
            foreach (var pi in typeof(T).GetProperties())
            {
                var priValue = pi.GetGetMethod().Invoke(primary, null);
                var secValue = pi.GetGetMethod().Invoke(secondary, null);
                if (priValue == null || (pi.PropertyType.IsValueType && priValue.Equals(Activator.CreateInstance(pi.PropertyType))))
                {
                    pi.GetSetMethod().Invoke(primary, new object[] { secValue });
                }
            }
        }
    }


    
}