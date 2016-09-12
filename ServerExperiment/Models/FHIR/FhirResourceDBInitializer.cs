﻿using ServerExperiment.Models.FHIR.Helpers.Device;
using ServerExperiment.Models.FHIR.Helpers.Observation;
using ServerExperiment.Models.FHIR.Helpers.Patient;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models
{
    public class FhirResourceDBInitializer : DropCreateDatabaseAlways<FhirResourceContext>
    {
        protected override void Seed(FhirResourceContext context)
        {
            // Seed Patient data
            IList<Patient> defaultPatients = new List<Patient>();

            string firstNames = "Nicholas;DingJin";
            string lastNames = "Ho;He";

            string countries = "Australia;Singapore";
            string cities = "Sydney;Singapore";
            string states = "NSW;SG";
            string postalCodes = "2052;458968";
            string addLine1 = "123 Happy Road;456 Rainbow Road";
            string AddLine2 = "Black Building;Blue Building";

            defaultPatients.Add(new Patient());
            defaultPatients.Add(new Patient() {
                RecordNo = 1,
                Nationality = "Blablabla",
                Timestamp = DateTime.UtcNow,
                IsDeleted = false, Version = 1,
                FirstNamesSerialised = firstNames,
                LastNamesSerialised = lastNames,
                Birthday = DateTime.Parse("22/03/1991"),
                Gender = GenderCode.Male,
                CountriesSerialised = countries,
                CitiesSerialised = cities,
                StatesSerialised = states,
                PostalCodesSerialised = postalCodes,
                AddressLines1Serialised = addLine1,
                AddressLines2Serialised = AddLine2,
                PeriodStartsSerialised = ";", // This represents two empty strings
                PeriodEndsSerialised = ";",
                Active = true,
                Deceased = false
            });
            defaultPatients.Add(new Patient() { RecordNo = 2,
                Nationality = "Australian",
                Timestamp = DateTime.UtcNow,
                IsDeleted = false,
                Version = 1,
                FirstNamesSerialised = firstNames,
                LastNamesSerialised = lastNames,
                Birthday = DateTime.Parse("22/03/1991"),
                Phone = "12345678",
                Email = "email@email.com",
                Gender = GenderCode.Male,
                Active = true,
                Deceased = false
            });

            foreach (Patient patient in defaultPatients)
                context.Patients.Add(patient);

            // Seed Device data
            IList<Device> defaultDevices = new List<Device>();

            defaultDevices.Add(new Device());
            defaultDevices.Add(new Device()
            {
                TypeSystem = "http://snomed.info/sct",
                TypeCode = "86184003",
                TypeDisplay = "Electrocardiographic monitor and recorder",
                Status = DevStatus.available,
                Manufacturer = "ACME",
                Model = "D3AD-B33F",
                PatientReference = "Patient/2"
            });
            defaultDevices.Add(new Device()
            {
                TypeText = "Spirometer",
                Status = DevStatus.available,
                Manufacturer = "ACME",
                Model = "D3AD-B33F",
            });

            foreach (Device device in defaultDevices)
                context.Devices.Add(device);
            
            // Seed Observation data
            IList<Observation> defaultObservation = new List<Observation>();

            defaultObservation.Add(new Observation());
            defaultObservation.Add(new Observation()
            {
                Status = ObsStatus.registered,
                CategoryCodeSerialised = "12345678",
                CategorySystemSerialised = "http://acme.org/sct",
                CategoryDisplaySerialised = "Blood Pressure",
                CategoryText = "Blood P.",

                CodeCodeSerialised = "55284-4",
                CodeDisplaySerialised = "Blood pressure systolic &amp; diastolic",
                CodeSystemSerialised = "http://loinc.org",
                CodeText = "Some Text",

                PatientReference = "Patient/3",
                PerformerReferencesSerialised = "Patient/3",

                //EffectiveDateTime = DateTime.Now,
                //Issued = DateTime.Now,

                ValueQuantityValueSerialised = "107",
                ValueQuantityUnitSerialised = "mm[Hg]",

                Comments = "A Comment",

                BodySiteCode = "368209003",
                BodySiteDisplay = "Right Arm",
                BodySiteSystem = "http://acme.org/sct",
                BodySiteText = "Some Text",

                InterpretationCode = "L",
                InterpretationDisplay = "Below low normal",
                InterpretationSystem = "http://hl7.org/fhir/v2/0078",
                InterpretationText = "low"
            });

            foreach (Observation observation in defaultObservation)
                context.Observations.Add(observation);
            
            base.Seed(context);
        }
    }
}