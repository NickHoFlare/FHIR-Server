using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using ServerExperiment.Models.FHIR.Mappers;
using ServerExperiment.Models.POCO;
using ServerExperiment.Models;
using ServerExperiment.Utils;
using ServerExperiment.Models.Repository;

namespace ServerExperiment.Controllers.FhirControllers
{
    public class DeviceController : ApiController
    {
        private DeviceRepository repository = new DeviceRepository();

        // GET: fhir/Device/5
        [Route("fhir/Device/{deviceId}")]
        [HttpGet]
        [RequireHttps]
        public HttpResponseMessage Read(int deviceId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Device device = repository.GetDeviceByID(deviceId);
            if (device == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("Device with id " + deviceId + " not found!", Encoding.UTF8, "text/html");
                return message;
            }
            else if (device.IsDeleted == true)
            {
                message.StatusCode = HttpStatusCode.Gone;
                message.Content = new StringContent("Device with id " + deviceId + " has been deleted!", Encoding.UTF8, "text/html");
                return message;
            }

            Hl7.Fhir.Model.Device fhirDevice = DeviceMapper.MapModel(device);

            DeviceRecord record = repository.GetLatestRecord(deviceId);

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
            repository.UpdateDevice(device);

            DeviceRecord record = repository.GetLatestRecord(deviceId);
            repository.AddUpdateRecord(device, record);

            try
            {
                repository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(deviceId))
                {
                    message.StatusCode = HttpStatusCode.NotFound;
                    message.Content = new StringContent("Device with id " + deviceId + " not found!", Encoding.UTF8, "text/html");
                    return message;
                }
                else
                {
                    throw;
                }
            }

            message.StatusCode = HttpStatusCode.OK;
            message.Content = new StringContent("Device with id " + deviceId + " has been modified!", Encoding.UTF8, "text/html");
            return message;
        }

        // POST: fhir/Device
        [Route("fhir/Device")]
        [HttpPost]
        [RequireHttps]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Device fhirDevice)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Device device = DeviceMapper.MapResource(fhirDevice);
            repository.AddDevice(device);
            repository.Save();

            DeviceRecord record = new DeviceRecord();
            repository.AddCreateRecord(device, record);
            repository.Save();

            message.Content = new StringContent("Device created!", Encoding.UTF8, "text/html");
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

            Device device = repository.GetDeviceByID(deviceId);
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

            repository.DeleteDevice(device);

            DeviceRecord record = repository.GetLatestRecord(deviceId);
            repository.AddDeleteRecord(device, record);
            repository.Save();

            message.StatusCode = HttpStatusCode.NoContent;
            return message;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repository.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DeviceExists(int id)
        {
            return repository.DeviceExists(id);
        }
    }
}