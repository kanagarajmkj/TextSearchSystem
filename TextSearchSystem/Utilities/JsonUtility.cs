using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TextSearchSystem.Utilities
{
    internal static class JsonUtility
    {
        public static JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();
        public static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }
        public static T Deserialize<T>(string input)
        {
            return JsonSerializer.Deserialize<T>(input);
        }
    }
}
