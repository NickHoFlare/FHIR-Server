using System;
using System.Collections.Generic;
using System.Linq;
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
        public static POCO.Device MapResource(Resource resource)
        {
            var source = resource as Device;
            if (source == null)
            {
                throw new ArgumentException("Resource in not a HL7 FHIR Device resouce");
            }

            POCO.Device device = new POCO.Device();

            device.DeviceId = int.Parse(resource.Id);

            // Device Type
            if (source.Type != null)
            {
                if (source.Type.Coding != null)
                {
                    device.TypeCode = source.Type.Coding.FirstOrDefault().Code;
                    device.TypeDisplay = source.Type.Coding.FirstOrDefault().Display;
                    device.TypeSystem = source.Type.Coding.FirstOrDefault().System;
                }
                if (source.Type.Text != null)
                    device.TypeText = source.Type.Text;
            }

            // Device Status
            var status = source.Status.GetValueOrDefault();
            switch (status)
            {
                case DeviceStatus.Available:
                    device.Status = DevStatus.available;
                    break;
                case DeviceStatus.NotAvailable:
                    device.Status = DevStatus.not_available;
                    break;
                case DeviceStatus.EnteredInError:
                    device.Status = DevStatus.entered_in_error;
                    break;
                default:
                    device.Status = DevStatus.entered_in_error;
                    break;
            }

            // Device Other details
            if (source.Manufacturer != null)
                device.Manufacturer = source.Manufacturer;
            if (source.Model != null)
                device.Model = source.Model;
            if (source.Udi != null)
                device.Udi = source.Udi;
            if (source.Expiry != null)
                device.Expiry = source.Expiry;
            if (source.LotNumber != null)
                device.LotNumber = source.LotNumber;
            if (source.Patient != null)
                device.PatientReference = source.Patient.Reference;

            return device;
        }

        /// <summary>
        /// Given a device POCO, maps the data to a device Resource.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Device MapModel(POCO.Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var resource = new Device();

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
                case DevStatus.available:
                    resource.Status = DeviceStatus.Available;
                    break;
                case DevStatus.not_available:
                    resource.Status = DeviceStatus.NotAvailable;
                    break;
                case DevStatus.entered_in_error:
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