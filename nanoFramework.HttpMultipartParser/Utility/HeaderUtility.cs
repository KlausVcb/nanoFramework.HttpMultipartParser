using System.Collections;

namespace nanoFramework.HttpMultipartParser.Utility
{
    public class HeaderUtility
    {
        public static void ParseHeaders(string text, Hashtable headers)
        {
            bool inQuotes = false;
            bool inKey = true;
            string key = string.Empty;
            string value = string.Empty;

            foreach (char c in text)
            {
                if (c == '"') 
                    inQuotes = !inQuotes;
                else if (inQuotes) 
                    value += c;
                else if (c == ';')
                {
                    headers[key.ToLower()] = value;
                    key = string.Empty;
                    inKey = true;
                }
                else if (c == '=' || c == ':')
                {
                    value = string.Empty;
                    inKey = false;
                }
                else if (c == ' ')
                    continue;
                else if (inKey)
                    key += c;
                else
                    value += c;
            }

            if(!string.IsNullOrEmpty(key)) headers.Add(key.ToLower(), value);
        }
    }
}
