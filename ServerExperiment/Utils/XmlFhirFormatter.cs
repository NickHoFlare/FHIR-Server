using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.IO;
using Hl7.Fhir.Serialization;
using System.Xml;

namespace ServerExperiment.Utils
{
    public class XmlFhirFormatter : BufferedMediaTypeFormatter
    {
        public XmlFhirFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml+fhir"));
        }

        public override bool CanReadType(Type type)
        {
            bool canRead = false;

            // for single product object
            if (type == typeof(Hl7.Fhir.Model.Patient) ||
                type == typeof(Hl7.Fhir.Model.Device)  ||
                type == typeof(Hl7.Fhir.Model.Observation))
            {
                canRead = true;

                return canRead;
            }
            return canRead;
        }

        public override bool CanWriteType(Type type)
        {
            bool canWrite = false;

            //for single product object
            if (type == typeof(Hl7.Fhir.Model.Patient) ||
                type == typeof(Hl7.Fhir.Model.Device) ||
                type == typeof(Hl7.Fhir.Model.Observation))
            {
                canWrite = true;

                return canWrite;
            }
            return canWrite;
        }

        public override void WriteToStream(Type type,
                                    object value,
                                    Stream writeStream,
                                    HttpContent content)
        {
            using (StreamWriter writer = new StreamWriter(writeStream))
            {
                var resource = value as Hl7.Fhir.Model.Resource;
                if (resource != null)
                {
                    string payload = FhirSerializer.SerializeResourceToXml(resource);

                    writer.Write(payload);
                }
                else
                {
                    throw new InvalidOperationException("Cannot serialize type");
                }
            }
        }


        public override object ReadFromStream(Type type,
                                              Stream readStream,
                                              HttpContent content,
                                              IFormatterLogger formatterLogger)
        {
            string resourceString = string.Empty;
            Hl7.Fhir.Model.Resource resource = null;

            using (StreamReader reader = new StreamReader(readStream))
            {
                resourceString = reader.ReadToEnd();
            }
            
            if (FhirParser.ProbeIsXml(resourceString))
            {
                resource = FhirParser.ParseResourceFromXml(resourceString) as Hl7.Fhir.Model.Resource;
            }

            return resource;
        }
    }
}