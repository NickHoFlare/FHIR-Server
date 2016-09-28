using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Data.Entity;
using System.Linq;

namespace ServerExperiment.Models.Repository
{
    public class DeviceRepository : IDisposable
    {
        private FhirResourceContext db = new FhirResourceContext();

        public Device GetDeviceByID(int deviceId)
        {
            return db.Devices.FirstOrDefault(p => p.DeviceId == deviceId);
        }

        public void AddDevice(Device device)
        {
            device.IsDeleted = false;

            db.Devices.Add(device);
        }

        public void UpdateDevice(Device device)
        {
            db.Entry(device).State = EntityState.Modified;
        }

        public void DeleteDevice(Device device)
        {
            device.IsDeleted = true;

            db.Entry(device).State = EntityState.Modified;
        }

        public bool DeviceExists(int deviceId)
        {
            return db.Devices.Count(e => e.DeviceId == deviceId) > 0;
        }

        public DeviceRecord GetLatestRecord(int deviceId)
        {
            return db.DeviceRecords.Where(rec => rec.DeviceId == deviceId)
                                    .OrderByDescending(rec => rec.LastModified)
                                    .First();
        }

        public void AddCreateRecord(Device device, DeviceRecord record)
        {
            record = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.CREATE);
            record.Device = device;

            db.DeviceRecords.Add(record);
        }

        public void AddUpdateRecord(Device device, DeviceRecord record)
        {
            record = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            record.Device = device;

            db.DeviceRecords.Add(record);
        }

        public void AddDeleteRecord(Device device, DeviceRecord record)
        {
            record = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            record.Device = device;

            db.DeviceRecords.Add(record);
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