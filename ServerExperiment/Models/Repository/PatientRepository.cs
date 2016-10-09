using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Data.Entity;
using System.Linq;
using Hl7.Fhir.Model;
using Observation = ServerExperiment.Models.POCO.Observation;
using Patient = ServerExperiment.Models.POCO.Patient;

namespace ServerExperiment.Models.Repository
{
    public class PatientRepository : IDisposable, IPatientRepository
    {
        private FhirResourceContext db = new FhirResourceContext();

        public IResource GetResourceByID(int patientId)
        {
            return db.Patients.FirstOrDefault(p => p.PatientId == patientId);
        }

        public void AddResource(IResource patient)
        {
            patient.IsDeleted = false;

            db.Patients.Add((Patient)patient);
        }

        public void UpdateResource(IResource patient)
        {
            db.Entry(patient).State = EntityState.Modified;
        }

        public void DeleteResource(IResource patient)
        {
            patient.IsDeleted = true;

            db.Entry(patient).State = EntityState.Modified;
        }

        public bool ResourceExists(int patientId)
        {
            return db.Patients.Count(e => e.PatientId == patientId) > 0;
        }

        public IRecord GetLatestRecord(int patientId)
        {
            return db.PatientRecords.Where(rec => rec.PatientId == patientId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public IRecord AddCreateRecord(IResource patient)
        {
            var patientRecord = new PatientRecord();

            patientRecord = (PatientRecord)ControllerUtils.AddMetadata(patientRecord, ControllerUtils.CREATE);
            patientRecord.Patient = (Patient)patient;

            db.PatientRecords.Add(patientRecord);

            return patientRecord;
        }

        public IRecord AddUpdateRecord(IResource patient, IRecord record)
        {
            var patientRecord = (PatientRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            patientRecord.Patient = (Patient)patient;

            db.PatientRecords.Add(patientRecord);

            return patientRecord;
        }

        public IRecord AddDeleteRecord(IResource patient, IRecord record)
        {
            var patientRecord = (PatientRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            patientRecord.Patient = (Patient)patient;

            db.PatientRecords.Add(patientRecord);

            return patientRecord;
        }

        public Resource AddMetadata(IResource resource, Resource fhirPatient, IRecord record)
        {
            Patient patient = (Patient)resource;
            fhirPatient.Id = patient.PatientId.ToString();
            fhirPatient.Meta = new Meta
            {
                ElementId = patient.PatientId.ToString(),
                VersionId = record.VersionId.ToString(),
                LastUpdated = record.LastModified
            };

            return fhirPatient;
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