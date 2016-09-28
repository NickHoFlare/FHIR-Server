using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.Repository
{
    public class PatientRepository : IDisposable
    {
        private FhirResourceContext db = new FhirResourceContext();

        public Patient GetPatientByID(int patientId)
        {
            return db.Patients.FirstOrDefault(p => p.PatientId == patientId);
        }

        public void AddPatient(Patient patient)
        {
            patient.IsDeleted = false;

            db.Patients.Add(patient);
        }

        public void UpdatePatient(Patient patient)
        {
            db.Entry(patient).State = EntityState.Modified;
        }

        public void DeletePatient(Patient patient)
        {
            patient.IsDeleted = true;

            db.Entry(patient).State = EntityState.Modified;
        }

        public bool PatientExists(int patientId)
        {
            return db.Patients.Count(e => e.PatientId == patientId) > 0;
        }

        public PatientRecord GetLatestRecord(int patientId)
        {
            return db.PatientRecords.Where(rec => rec.PatientId == patientId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public void AddCreateRecord(Patient patient, PatientRecord record)
        {
            record = (PatientRecord)ControllerUtils.AddMetadata(record, ControllerUtils.CREATE);
            record.Patient = patient;

            db.PatientRecords.Add(record);
        }

        public void AddUpdateRecord(Patient patient, PatientRecord record)
        {
            record = (PatientRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            record.Patient = patient;

            db.PatientRecords.Add(record);
        }

        public void AddDeleteRecord(Patient patient, PatientRecord record)
        {
            record = (PatientRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            record.Patient = patient;

            db.PatientRecords.Add(record);
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