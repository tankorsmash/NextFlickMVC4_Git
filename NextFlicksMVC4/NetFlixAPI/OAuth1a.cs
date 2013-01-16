using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace NextFlicksMVC4.NetFlixAPI
{
    static class OAuth1a
    {

        //gameplan:
        // target URL
        // Netflix params: Title, offset, limit
        // OAuth params: consumer_key, nonce, timestamp, signature_method
        //
        // and then the signature after, which is based on the above encoded
        //
        // HMAC outputs 20 chars, turn that to b64 then encode that
        //
        // Need to encode certain chars AND make sure to encode it befre I pass
        // it to the MHA or whatever

        /// <summary>
        /// Requests catalog/titles or catalogs/people and writes to  a file, if one is specified, otherwise it returns an empty string
        /// </summary>
        /// <param name="resource">which catalog item you want, catalog/titles or catalog/people</param>
        /// <param name="term">the words you are searching for</param>
        /// <param name="start_index"></param>
        /// <param name="max_results">Limit is 100</param>
        /// <param name="outputPath"></param>
        public static string GetNextflixCatalogDataString(string resource, string term,
                string start_index = "0", string max_results = "25", string outputPath = "")
        {

            const string verb = "GET";
            string base_url = String.Format("http://api-public.netflix.com/{0}", resource);

                //Not going to change
            string consumer_key = "u7r68et24v6rd5r9u828qvte";
            string consumer_secret = "uWdp2AXnnu";
            string oauth_signature_method = "HMAC-SHA1";
            string oauth_version = "1.0";

                //dynamic values
            string oauth_timestamp = GenerateTimeStamp();
            string oauth_nonce = GenerateTimeStamp();
            string oauth_signature = GenerateSignature(verb, base_url,
                                                       consumer_key,
                                                       consumer_secret,
                                                       oauth_timestamp,
                                                       oauth_nonce,
                                                       oauth_signature_method,
                                                       new KeyValuePair
                                                           <string, string>(
                                                           "term", term),
                                                       new KeyValuePair
                                                           <string, string>(
                                                           "start_index", start_index),
                                                       new KeyValuePair
                                                           <string, string>(
                                                           "max_results", max_results)
                                                           );
                                        //supposed to limit results to a country, but no luck
                                                       //new KeyValuePair<string, string>(
                                                       //    "country", "US"));
            Trace.WriteLine(oauth_signature);

            //Need to send the request now

            //needs the baseurl, then add in all the params, followed by the
            //signature

            //this creates the URL we are going to send. It takes the URL specified
            // at the top of the method, and then uses the rest of the params string
            // with the oauth_signature
            string toSend = String.Format("{0}?{1}", base_url, oauth_signature);

            //get the URL to send, now we need to create ethe request
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(toSend);
            web.KeepAlive = true;


            Trace.WriteLine("Starting GetResponse with\n{0}", toSend);
            var resp = web.GetResponse();

            Trace.WriteLine("Starting GetStream");
            Stream objStream;
            objStream = resp.GetResponseStream();

            //store the returned data in memory
            StreamReader objReader = new StreamReader(objStream);
            //var data = objReader.ReadToEnd();


            //empty the file. TODO: make good.
            using (StreamWriter writer = new StreamWriter(outputPath))
            { //empty the bitch
            }

            string line;
            int line_limit = 10000000;
            int line_count = 0;

            Tools.WriteTimeStamp("Starting to write");
            using (StreamWriter file = new StreamWriter(outputPath, append: true)) {
                while ((line = objReader.ReadLine()) != null && !(line_count >line_limit))
                {
                    file.WriteLine(line);
                    line_count += 1;
                    //string msg = String.Format("Line number {0} written",
                    //                           line_count.ToString());
                    //Trace.WriteLine(msg);

                }
                file.Close();
            }
            Tools.WriteTimeStamp();
            Trace.WriteLine("Successfully wrote and closed to {0}", outputPath);
            return outputPath;
            //}
            //else
            //{
            //    Trace.WriteLine("No filepath specified, return string instead");
            //    Trace.WriteLine("Done!");
            //    return data;
            //}

        }

        public static string GenerateTimeStamp()
        {
            double time = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return Convert.ToInt32(time).ToString();
        }

        public static string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GenerateSignature(string HttpMethod, string url,
                string consumer_key, string consumer_secret, string
                oauth_timestamp, string oauth_nonce, string
                oauth_signature_method= "HMACSHA1", params KeyValuePair<string,
                string>[] parameters)
        {
            //so this needs an encoded base string, which'll be
            //METHOD&URL&PARAMS
            //then I pass it to the HMAC-SHA1 which spits out a 20char, which'll
            //be encoded and used as sig param on the unencoded basestring

            //create BASESTRING where we already know the METHOD and URL, so we
            //need to grab all the params

            //This is a set of params that need to be passed to the paramString method, but the
            // method is expecting an array of KVPs so I need to make an array
            // from the default OAuth args and then add the custom params, like
            // Term and stuff later.
            KeyValuePair<string, string>[] oauthDefaultParams =
                {
                    new KeyValuePair<string, string>("oauth_consumer_key", consumer_key),
                    new KeyValuePair<string, string>("oauth_signature_method", oauth_signature_method),
                    new KeyValuePair<string, string>("oauth_version", "1.0"),
                    new KeyValuePair<string, string>("oauth_timestamp", oauth_timestamp),
                    new KeyValuePair<string, string>("oauth_nonce", oauth_nonce)
                };

            //Turn the array to a list so you can addrange, then back to an array
            // for the sake of the method which requires it.

            // but first, remove spaces from the values in the list
            var netflixParameters = parameters.ToList();
            //this has to be bad ideas, but I copy the list, then iterate over that
            // while replacing the values in the original list
            foreach (var kvp in new List<KeyValuePair<string,string>>(netflixParameters))
            {
                var key = kvp.Key;
                var val = kvp.Value.Replace(" ", "+");
                var index = netflixParameters.IndexOf(kvp);
                var newKvp = new KeyValuePair<string, string>(key, val);
                netflixParameters[index] = newKvp;
            }

            //Add in the oauth params that need to be in every signed request
            netflixParameters.AddRange(oauthDefaultParams);
            //turn the list back into an array
            var combinedParamsArray = netflixParameters.ToArray();
            //throw the array to the function to get back a parameter string
            string enc_params = GenerateEncodedParameterString(combinedParamsArray);
            
            //Now we've got the encoded param set, we need to encode the uri and prepend the httpmethod.

            //TODO: fix the hardcoded key so that it can accept tokens too
            string signString = consumer_secret + "&";

            //need this basestring to combine with the signString to make the
            //signature, once we add it to the SHA1 or whatever
            string encodedBaseString = GenerateEncodedBaseString(HttpMethod, url, enc_params);


            //returns the signature that we append to the url string we send to
            //the API
            string encoded_signature = Sign(encodedBaseString, signString);

            //return encoded_signature;
            //return HttpUtility.UrlEncode(encoded_signature);

            //return paramstring with signature
            return enc_params + "&oauth_signature=" + UpperCaseUrlEncode(encoded_signature);
        }

        //sorts the params you're about to send to the api and then encodes
        //them. It's still uses the old HttpUtility Encode, but it doesn't seem
        //to matter just yet. We'll see.
        public static string GenerateEncodedParameterString( params KeyValuePair<string,string>[] parameters)
        {
            var q = from entry in parameters
                let encodedkey = UpperCaseUrlEncode(entry.Key)
                let encodedValue = UpperCaseUrlEncode(entry.Value)
                let encodedEntry = encodedkey + "=" + encodedValue
                orderby encodedEntry
                select encodedEntry;
            var a = q.ToArray();
            var result = string.Join("&", a);
            return result;
        }

        //wraps HttpUtility.UrlEncode so that it outputs capitalized code.
        //Required for NetflixAPI
        public static string UpperCaseUrlEncode(string s)
        {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }
        

        //returns method&url&params encoded
        public static string GenerateEncodedBaseString( string HttpMethod, string baseURI, string paramString)
        {
             return HttpMethod.ToUpper() + "&" + UpperCaseUrlEncode(baseURI) + "&" + UpperCaseUrlEncode(paramString);
        }

        //I have no idea how this works, but it takes the signing key and uses
        //it to create somesort of cryptokey and then applies it to the
        //basestring and then returns the b64 version of it.
        public static string Sign(string signatureBaseString, string signingKey)
        {
            var keyBytes = System.Text.Encoding.ASCII.GetBytes(signingKey);
            using (var myhmacsha1 = new System.Security.Cryptography.HMACSHA1(keyBytes)) {
                byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(signatureBaseString);
                var stream = new MemoryStream(byteArray);
                var signedValue = myhmacsha1.ComputeHash(stream);
                var result = Convert.ToBase64String(signedValue, Base64FormattingOptions.None);
                return result;
            }
        }


    }
}
