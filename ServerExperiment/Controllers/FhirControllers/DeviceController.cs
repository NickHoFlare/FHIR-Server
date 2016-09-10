using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ServerExperiment.Models;
using System.Text;
using ServerExperiment.Models.FHIR.Mappers;

namespace ServerExperiment.Controllers.FhirControllers
{
    public class DeviceController : ApiController
    {
        private FhirResourceContext db = new FhirResourceContext();

        // GET: fhir/Device
        // This will not exist in final FHIR implementation.
        public IQueryable<Device> GetDevices()
        {
            return db.Devices;
        }

        // GET: fhir/Device/5
        [Route("fhir/Device/{deviceId}")]
        [HttpGet]
        public HttpResponseMessage Read(int deviceId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Device device = db.Devices.Find(deviceId);
            if (device == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("Device with id " + deviceId + " not found!", Encoding.UTF8, "text/html");
                return message;
            }

            Hl7.Fhir.Model.Device fhirDevice = DeviceMapper.MapModel(device);
            string fixedFormat = ControllerUtils.FixMimeString(_format);

            string payload = ControllerUtils.Serialize(fhirDevice, fixedFormat, _summary);

            message.Content = new StringContent(payload, Encoding.UTF8, fixedFormat);
            return message;
        }

        // PUT: api/Device/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDevice(int id, Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != device.DeviceId)
            {
                return BadRequest();
            }

            db.Entry(device).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Device
        [ResponseType(typeof(Device))]
        public IHttpActionResult PostDevice(Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Devices.Add(device);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = device.DeviceId }, device);
        }

        // DELETE: api/Device/5
        [ResponseType(typeof(Device))]
        public IHttpActionResult DeleteDevice(int id)
        {
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return NotFound();
            }

            db.Devices.Remove(device);
            db.SaveChanges();

            return Ok(device);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DeviceExists(int id)
        {
            return db.Devices.Count(e => e.DeviceId == id) > 0;
        }
    }
}