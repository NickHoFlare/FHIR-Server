using System;
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
    public class ObservationController : ApiController
    {
        private FhirResourceContext db = new FhirResourceContext();

        // GET: fhir/Observation
        // This will not exist in final FHIR implementation.
        public IQueryable<Observation> GetObservations()
        {
            return db.Observations;
        }

        // GET: fhir/Observation/5
        [Route("fhir/Observation/{observationId}")]
        [HttpGet]
        public HttpResponseMessage Read(int observationId, string _format = "application/xml+FHIR", bool _summary = false)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Observation observation = db.Observations.Find(observationId);
            if (observation == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("observation with id " + observationId + " not found!", Encoding.UTF8, "text/html");
                return message;
            }

            Hl7.Fhir.Model.Observation fhirObservation = ObservationMapper.MapModel(observation);
            string fixedFormat = ControllerUtils.FixMimeString(_format);

            string payload = ControllerUtils.Serialize(fhirObservation, fixedFormat, _summary);

            message.Content = new StringContent(payload, Encoding.UTF8, fixedFormat);
            return message;
        }

        // PUT: fhir/Observation/5
        [Route("fhir/Observation/{observationId}")]
        [HttpPut]
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

            db.Entry(observation).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObservationExists(observationId))
                {
                    message.StatusCode = HttpStatusCode.NotFound;
                    message.Content = new StringContent("observation with id " + observationId + " not found!", Encoding.UTF8, "text/html");
                    return message;
                }
                else
                {
                    throw;
                }
            }

            message.StatusCode = HttpStatusCode.NoContent;
            return message;
        }

        // POST: fhir/Observation
        [Route("fhir/Observation")]
        [HttpPost]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Observation fhirObservation)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Models.Observation observation = ObservationMapper.MapResource(fhirObservation);

            db.Observations.Add(observation);
            db.SaveChanges();

            message.Content = new StringContent("Observation created!", Encoding.UTF8, "text/html");
            message.StatusCode = HttpStatusCode.Created;
            message.Headers.Location = new Uri(Url.Link("SpecificObservation", new { id = observation.ObservationId }));

            return message;
        }

        // DELETE: fhir/Observation/5
        [Route("fhir/Observation/{observationId}")]
        [HttpDelete]
        public HttpResponseMessage Delete(int observationId)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Observation observation = db.Observations.Find(observationId);
            if (observation == null)
            {
                message.StatusCode = HttpStatusCode.NotFound;
                message.Content = new StringContent("Observation with id " + observationId + " not found!", Encoding.UTF8, "text/html");
                return message;
            }

            db.Observations.Remove(observation);
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

        private bool ObservationExists(int id)
        {
            return db.Observations.Count(e => e.ObservationId == id) > 0;
        }
    }
}