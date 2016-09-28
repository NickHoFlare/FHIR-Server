using System;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using ServerExperiment.POCO.FHIR.Mappers;
using ServerExperiment.Utils;
using ServerExperiment.Models.Repository;

namespace ServerExperiment.Controllers
{
    public class PatientController : ApiController
    {
        private PatientRepository repository = new PatientRepository();

        // GET: fhir/Patient/5
        [Route("fhir/Patient/{patientId}")]
        [HttpGet]
        [RequireHttps]
        public HttpResponseMessage Read(int patientId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Patient patient = repository.GetPatientByID(patientId);
            if (patient == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("Patient with id "+patientId+" not found!", Encoding.UTF8, "text/html");
                return message;
            }
            else if (patient.IsDeleted == true)
            {
                message.StatusCode = HttpStatusCode.Gone;
                message.Content = new StringContent("Patient with id " + patientId + " has been deleted!", Encoding.UTF8, "text/html");
                return message;
            }

            Hl7.Fhir.Model.Patient fhirPatient = PatientMapper.MapModel(patient);

            PatientRecord record = repository.GetLatestRecord(patientId);

            string fixedFormat = ControllerUtils.FixMimeString(_format);
            string payload = ControllerUtils.Serialize(fhirPatient, fixedFormat, _summary);
            message.Content = new StringContent(payload, Encoding.UTF8, fixedFormat);
            message.Content.Headers.LastModified = record.LastModified;

            return message;
        }

        // PUT: fhir/Patient/5
        [Route("fhir/Patient/{patientId}")]
        [HttpPut]
        [RequireHttps]
        public HttpResponseMessage Update(Hl7.Fhir.Model.Patient fhirPatient, int patientId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            if (patientId != int.Parse(fhirPatient.Id))
            {
                message.StatusCode = HttpStatusCode.BadRequest;
                message.Content = new StringContent("Mismatch of patient ID! Provided " + patientId + " in URL but found " + fhirPatient.Id + "in payload!", Encoding.UTF8, "text/html");
                return message;
            }

            Patient patient = PatientMapper.MapResource(fhirPatient);
            repository.UpdatePatient(patient);

            PatientRecord record = repository.GetLatestRecord(patientId);
            repository.AddUpdateRecord(patient, record);

            try
            {
                repository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(patientId))
                {
                    message.StatusCode = HttpStatusCode.NotFound;
                    message.Content = new StringContent("Patient with id " + patientId + " not found!", Encoding.UTF8, "text/html");
                    return message;
                }
                else
                {
                    throw;
                }
            }

            message.StatusCode = HttpStatusCode.OK;
            message.Content = new StringContent("Patient with id " + patientId + " has been modified!", Encoding.UTF8, "text/html");
            return message;
        }

        // POST: fhir/Patient
        [Route("fhir/Patient")]
        [HttpPost]
        [RequireHttps]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Patient fhirPatient)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Patient patient = PatientMapper.MapResource(fhirPatient);
            repository.AddPatient(patient);
            repository.Save();

            PatientRecord record = new PatientRecord();
            repository.AddCreateRecord(patient, record);
            repository.Save();

            message.Content = new StringContent("Patient created!", Encoding.UTF8, "text/html");
            message.StatusCode = HttpStatusCode.Created;
            message.Headers.Location = new Uri(Url.Link("SpecificPatient", new { id = patient.PatientId }));

            return message;
        }

        // DELETE: fhir/Patient/5
        [Route("fhir/Patient/{patientId}")]
        [HttpDelete]
        [RequireHttps]
        public HttpResponseMessage Delete(int patientId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Patient patient = repository.GetPatientByID(patientId);
            if (patient == null)
            {
                message.StatusCode = HttpStatusCode.NoContent;
                return message;
            }
            else if (patient.IsDeleted == true)
            {
                message.StatusCode = HttpStatusCode.OK;
                return message;
            }

            repository.DeletePatient(patient); // Does not actually delete from DB, simply flips isDeleted flag.

            PatientRecord record = repository.GetLatestRecord(patientId);
            repository.AddDeleteRecord(patient, record);
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

        private bool PatientExists(int id)
        {
            return repository.PatientExists(id);
        }
    }
}