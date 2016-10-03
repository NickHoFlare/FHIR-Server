using System;
using System.Net;
using System.Net.Http;
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
        public HttpResponseMessage Update(Hl7.Fhir.Model.Device fhirDevice, int deviceId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            if (deviceId != int.Parse(fhirDevice.Id))
            {
                message.StatusCode = HttpStatusCode.BadRequest;
                message.Content = new StringContent("Mismatch of Device ID! Provided " + deviceId + " in URL but found " + fhirDevice.Id + "in payload!", Encoding.UTF8, "text/html");
                return message;
            }

            Device device = DeviceMapper.MapResource(fhirDevice);

            if (DeviceExists(deviceId))
            {
                DeviceRecord record = (DeviceRecord)deviceRepository.GetLatestRecord(deviceId);

                deviceRepository.UpdateResource(device);
                deviceRepository.AddUpdateRecord(device, record);

                deviceRepository.Save();

                message.StatusCode = HttpStatusCode.OK;
                message.Content = new StringContent("Device with id " + deviceId + " has been modified!", Encoding.UTF8, "text/html");
            }
            else
            {
                deviceRepository.AddResource(device);
                deviceRepository.Save();

                deviceRepository.AddCreateRecord(device);
                deviceRepository.Save();

                message.Content = new StringContent("Device created with ID " + device.DeviceId + "!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.Created;
                message.Headers.Location = new Uri(Url.Link("SpecificDevice", new { id = device.DeviceId }));
            }

            return message;
        }

        // POST: fhir/Device
        [Route("fhir/Device")]
        [HttpPost]
        [RequireHttps]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Device fhirDevice)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            if (fhirDevice.Id != null)
            {
                message.Content = new StringContent("Device to be added should NOT already have a logical ID!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.BadRequest;
                return message;
            }

            Device device = DeviceMapper.MapResource(fhirDevice);
            deviceRepository.AddResource(device);
            deviceRepository.Save();

            deviceRepository.AddCreateRecord(device);
            deviceRepository.Save();

            message.Content = new StringContent("Device created with ID " + device.DeviceId + "!", Encoding.UTF8, "text/html");
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
            else if (device.IsDeleted == true)
            {
                message.StatusCode = HttpStatusCode.OK;
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