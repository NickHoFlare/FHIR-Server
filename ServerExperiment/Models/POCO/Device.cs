using ServerExperiment.Models.FHIR.Helpers.Device;
using System.ComponentModel.DataAnnotations;

namespace ServerExperiment.Models.POCO
{
    public class Device : IResource
	{
        public Device()
        {
            VersionId = 0;
            IsDeleted = false;

            DeviceId = 0;
        }

        // Each Record is immutable, in case of updates we create a new record and 
        // keep track of Version, Time of modification and action type like CREATE/UPDATE
        public int VersionId { get; set; }
        public bool IsDeleted { get; set; }

        // Logical Identifier
        [Key]
        public int DeviceId { get; set; }

        // Device properties
        public string TypeSystem { get; set; }
        public string TypeCode { get; set; }
        public string TypeDisplay { get; set; }
        public string TypeText { get; set; }
        
        public DevStatus Status { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Expiry { get; set; }
        public string Udi { get; set; }
        public string LotNumber { get; set; }

        public string PatientReference { get; set; }
    }
}