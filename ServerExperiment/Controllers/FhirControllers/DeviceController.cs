using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Text;
using ServerExperiment.Models.FHIR.Mappers;
using ServerExperiment.Models.POCO;
using ServerExperiment.Utils;
using ServerExperiment.Models.Repository;

namespace ServerExperiment.Controllers.FhirControllers
{
    public class DeviceController : ApiController
    {
        //private DeviceRepository deviceRepository = new DeviceRepository();

        private readonly IDeviceRepository deviceRepository;

        public DeviceController(IDeviceRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            this.deviceRepository = repository;
        }

        // GET: fhir/Device/5
        [Route("fhir/Device/{deviceId}")]
        [HttpGet]
        [RequireHttps]
        public HttpResponseMessage Read(int deviceId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Device device = (Device)deviceRepository.GetResourceByID(deviceId);
            if (device == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("Device with id " + deviceId + " not found!", Encoding.UTF8, "text/html");
                return message;
            }
            if (device.IsDeleted)
            {
                message.StatusCode = HttpStatusCode.Gone;
                message.Content = new StringContent("Device with id " + deviceId + " has been deleted!", Encoding.UTF8, "text/html");
                return message;
            }

            Hl7.Fhir.Model.Device fhirDevice = DeviceMapper.MapModel(device);

            DeviceRecord record = (DeviceRecord)deviceRepository.GetLatestRecord(deviceId);

            string fixedFormat = ControllerUtils.FixMimeString(_format);
            string payload = ControllerUtils.Serialize(fhirDevice, fixedFormat, _summary);
            message.Content = new StringContent(payload, Encoding.UTF8, fixedFormat);
            message.Content.Headers.LastModified = record.LastModified;

            return message;
        }

        // PUT: fhir/Device/5
        [Route("fhir/Device/{deviceId}")]
        [HttpPut]
        [RequireHttps]
        public HttpResponseMessage Update(Hl7.Fhir.Model.Resource resource, int deviceId, string _format = "application/xml+FHIR", bool test = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();
            Hl7.Fhir.Model.Device fhirDevice = null;
            // Check that input resource is of type Device
            try
            {
                fhirDevice = (Hl7.Fhir.Model.Device)resource;
            }
            catch (Exception ex)
            {
                message.Content = new StringContent("Resource is of the wrong type, expecting Device!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.NotFound;
                return message;
            }
            // Check that input resource has a logical ID
            if (fhirDevice.Id == null)
            {
                message.Content = new StringContent("Device to be updated should have a logical ID!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.BadRequest;
                return message;
            }
            // Check that URL resource ID == input resource logical ID
            if (deviceId != int.Parse(fhirDevice.Id))
            {
                message.StatusCode = HttpStatusCode.BadRequest;
                message.Content = new StringContent("Mismatch of Device ID! Provided " + deviceId + " in URL but found " + fhirDevice.Id + "in payload!", Encoding.UTF8, "text/html");
                return message;
            }

            Device device = DeviceMapper.MapResource(fhirDevice);

            // Determine MediaTypeFormatter to use
            MediaTypeFormatter formatter = ControllerUtils.ChooseMediaTypeFormatter(_format);

            if (DeviceExists(deviceId))
            {
                DeviceRecord record = (DeviceRecord)deviceRepository.GetLatestRecord(deviceId);
                record = (DeviceRecord)deviceRepository.AddUpdateRecord(device, record);
                deviceRepository.UpdateResource(device);

                deviceRepository.Save();

                // Save metadata
                fhirDevice = (Hl7.Fhir.Model.Device)deviceRepository.AddMetadata(device, fhirDevice, record);

                message.StatusCode = HttpStatusCode.OK;
                message.Content = new ObjectContent(typeof(Hl7.Fhir.Model.Device), fhirDevice, formatter);
            }
            else
            {
                deviceRepository.AddResource(device);
                deviceRepository.Save();

                DeviceRecord record = (DeviceRecord)deviceRepository.AddCreateRecord(device);
                deviceRepository.Save();

                // Save metadata
                fhirDevice = (Hl7.Fhir.Model.Device)deviceRepository.AddMetadata(device, fhirDevice, record);

                // For testing purposes only.
                if (test)
                    device.DeviceId = 7357;

                message.Content = new ObjectContent(typeof(Hl7.Fhir.Model.Device), fhirDevice, formatter);
                message.StatusCode = HttpStatusCode.Created;
                message.Headers.Location = new Uri(Url.Link("SpecificDevice", new { id = device.DeviceId }));
            }

            return message;
        }

        // POST: fhir/Device
        [Route("fhir/Device")]
        [HttpPost]
        [RequireHttps]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Resource resource, string _format = "application/xml+FHIR", bool test = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Hl7.Fhir.Model.Device fhirDevice = null;
            try
            {
                fhirDevice = (Hl7.Fhir.Model.Device) resource;
            }
            catch (Exception ex)
            {
                message.Content = new StringContent("Resource is of the wrong type, expecting Device!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.NotFound;
                return message;
            }

            if (fhirDevice.Id != null)
            {
                message.Content = new StringContent("Device to be added should NOT already have a logical ID!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.BadRequest;
                return message;
            }

            Device device = DeviceMapper.MapResource(fhirDevice);
            deviceRepository.AddResource(device);
            deviceRepository.Save();

            DeviceRecord record = (DeviceRecord)deviceRepository.AddCreateRecord(device);
            deviceRepository.Save();

            // For testing purposes only.
            if (test && device.DeviceId == 0)
                device.DeviceId = 7357;

            // Save metadata
            fhirDevice = (Hl7.Fhir.Model.Device)deviceRepository.AddMetadata(device, fhirDevice, record);

            // Determine MediaTypeFormatter to use
            MediaTypeFormatter formatter = ControllerUtils.ChooseMediaTypeFormatter(_format);

            message.Content = new ObjectContent(typeof(Hl7.Fhir.Model.Device), fhirDevice, formatter);
            message.StatusCode = HttpStatusCode.Created;
            message.Headers.Location = new Uri(Url.Link("SpecificDevice", new { id = device.DeviceId }));

            return message;
        }

        // DELETE: fhir/Device/5
        [Route("fhir/Device/{deviceId}")]
        [HttpDelete]
        [RequireHttps]
        public HttpResponseMessage Delete(int deviceId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Device device = (Device)deviceRepository.GetResourceByID(deviceId);
            if (device == null)
            {
                message.StatusCode = HttpStatusCode.NoContent;
                return message;
            }
            if (device.IsDeleted)
            {
                message.StatusCode = HttpStatusCode.OK;
                message.ReasonPhrase = "Resource already deleted";
                return message;
            }

            deviceRepository.DeleteResource(device);

            DeviceRecord record = (DeviceRecord)deviceRepository.GetLatestRecord(deviceId);
            deviceRepository.AddDeleteRecord(device, record);
            deviceRepository.Save();

            message.StatusCode = HttpStatusCode.NoContent;
            return message;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                deviceRepository.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DeviceExists(int id)
        {
            return deviceRepository.ResourceExists(id);
        }
    }
}