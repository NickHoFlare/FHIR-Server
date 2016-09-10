using Hl7.Fhir.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExperiment.Controllers.FhirControllers
{
    public static class ControllerUtils
    {
        public static string FixMimeString(string mimeFormat)
        {
            if (mimeFormat.IndexOf("JSON", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                mimeFormat = "application/JSON+FHIR";
            }
            else
            {
                mimeFormat = "application/XML+FHIR";
            }

            return mimeFormat;
        }
        public static string Serialize(Hl7.Fhir.Model.Resource fhirResource, string mimeFormat, bool summary)
        {
            string payload = string.Empty;

            if (mimeFormat.IndexOf("JSON", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                payload = FhirSerializer.SerializeResourceToJson(fhirResource, summary);
            }
            else
            {
                payload = FhirSerializer.SerializeResourceToXml(fhirResource, summary);
            }

            return payload;
        }

    }
}