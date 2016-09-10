using ServerExperiment.Models.FHIR.Helpers.Device;
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
        public string TypeSystem { get; set; }
        public string TypeCode { get; set; }
        public string TypeDisplay { get; set; }
        public string TypeText { get; set; }
        
        public Status Status { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Expiry { get; set; }
        public string Udi { get; set; }
        public string LotNumber { get; set; }

        public string PatientReference { get; set; }
    }
}