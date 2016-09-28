using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.Repository
{
    public class ObservationRepository : IDisposable, IRepository
    {
        private FhirResourceContext db = new FhirResourceContext();

        public IResource GetResourceByID(int observationId)
        {
            return db.Observations.FirstOrDefault(p => p.ObservationId == observationId);
        }

        public void AddResource(IResource observation)
        {
            observation.IsDeleted = false;

            db.Observations.Add((Observation)observation);
        }

        public void UpdateResource(IResource observation)
        {
            db.Entry(observation).State = EntityState.Modified;
        }

        public void DeleteResource(IResource observation)
        {
            observation.IsDeleted = true;

            db.Entry(observation).State = EntityState.Modified;
        }

        public bool ResourceExists(int observationId)
        {
            return db.Observations.Count(e => e.ObservationId == observationId) > 0;
        }

        public IRecord GetLatestRecord(int observationId)
        {
            return db.ObservationRecords.Where(rec => rec.ObservationId == observationId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public void AddCreateRecord(IResource observation, IRecord record)
        {
            ObservationRecord observationRecord = (ObservationRecord)record;

            observationRecord = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.CREATE);
            observationRecord.Observation = (Observation)observation;

            db.ObservationRecords.Add(observationRecord);
        }

        public void AddUpdateRecord(IResource observation, IRecord record)
        {
            ObservationRecord observationRecord = (ObservationRecord)record;

            observationRecord = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            observationRecord.Observation = (Observation)observation;

            db.ObservationRecords.Add(observationRecord);
        }

        public void AddDeleteRecord(IResource observation, IRecord record)
        {
            ObservationRecord observationRecord = (ObservationRecord)record;

            observationRecord = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            observationRecord.Observation = (Observation)observation;

            db.ObservationRecords.Add(observationRecord);
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