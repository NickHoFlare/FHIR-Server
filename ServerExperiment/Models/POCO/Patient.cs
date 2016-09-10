using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models
{
	public class Patient
	{
        public Patient()
        {
            RecordNo = 0;
            Timestamp = DateTime.UtcNow;
            IsDeleted = false;

            PatientId = 0;
            Version = 0;
            Birthday = DateTime.UtcNow;
            Gender = GenderCode.Unknown;
            Active = true;
            Deceased = false;

            FirstNamesSerialised = "Unknown";

            _firstNames = new List<string>() { "Unknown" };
            _lastNames = new List<string>() { "Unknown2" };

            Address = new List<PatientAddressSet>(); // REMOVE ME LATER
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
        public int PatientId { get; set; }
        // Patient properties
        // EF DOES NOT SERIALISE/DESERIALISE COLLECTIONS FOR ME, MUST DO IT MYSELF
        private List<string> _firstNames;
        public List<string> FirstNames
        {
            get { return _firstNames; }
            set { _firstNames = value; }
        }
        public string FirstNamesSerialised
        {
            get { return String.Join(";", _firstNames); }
            set { _firstNames = value.Split(';').ToList(); }
        }

        private List<string> _lastNames;
        public List<string> LastNames
        {
            get { return _lastNames; }
            set { _lastNames = value; }
        }
        public string LastNamesSerialised
        {
            get { return String.Join(";", _lastNames); }
            set { _lastNames = value.Split(';').ToList(); }
        }

        public List<PatientAddressSet> Address { get; set; }

        public DateTime Birthday { get; set; }
        public GenderCode Gender { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }

        public bool Active { get; set; }
        public bool Deceased { get; set; }

        // Property that needs to be mapped to an extension, defintions for the nationalities can be found here:
        // http://www.englishclub.com/vocabulary/world-countries-nationality.htm
        //
        public string Nationality { get; set; }
    }
}