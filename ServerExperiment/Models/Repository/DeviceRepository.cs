using ServerExperiment.Controllers.FhirControllers;
using ServerExperiment.Models.POCO;
using System;
using System.Data.Entity;
using System.Linq;
using Hl7.Fhir.Model;
using Device = ServerExperiment.Models.POCO.Device;

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
            // If a resources is "deleted" yet we update it, we effectively "undelete" it.
            device.IsDeleted = false;

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

        public IRecord AddCreateRecord(IResource device)
        {
            var deviceRecord = new DeviceRecord();

            deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(deviceRecord, ControllerUtils.CREATE);
            deviceRecord.Device = (Device)device;

            _db.DeviceRecords.Add(deviceRecord);

            return deviceRecord;
        }

        public IRecord AddUpdateRecord(IResource device, IRecord record)
        {
            var deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.UPDATE);
            deviceRecord.Device = (Device)device;

            _db.DeviceRecords.Add(deviceRecord);

            return deviceRecord;
        }

        public IRecord AddDeleteRecord(IResource device, IRecord record)
        {
            var deviceRecord = (DeviceRecord)ControllerUtils.AddMetadata(record, ControllerUtils.DELETE);
            deviceRecord.Device = (Device)device;

            _db.DeviceRecords.Add(deviceRecord);

            return deviceRecord;
        }

        public Resource AddMetadata(IResource resource, Resource fhirDevice, IRecord record)
        {
            Device device = (Device)resource;
            fhirDevice.Id = device.DeviceId.ToString();
            fhirDevice.Meta = new Meta
            {
                ElementId = device.DeviceId.ToString(),
                VersionId = record.VersionId.ToString(),
                LastUpdated = record.LastModified
            };

            return fhirDevice;
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