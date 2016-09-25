using ServerExperiment.Controllers.FhirControllers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.POCO
{
	public class ObservationRecord : IRecord
	{
        public ObservationRecord()
        {
            RecordId = 0;
            VersionId = 0;
            LastModified = DateTime.UtcNow;
            Action = ControllerUtils.UNASSIGNED;
        }

        public ObservationRecord(int recordId, int versionId, DateTime lastModified, string action, bool isDeleted)
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

        // Foreign key to a observation POCO model
        public int ObservationId { get; set; }

        [ForeignKey("ObservationId")]
        public Observation Observation { get; set; }

        // Record metadata
        public int VersionId { get; set; }
        public DateTime LastModified { get; set; }
        public string Action { get; set; }
    }
}