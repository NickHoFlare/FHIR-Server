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
        private PatientRepository patientRepository = new PatientRepository();

        // GET: fhir/Patient/5
        [Route("fhir/Patient/{patientId}")]
        [HttpGet]
        [RequireHttps]
        public HttpResponseMessage Read(int patientId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Patient patient = (Patient)patientRepository.GetResourceByID(patientId);
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

            PatientRecord record = (PatientRecord)patientRepository.GetLatestRecord(patientId);

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

            if (PatientExists(patientId))
            {
                PatientRecord record = (PatientRecord)patientRepository.GetLatestRecord(patientId);

                patientRepository.UpdateResource(patient);
                patientRepository.AddUpdateRecord(patient, record);

                patientRepository.Save(); // Look out for DbUpdateConcurrencyException

                message.StatusCode = HttpStatusCode.OK;
                message.Content = new StringContent("Patient with id " + patientId + " has been modified!", Encoding.UTF8, "text/html");
            }
            else
            {
                patientRepository.AddResource(patient);
                patientRepository.Save();

                PatientRecord record = new PatientRecord();
                patientRepository.AddCreateRecord(patient, record);
                patientRepository.Save();

                message.Content = new StringContent("Patient created!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.Created;
                message.Headers.Location = new Uri(Url.Link("SpecificPatient", new { id = patient.PatientId }));
            }

            return message;
        }

        // POST: fhir/Patient
        [Route("fhir/Patient")]
        [HttpPost]
        [RequireHttps]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Patient fhirPatient)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            if (fhirPatient.Id != null)
            {
                message.Content = new StringContent("Patient to be added should NOT already have a logical ID!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.BadRequest;
                return message;
            }

            Patient patient = PatientMapper.MapResource(fhirPatient);
            patientRepository.AddResource(patient);
            patientRepository.Save();

            PatientRecord record = new PatientRecord();
            patientRepository.AddCreateRecord(patient, record);
            patientRepository.Save();

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

            Patient patient = (Patient)patientRepository.GetResourceByID(patientId);
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

            patientRepository.DeleteResource(patient); // Does not actually delete from DB, simply flips isDeleted flag.

            PatientRecord record = (PatientRecord)patientRepository.GetLatestRecord(patientId);
            patientRepository.AddDeleteRecord(patient, record);
            patientRepository.Save();

            message.StatusCode = HttpStatusCode.NoContent;
            return message;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                patientRepository.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PatientExists(int id)
        {
            return patientRepository.ResourceExists(id);
        }
    }
}