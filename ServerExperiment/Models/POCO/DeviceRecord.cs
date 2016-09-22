using ServerExperiment.Controllers.FhirControllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.POCO
{
	public class DeviceRecord : IRecord
	{
        public DeviceRecord()
        {
            RecordId = 0;
            VersionId = 0;
            LastModified = DateTime.UtcNow;
            Action = ControllerUtils.UNASSIGNED;
        }

        public DeviceRecord(int recordId, int versionId, DateTime lastModified, string action, bool isDeleted)
        {
            RecordId = recordId;
            VersionId = versionId;
            lastModified = LastModified;
            Action = action;
        }
        
        // Each Record is immutable, in case of updates we create a new record and 
        // keep track of Version, Time of modification and action type like CREATE/UPDATE
        [Key]
        public int RecordId { get; set; }

        // Foreign key to a device POCO model
        public int DeviceId { get; set; }

        [ForeignKey("DeviceId")]
        public Device Device { get; set; }

        // Record metadata
        public int VersionId { get; set; }
        public DateTime LastModified { get; set; }
        public string Action { get; set; }
    }
}