using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NextFlicksMVC4.Controllers;
using System.Xml;

namespace NextFlicksMVC4.Tools
{
    public static class Tools
    {
        public static string GetParamName(System.Reflection.MethodInfo method, int index)
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
    }


    
}