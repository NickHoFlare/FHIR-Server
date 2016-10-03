using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Data.Entity;
using System.Linq;

namespace ServerExperiment.Models.Repository
{
    public class ObservationRepository : IDisposable, IObservationRepository
    {
        private FhirResourceContext _db = new FhirResourceContext();

        public IResource GetResourceByID(int observationId)
        {
            return _db.Observations.FirstOrDefault(p => p.ObservationId == observationId);
        }

        public void AddResource(IResource observation)
        {
            observation.IsDeleted = false;

            _db.Observations.Add((Observation)observation);
        }

        public void UpdateResource(IResource observation)
        {
            _db.Entry(observation).State = EntityState.Modified;
        }

        public void DeleteResource(IResource observation)
        {
            observation.IsDeleted = true;

            _db.Entry(observation).State = EntityState.Modified;
        }

        public bool ResourceExists(int observationId)
        {
            return _db.Observations.Count(e => e.ObservationId == observationId) > 0;
        }

        public IRecord GetLatestRecord(int observationId)
        {
            return _db.ObservationRecords.Where(rec => rec.ObservationId == observationId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public void AddCreateRecord(IResource observation)
        {
            var observationRecord = new ObservationRecord();

            observationRecord = (ObservationRecord)ControllerUtils.AddMetadata(observationRecord, ControllerUtils.CREATE);
            observationRecord.Observation = (Observation)observation;

            _db.ObservationRecords.Add(observationRecord);
        }

        public void AddUpdateRecord(IResource observation, IRecord record)
        {
            var observationRecord = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            observationRecord.Observation = (Observation)observation;

            _db.ObservationRecords.Add(observationRecord);
        }

        public void AddDeleteRecord(IResource observation, IRecord record)
        {
            var observationRecord = (ObservationRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            observationRecord.Observation = (Observation)observation;

            _db.ObservationRecords.Add(observationRecord);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        #region Dispose Code
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
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