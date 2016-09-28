using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Data.Entity;
using System.Linq;

namespace ServerExperiment.Models.Repository
{
    public class DeviceRepository : IDisposable, IDeviceRepository
    {
        private FhirResourceContext db = new FhirResourceContext();

        public IResource GetResourceByID(int deviceId)
        {
            return db.Devices.FirstOrDefault(p => p.DeviceId == deviceId);
        }

        public void AddResource(IResource device)
        {
            device.IsDeleted = false;

            db.Devices.Add((Device)device);
        }

        public void UpdateResource(IResource device)
        {
            db.Entry(device).State = EntityState.Modified;
        }

        public void DeleteResource(IResource device)
        {
            device.IsDeleted = true;

            db.Entry(device).State = EntityState.Modified;
        }

        public bool ResourceExists(int deviceId)
        {
            return db.Devices.Count(e => e.DeviceId == deviceId) > 0;
        }

        public IRecord GetLatestRecord(int deviceId)
        {
            return db.DeviceRecords.Where(rec => rec.DeviceId == deviceId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public void AddCreateRecord(IResource device, IRecord record)
        {
            DeviceRecord deviceRecord = (DeviceRecord)record;

            deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.CREATE);
            deviceRecord.Device = (Device)device;

            db.DeviceRecords.Add(deviceRecord);
        }

        public void AddUpdateRecord(IResource device, IRecord record)
        {
            DeviceRecord deviceRecord = (DeviceRecord)record;

            deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            deviceRecord.Device = (Device)device;

            db.DeviceRecords.Add(deviceRecord);
        }

        public void AddDeleteRecord(IResource device, IRecord record)
        {
            DeviceRecord deviceRecord = (DeviceRecord)record;

            deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            deviceRecord.Device = (Device)device;

            db.DeviceRecords.Add(deviceRecord);
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