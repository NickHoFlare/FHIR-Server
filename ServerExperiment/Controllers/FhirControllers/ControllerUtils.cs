using Hl7.Fhir.Serialization;
using ServerExperiment.Models;
using ServerExperiment.Models.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public static IModel AddMetadata(IModel model, string action)
        {
            IModel updatedModel = model;

            updatedModel.RecordId++;
            updatedModel.LastModified = DateTime.UtcNow;
            
            if (action == CREATE)
            {
                updatedModel.Action = CREATE;
                updatedModel.IsDeleted = false;
                updatedModel.VersionId = 1;
            }
            else if (action == UPDATE)
            {
                updatedModel.Action = UPDATE;
                updatedModel.IsDeleted = false;
                updatedModel.VersionId++;
            }
            else if (action == DELETE)
            {
                updatedModel.Action = DELETE;
                updatedModel.IsDeleted = true;
            }

            return updatedModel;
        }
    }
}