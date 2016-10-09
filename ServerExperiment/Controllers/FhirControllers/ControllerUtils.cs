using Hl7.Fhir.Serialization;
using ServerExperiment.Models.POCO;
using System;
using System.Net.Http.Formatting;
using ServerExperiment.Utils;

namespace ServerExperiment.Controllers.FhirControllers
{
    public static class ControllerUtils
    {
        public const string CREATE = "CREATE";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";
        public const string UNASSIGNED = "UNASSIGNED";

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
            string payload;

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

        public static IRecord AddMetadata(IRecord record, string action)
        {
            IRecord updatedRecord = record;

            updatedRecord.LastModified = DateTime.UtcNow;
            
            if (action == CREATE)
            {
                updatedRecord.Action = CREATE;
                updatedRecord.VersionId = 1;
            }
            else if (action == UPDATE)
            {
                updatedRecord.Action = UPDATE;
                updatedRecord.VersionId++;
            }
            else if (action == DELETE)
            {
                updatedRecord.Action = DELETE;
            }

            return updatedRecord;
        }

        public static MediaTypeFormatter ChooseMediaTypeFormatter(string format)
        {
            // Determine MediaTypeFormatter to use
            string fixedFormat = ControllerUtils.FixMimeString(format);

            MediaTypeFormatter formatter = null;
            if (fixedFormat.Equals("application/json+fhir", StringComparison.CurrentCultureIgnoreCase))
            {
                formatter = new JsonFhirFormatter();
            }
            else
            {
                formatter = new XmlFhirFormatter();
            }

            return formatter;
        }
    }
}