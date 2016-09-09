using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models
{
	public class Device
	{
        public Device()
        {
            RecordNo = 0;
            Timestamp = DateTime.UtcNow;
            IsDeleted = false;

            DeviceId = 0;
            Version = 0;
        }

        public Device(Device device)
        {
            RecordNo = 0;

            Action = device.Action;
            Timestamp = device.Timestamp;
            Version = device.Version;
            IsDeleted = device.IsDeleted;

            // Logical Identifier
            DeviceId = device.DeviceId;

            // Device properties
            IdentifierType = device.IdentifierType;
            Identifier = device.Identifier;
            TypeLong = device.TypeLong;
            TypeShort = device.TypeShort;

            Note = device.Note;

            Status = device.Status;
            Manufacturer = device.Manufacturer;
            Model = device.Model;
            Expiry = device.Expiry;
            Udi = device.Udi;
            LotNumber = device.LotNumber;

            OrganisationReference = device.OrganisationReference;
            PatientReference = device.PatientReference;
        }

        // Each Record is immutable, in case of updates we create a new record and 
        // keep track of Version, Time of modification and action type like CREATE/UPDATE
        public int RecordNo { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public bool IsDeleted { get; set; }

        // Logical Identifier
        [Key]
        public int DeviceId { get; set; }

        // Device properties
        public string IdentifierType { get; set; }
        public string Identifier { get; set; }
        public string TypeLong { get; set; }
        public string TypeShort { get; set; }

        public string Note { get; set; }
        
        public string Status { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Expiry { get; set; }
        public string Udi { get; set; }
        public string LotNumber { get; set; }

        public string OrganisationReference { get; set; }
        public string PatientReference { get; set; }
    }
}