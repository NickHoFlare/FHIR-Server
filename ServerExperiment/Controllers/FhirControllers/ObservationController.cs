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

namespace ServerExperiment.Controllers.FhirControllers
{
    public class ObservationController : ApiController
    {
        private FhirResourceContext db = new FhirResourceContext();

        // GET: fhir/Observation/5
        [Route("fhir/Observation/{observationId}")]
        [HttpGet]
        [RequireHttps]
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
            else if (observation.IsDeleted == true)
            {
                message.StatusCode = HttpStatusCode.Gone;
                message.Content = new StringContent("Observation with id " + observationId + " has been deleted!", Encoding.UTF8, "text/html");
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

            db.Entry(observation).State = EntityState.Modified;
            //db.SaveChanges();

            ObservationRecord record = db.ObservationRecords.Where(rec => rec.ObservationId == observationId).OrderByDescending(rec => rec.LastModified).First();
            record = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            record.Observation = observation;

            db.ObservationRecords.Add(record);

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

            message.StatusCode = HttpStatusCode.OK;
            message.Content = new StringContent("Patient with id " + observationId + " has been modified!", Encoding.UTF8, "text/html");
            return message;
        }

        // POST: fhir/Observation
        [Route("fhir/Observation")]
        [HttpPost]
        [RequireHttps]
        public HttpResponseMessage Create(Hl7.Fhir.Model.Observation fhirObservation)
        {
            HttpResponseMessage message = new HttpResponseMessage();

            Observation observation = ObservationMapper.MapResource(fhirObservation);
            observation.IsDeleted = false;

            db.Observations.Add(observation);
            db.SaveChanges();

            ObservationRecord record = new ObservationRecord();
            record = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.CREATE);
            record.Observation = observation;

            db.ObservationRecords.Add(record);
            db.SaveChanges();

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

            Observation observation = db.Observations.Find(observationId);
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

            observation.IsDeleted = true;

            db.Entry(observation).State = EntityState.Modified;

            ObservationRecord record = db.ObservationRecords.Where(rec => rec.ObservationId == observationId).OrderByDescending(rec => rec.LastModified).First();
            record = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            record.Observation = observation;

            db.ObservationRecords.Add(record);
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