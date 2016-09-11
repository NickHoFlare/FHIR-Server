using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.Device;
using ServerExperiment.Models.FHIR.Helpers.Device;

namespace ServerExperiment.Models.FHIR.Mappers
{
    public class DeviceMapper
    {
        /// <summary>
        /// Given a Device Resource, maps the data in the resource to a Device POCO.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static Models.Device MapResource(Resource resource)
        {
            var source = resource as Hl7.Fhir.Model.Device;
            if (source == null)
            {
                throw new ArgumentException("Resource in not a HL7 FHIR Device resouce");
            }

            Models.Device device = new Models.Device();

            // Device Type
            device.TypeCode = source.Type.Coding.FirstOrDefault().Code;
            device.TypeDisplay = source.Type.Coding.FirstOrDefault().Display;
            device.TypeSystem = source.Type.Coding.FirstOrDefault().System;
            device.TypeText = source.Type.Text;

            // Device Status
            var status = source.Status.GetValueOrDefault();
            switch (status)
            {
                case DeviceStatus.Available:
                    device.Status = Status.available;
                    break;
                case DeviceStatus.NotAvailable:
                    device.Status = Status.not_available;
                    break;
                case DeviceStatus.EnteredInError:
                    device.Status = Status.entered_in_error;
                    break;
                default:
                    device.Status = Status.entered_in_error;
                    break;
            }

            // Device Other details
            device.Manufacturer = source.Manufacturer;
            device.Model = source.Model;
            device.Udi = source.Udi;
            device.Expiry = source.Expiry;
            device.LotNumber = source.LotNumber;
            device.PatientReference = source.Patient.Reference;

            return device;
        }

        /// <summary>
        /// Given a device POCO, maps the data to a device Resource.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Hl7.Fhir.Model.Device MapModel(Models.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var resource = new Hl7.Fhir.Model.Device();

            resource.Id = device.DeviceId.ToString("D");

            // Device Type
            if (device.TypeCode != null || device.TypeDisplay != null || device.TypeSystem != null || device.TypeText != null)
            {
                CodeableConcept deviceType = new CodeableConcept();
                List<Coding> deviceCodings = new List<Coding>();

                if (device.TypeCode != null || device.TypeDisplay != null || device.TypeSystem != null)
                {
                    Coding deviceCoding = new Coding()
                    {
                        System = device.TypeSystem,
                        Display = device.TypeDisplay,
                        Code = device.TypeCode
                    };
                    deviceCodings.Add(deviceCoding);
                    deviceType.Coding = deviceCodings;
                }
                deviceType.Text = device.TypeText;
                
                resource.Type = deviceType;
            }

            // Device Status
            switch (device.Status)
            {
                case Status.available:
                    resource.Status = DeviceStatus.Available;
                    break;
                case Status.not_available:
                    resource.Status = DeviceStatus.NotAvailable;
                    break;
                case Status.entered_in_error:
                    resource.Status = DeviceStatus.EnteredInError;
                    break;
                default:
                    resource.Status = DeviceStatus.EnteredInError;
                    break;
            }

            // Device Other details
            resource.Manufacturer = device.Manufacturer;
            resource.Model = device.Model;
            resource.Expiry = device.Expiry;
            resource.Udi = device.Udi;
            resource.LotNumber = device.LotNumber;

            // Device Reference to Patient
            if (device.PatientReference != null)
            {
                resource.Patient = new ResourceReference();
                resource.Patient.Reference = device.PatientReference;
            }

            return resource;
        }
    }
}