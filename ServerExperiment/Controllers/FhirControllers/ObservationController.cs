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
    public class ObservationController : ApiController
    {
        private ObservationRepository repository = new ObservationRepository();

        // GET: fhir/Observation/5
        [Route("fhir/Observation/{observationId}")]
        [HttpGet]
        [RequireHttps]
        public HttpResponseMessage Read(int observationId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Observation observation = repository.GetObservationByID(observationId);
            if (observation == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("observation with id " + observationId + " not found!", Encoding.UTF8, "text/html");
                return message;
            }
            else if (observation.IsDeleted == true)
            {
                message.StatusCode = HttpStatusCode.Gone;
                message.Content = new StringContent("Observation with id " + observationId + " has been deleted!", Encoding.UTF8, "text/html");
                return message;
            }

            Hl7.Fhir.Model.Observation fhirObservation = ObservationMapper.MapModel(observation);

            ObservationRecord record = repository.GetLatestRecord(observationId);

            string fixedFormat = ControllerUtils.FixMimeString(_format);
            string payload = ControllerUtils.Serialize(fhirObservation, fixedFormat, _summary);
            message.Content = new StringContent(payload, Encoding.UTF8, fixedFormat);
            message.Content.Headers.LastModified = record.LastModified;

            return message;
        }

        // PUT: fhir/Observation/5
        [Route("fhir/Observation/{observationId}")]
        [HttpPut]
        [RequireHttps]
        public HttpResponseMessage Update(Hl7.Fhir.Model.Observation fhirObservation, int observationId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            if (observationId != int.Parse(fhirObservation.Id))
            {
                message.StatusCode = HttpStatusCode.BadRequest;
                message.Content = new StringContent("Mismatch of observation ID! Provided " + observationId + " in URL but found " + fhirObservation.Id + "in payload!", Encoding.UTF8, "text/html");
                return message;
            }

            Observation observation = ObservationMapper.MapResource(fhirObservation);
            if (ObservationExists(observationId))
            {
                ObservationRecord record = repository.GetLatestRecord(observationId);

                repository.UpdateObservation(observation);
                repository.AddUpdateRecord(observation, record);

                repository.Save(); // Look out for DbUpdateConcurrencyException

                message.StatusCode = HttpStatusCode.OK;
                message.Content = new StringContent("Observation with id " + observationId + " has been modified!", Encoding.UTF8, "text/html");
            }
            else
            {
                repository.AddObservation(observation);
                repository.Save();

                ObservationRecord record = new ObservationRecord();
                repository.AddCreateRecord(observation, record);
                repository.Save();

                message.Content = new StringContent("Observation created!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.Created;
                message.Headers.Location = new Uri(Url.Link("SpecificObservation", new { id = observation.ObservationId }));
            }

            return message;
        }

        // POST: fhir/Observation
        [Route("fhir/Observation")]
        [HttpPost]
        [RequireHttps]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Observation fhirObservation)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            if (fhirObservation.Id != null || fhirObservation.Id != string.Empty)
            {
                message.Content = new StringContent("Observation to be added should NOT already have a logical ID!", Encoding.UTF8, "text/html");
                message.StatusCode = HttpStatusCode.BadRequest;
                return message;
            }

            Observation observation = ObservationMapper.MapResource(fhirObservation);
            repository.AddObservation(observation);
            repository.Save();

            ObservationRecord record = new ObservationRecord();
            repository.AddCreateRecord(observation, record);
            repository.Save();

            message.Content = new StringContent("Observation created!", Encoding.UTF8, "text/html");
            message.StatusCode = HttpStatusCode.Created;
            message.Headers.Location = new Uri(Url.Link("SpecificObservation", new { id = observation.ObservationId }));

            return message;
        }

        // DELETE: fhir/Observation/5
        [Route("fhir/Observation/{observationId}")]
        [HttpDelete]
        [RequireHttps]
        public HttpResponseMessage Delete(int observationId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Observation observation = repository.GetObservationByID(observationId);
            if (observation == null)
            {
                message.StatusCode = HttpStatusCode.NoContent;
                return message;
            }
            else if (observation.IsDeleted == true)
            {
                message.StatusCode = HttpStatusCode.OK;
                return message;
            }

            repository.DeleteObservation(observation); // Does not actually delete from DB, simply flips isDeleted flag.

            ObservationRecord record = repository.GetLatestRecord(observationId);
            repository.AddDeleteRecord(observation, record);
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

        private bool ObservationExists(int id)
        {
            return repository.ObservationExists(id);
        }
    }
}