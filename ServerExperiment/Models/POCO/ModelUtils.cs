using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Hl7.Fhir.Support;

namespace ServerExperiment.Models.POCO
{
    public static class ModelUtils
    {
        public static string ReturnSerialisedString(List<string> strings)
        {
            string serialisedString = null;

            if (!strings.IsNullOrEmpty())
            {
                if (strings[0] != string.Empty)
                {
                    serialisedString = string.Join(";", strings);
                }
            }

            return serialisedString;
        }

        public static List<string> DeserialiseString(string serialisedString)
        {
            List<string> strings = new List<string>();

            if (!serialisedString.IsNullOrEmpty())
            {
                strings = serialisedString.Split(';').ToList();
            }

            return strings;
        }

    }
}