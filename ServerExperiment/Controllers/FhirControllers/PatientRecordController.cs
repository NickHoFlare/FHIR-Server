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
    public class PatientRecordController : ApiController
    {
        private FhirResourceContext db = new FhirResourceContext();

        // GET: fhir/PatientRecord
        [Route("fhir/PatientRecord")]
        [HttpGet]
        [RequireHttps]
        public IQueryable<PatientRecord> GetPatientRecords()
        {
            return db.PatientRecords;
        }

        // GET: fhir/PatientRecord/5
        [Route("fhir/PatientRecord/{id}")]
        [HttpGet]
        [ResponseType(typeof(PatientRecord))]
        [RequireHttps]
        public IHttpActionResult GetPatientRecord(int id)
        {
            PatientRecord patientRecord = db.PatientRecords.Find(id);
            if (patientRecord == null)
            {
                return NotFound();
            }

            return Ok(patientRecord);
        }

        // Methods below are probably not going to be used.

        // PUT: fhir/PatientRecord/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPatientRecord(int id, PatientRecord patientRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != patientRecord.RecordId)
            {
                return BadRequest();
            }

            db.Entry(patientRecord).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientRecordExists(id))
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

        // POST: fhir/PatientRecord
        [ResponseType(typeof(PatientRecord))]
        public IHttpActionResult PostPatientRecord(PatientRecord patientRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.PatientRecords.Add(patientRecord);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = patientRecord.RecordId }, patientRecord);
        }

        // DELETE: fhir/PatientRecord/5
        [ResponseType(typeof(PatientRecord))]
        public IHttpActionResult DeletePatientRecord(int id)
        {
            PatientRecord patientRecord = db.PatientRecords.Find(id);
            if (patientRecord == null)
            {
                return NotFound();
            }

            db.PatientRecords.Remove(patientRecord);
            db.SaveChanges();

            return Ok(patientRecord);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PatientRecordExists(int id)
        {
            return db.PatientRecords.Count(e => e.RecordId == id) > 0;
        }
    }
}