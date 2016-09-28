using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.Repository
{
    public class ObservationRepository : IDisposable
    {
        private FhirResourceContext db = new FhirResourceContext();

        public Observation GetObservationByID(int observationId)
        {
            return db.Observations.FirstOrDefault(p => p.ObservationId == observationId);
        }

        public void AddObservation(Observation observation)
        {
            observation.IsDeleted = false;

            db.Observations.Add(observation);
        }

        public void UpdateObservation(Observation observation)
        {
            db.Entry(observation).State = EntityState.Modified;
        }

        public void DeleteObservation(Observation observation)
        {
            observation.IsDeleted = true;

            db.Entry(observation).State = EntityState.Modified;
        }

        public bool ObservationExists(int observationId)
        {
            return db.Observations.Count(e => e.ObservationId == observationId) > 0;
        }

        public ObservationRecord GetLatestRecord(int observationId)
        {
            return db.ObservationRecords.Where(rec => rec.ObservationId == observationId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public void AddCreateRecord(Observation observation, ObservationRecord record)
        {
            record = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.CREATE);
            record.Observation = observation;

            db.ObservationRecords.Add(record);
        }

        public void AddUpdateRecord(Observation observation, ObservationRecord record)
        {
            record = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            record.Observation = observation;

            db.ObservationRecords.Add(record);
        }

        public void AddDeleteRecord(Observation observation, ObservationRecord record)
        {
            record = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            record.Observation = observation;

            db.ObservationRecords.Add(record);
        }

        public void Save()
        {
            db.SaveChanges();
        }

        #region Dispose Code
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}