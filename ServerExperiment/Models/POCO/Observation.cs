using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace ServerExperiment.Models
{
	public class Observation
	{
        public Observation()
        {
            RecordNo = 0;
            Timestamp = DateTime.UtcNow;
            IsDeleted = false;

            ObservationId = 0;
            Version = 0;
        }

        public Observation(Observation observation)
        {
            RecordNo = 0;

            Action = observation.Action;
            Timestamp = observation.Timestamp;
            Version = observation.Version;
            IsDeleted = observation.IsDeleted;

            // Logical Identifier
            ObservationId = observation.ObservationId;

            // observation properties

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
        public int ObservationId { get; set; }

        // observation properties
        public Identifier Identifier { get; set; }
        public string Status { get; set; }
        public CodeableConcept Category { get; set; }
        public CodeableConcept Code { get; set; }

        public string SubjectReference { get; set; }
        public string PerformerReference { get; set; }
        public string DeviceReference { get; set; }

        public DateTime EffectiveDateTime { get; set; }
        public Period EffectivePeriod { get; set; }
        public Instant Issued { get; set; }

        // Applicable to both individual ValueSet and Componentised Valuesets.
        public List<ValueSet> Values { get; set; }

        public CodeableConcept Interpretation { get; set; }

        public string comments { get; set; }

        public CodeableConcept BodySite { get; set; }
    }
}