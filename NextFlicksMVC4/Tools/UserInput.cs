using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NextFlicksMVC4
{
	public static class UserInput
	{
		public static List<string> DeliminateStrings (List<string> userInput)
		{
			string[] tags = userInput[0].Split (',');
			List<String> deliminatedInput = new List<String>();
			foreach(string tag in tags)
			{
				deliminatedInput.Add (tag);
			}
			return deliminatedInput;
		}
		public static List<String> StripWhiteSpace (List<String> userInput)
		{
			List<String> trimmedInput = new List<String>();
			
			for(int i = 0; i < userInput.Count; i++)
			{
				trimmedInput.Add(userInput[i].Trim());
			}
			return trimmedInput;
		}
		
		public static List<String> SanitizeSpecialCharacters (List<String> userInput)
		{
			List<String> sanitizedInput = new List<string> ();
			foreach (string tag in userInput) 
			{
				Regex badCharReplace = new Regex (@"([<>""'%;()&])");
				string goodChars = badCharReplace.Replace (tag, "");
				sanitizedInput.Add (goodChars);
			}
			return sanitizedInput;
		}
	}
}

