using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ServerExperiment.Models;
using Hl7.Fhir.Serialization;
using ServerExperiment.Models.FHIR.Mappers;
using System.Text;
using ServerExperiment.Controllers.FhirControllers;

namespace ServerExperiment.Controllers
{
    public class PatientController : ApiController
    {
        private FhirResourceContext db = new FhirResourceContext();
        
        // GET: fhir/Patient
        // This will not exist in final FHIR implementation.
        public IQueryable<Patient> GetPatients()
        {
            return db.Patients;
        }

        // GET: fhir/Patient/5
        [Route("fhir/Patient/{patientId}")]
        [HttpGet]
        public HttpResponseMessage Read(int patientId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("Patient with id "+patientId+" not found!", Encoding.UTF8, "text/html");
                return message;
            }

            Hl7.Fhir.Model.Patient fhirPatient = PatientMapper.MapModel(patient);
            string fixedFormat = ControllerUtils.FixMimeString(_format);

            string payload = ControllerUtils.Serialize(fhirPatient, fixedFormat, _summary);

            message.Content = new StringContent(payload, Encoding.UTF8, fixedFormat);
            return message;
        }

        // PUT: fhir/Patient/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPatient(int id, Patient patient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != patient.PatientId)
            {
                return BadRequest();
            }

            db.Entry(patient).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(id))
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

        // POST: fhir/Patient
        [Route("fhir/Patient")]
        [HttpPost]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Patient fhirPatient)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Models.Patient patient = PatientMapper.MapResource(fhirPatient);

            db.Patients.Add(patient);
            db.SaveChanges();

            message.Content = new StringContent("Patient created!", Encoding.UTF8, "text/html");
            message.StatusCode = HttpStatusCode.Created;
            message.Headers.Location = new Uri(Url.Link("SpecificPatient", new { id = patient.PatientId }));

            return message;
        }

        // DELETE: fhir/Patient/5
        [Route("fhir/Patient/{patientId}")]
        [HttpDelete]
        public HttpResponseMessage Delete(int patientId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Patient patient = db.Patients.Find(patientId);
            if (patient == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("Patient with id " + patientId + " not found!", Encoding.UTF8, "text/html");
                return message;
            }

            db.Patients.Remove(patient);
            db.SaveChanges();

            message.StatusCode = HttpStatusCode.NoContent;
            return message;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PatientExists(int id)
        {
            return db.Patients.Count(e => e.PatientId == id) > 0;
        }
    }
}