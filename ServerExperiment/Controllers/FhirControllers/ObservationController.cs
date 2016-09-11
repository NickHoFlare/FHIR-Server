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

        // PUT: api/Observation/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutObservation(int id, Observation observation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != observation.ObservationId)
            {
                return BadRequest();
            }

            db.Entry(observation).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObservationExists(id))
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

        // POST: api/Observation
        [ResponseType(typeof(Observation))]
        public IHttpActionResult PostObservation(Observation observation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Observations.Add(observation);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = observation.ObservationId }, observation);
        }

        // DELETE: api/Observation/5
        [ResponseType(typeof(Observation))]
        public IHttpActionResult DeleteObservation(int id)
        {
            Observation observation = db.Observations.Find(id);
            if (observation == null)
            {
                return NotFound();
            }

            db.Observations.Remove(observation);
            db.SaveChanges();

            return Ok(observation);
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