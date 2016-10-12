using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using ServerExperiment.Models;
using ServerExperiment.Models.POCO;
using ServerExperiment.Utils;

namespace ServerExperiment.Controllers.FhirControllers
{
    public class DeviceRecordController : ApiController
    {
        private FhirResourceContext db = new FhirResourceContext();

        // GET: fhir/DeviceRecord
        [Route("fhir/DeviceRecord")]
        [HttpGet]
        [RequireHttps]
        public IQueryable<DeviceRecord> GetDeviceRecords()
        {
            return db.DeviceRecords;
        }

        // GET: fhir/DeviceRecord/5
        [Route("fhir/DeviceRecord/{id}")]
        [HttpGet]
        [ResponseType(typeof(DeviceRecord))]
        [RequireHttps]
        public IHttpActionResult GetDeviceRecord(int id)
        {
            DeviceRecord deviceRecord = db.DeviceRecords.Find(id);
            if (deviceRecord == null)
            {
                return NotFound();
            }

            return Ok(deviceRecord);
        }

        // Methods below are probably not going to be used.
/*
        // PUT: api/DeviceRecord/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDeviceRecord(int id, DeviceRecord deviceRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != deviceRecord.RecordId)
            {
                return BadRequest();
            }

            db.Entry(deviceRecord).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceRecordExists(id))
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

        // POST: api/DeviceRecord
        [ResponseType(typeof(DeviceRecord))]
        public IHttpActionResult PostDeviceRecord(DeviceRecord deviceRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.DeviceRecords.Add(deviceRecord);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = deviceRecord.RecordId }, deviceRecord);
        }

        // DELETE: api/DeviceRecord/5
        [ResponseType(typeof(DeviceRecord))]
        public IHttpActionResult DeleteDeviceRecord(int id)
        {
            DeviceRecord deviceRecord = db.DeviceRecords.Find(id);
            if (deviceRecord == null)
            {
                return NotFound();
            }

            db.DeviceRecords.Remove(deviceRecord);
            db.SaveChanges();

            return Ok(deviceRecord);
        }
*/
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DeviceRecordExists(int id)
        {
            return db.DeviceRecords.Count(e => e.RecordId == id) > 0;
        }
    }
}