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
            LastName = "Unknown";
            Birthday = DateTime.UtcNow;
            Gender = GenderCode.Unknown;
            Active = true;
            Deceased = false;
        }

        public Patient(Patient patient)
        {
            RecordNo = 0;

            Action = patient.Action;
            Timestamp = patient.Timestamp;
            Version = patient.Version;
            IsDeleted = patient.IsDeleted;

            // Logical Identifier
            PatientId = patient.PatientId;

            // Patient properties
            FirstName = patient.FirstName;
            LastName = patient.LastName;

            Birthday = patient.Birthday;
            Gender = patient.Gender;

            Email = patient.Email;
            Phone = patient.Phone;
            Mobile = patient.Mobile;

            AddressLine1 = patient.AddressLine1;
            AddressLine2 = patient.AddressLine2;
            PostalCode = patient.PostalCode;
            City = patient.City;
            Country = patient.Country;
            District = patient.District;
            State = patient.State;
            PeriodStart = patient.PeriodStart;
            PeriodEnd = patient.PeriodEnd;

            Active = patient.Active;
            Deceased = patient.Deceased;
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
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public DateTime Birthday { get; set; }
        public GenderCode Gender { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string PeriodStart { get; set; }
        public string PeriodEnd { get; set; }

        public bool Active { get; set; }
        public bool Deceased { get; set; }

        // Property that needs to be mapped to an extension, defintions for the nationalities can be found here:
        // http://www.englishclub.com/vocabulary/world-countries-nationality.htm
        //
        public string Nationality { get; set; }
    }
}