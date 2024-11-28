using System.Collections;

namespace nanoFramework.HttpMultipartParser.Utility
{
    internal static class HashtableUtility
    {
        public static bool TryGetValue(this Hashtable hashtable, string key, out string value) 
        {
            if (hashtable != null && hashtable.Contains(key))
            {
                var obj = hashtable[key];
                value = obj == null ? string.Empty : obj.ToString();
                return true;
            }

            value = null;
            return false;
        }
    }
}
