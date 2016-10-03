using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Data.Entity;
using System.Linq;

namespace ServerExperiment.Models.Repository
{
    public class DeviceRepository : IDisposable, IDeviceRepository
    {
        private FhirResourceContext _db = new FhirResourceContext();

        public IResource GetResourceByID(int deviceId)
        {
            return _db.Devices.FirstOrDefault(p => p.DeviceId == deviceId);
        }

        public void AddResource(IResource device)
        {
            device.IsDeleted = false;

            _db.Devices.Add((Device)device);
        }

        public void UpdateResource(IResource device)
        {
            _db.Entry(device).State = EntityState.Modified;
        }

        public void DeleteResource(IResource device)
        {
            device.IsDeleted = true;

            _db.Entry(device).State = EntityState.Modified;
        }

        public bool ResourceExists(int deviceId)
        {
            return _db.Devices.Count(e => e.DeviceId == deviceId) > 0;
        }

        public IRecord GetLatestRecord(int deviceId)
        {
            return _db.DeviceRecords.Where(rec => rec.DeviceId == deviceId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public void AddCreateRecord(IResource device, IRecord record)
        {
            var deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.CREATE);
            deviceRecord.Device = (Device)device;

            _db.DeviceRecords.Add(deviceRecord);
        }

        public void AddUpdateRecord(IResource device, IRecord record)
        {
            var deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            deviceRecord.Device = (Device)device;

            _db.DeviceRecords.Add(deviceRecord);
        }

        public void AddDeleteRecord(IResource device, IRecord record)
        {
            var deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            deviceRecord.Device = (Device)device;

            _db.DeviceRecords.Add(deviceRecord);
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