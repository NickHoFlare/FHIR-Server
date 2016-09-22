﻿using System;
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
using ServerExperiment.Models.POCO;

namespace ServerExperiment.Controllers.FhirControllers
{
    public class ObservationRecordController : ApiController
    {
        private FhirResourceContext db = new FhirResourceContext();

        // GET: api/ObservationRecord
        public IQueryable<ObservationRecord> GetObservationRecords()
        {
            return db.ObservationRecords;
        }

        // GET: api/ObservationRecord/5
        [ResponseType(typeof(ObservationRecord))]
        public IHttpActionResult GetObservationRecord(int id)
        {
            ObservationRecord observationRecord = db.ObservationRecords.Find(id);
            if (observationRecord == null)
            {
                return NotFound();
            }

            return Ok(observationRecord);
        }

        // PUT: api/ObservationRecord/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutObservationRecord(int id, ObservationRecord observationRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != observationRecord.RecordId)
            {
                return BadRequest();
            }

            db.Entry(observationRecord).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObservationRecordExists(id))
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

        // POST: api/ObservationRecord
        [ResponseType(typeof(ObservationRecord))]
        public IHttpActionResult PostObservationRecord(ObservationRecord observationRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ObservationRecords.Add(observationRecord);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = observationRecord.RecordId }, observationRecord);
        }

        // DELETE: api/ObservationRecord/5
        [ResponseType(typeof(ObservationRecord))]
        public IHttpActionResult DeleteObservationRecord(int id)
        {
            ObservationRecord observationRecord = db.ObservationRecords.Find(id);
            if (observationRecord == null)
            {
                return NotFound();
            }

            db.ObservationRecords.Remove(observationRecord);
            db.SaveChanges();

            return Ok(observationRecord);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ObservationRecordExists(int id)
        {
            return db.ObservationRecords.Count(e => e.RecordId == id) > 0;
        }
    }
}