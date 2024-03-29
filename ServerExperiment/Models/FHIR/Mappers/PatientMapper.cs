﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Hl7.Fhir.Model;
using ServerExperiment.Models.FHIR.Helpers.Patient;

namespace ServerExperiment.POCO.FHIR.Mappers
{
    public class PatientMapper
    {
        /// <summary>
        /// Given a Patient Resource, maps the data in the resource to a Patient POCO.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static Models.POCO.Patient MapResource(Resource resource)
        {
            var source = resource as Patient;
            if (source == null)
            {
                throw new ArgumentException("Resource in not a HL7 FHIR Patient resouce");
            }

            Models.POCO.Patient patient = new Models.POCO.Patient();

            int resultId = 0;
            int.TryParse(resource.Id, out resultId);
            patient.PatientId = resultId;

            patient.Active = source.Active ?? true; // Set to value of source.Active. if null, set to true.
            var deceased = source.Deceased as FhirBoolean;
            if (deceased != null)
                patient.Deceased = deceased.Value ?? false;

            // Patient names
            List<string> firstNames = new List<string>();
            List<string> lastNames = new List<string>();
            
            foreach (var name in source.Name)
            {
                var firstName = name.Given.FirstOrDefault();
                var lastName = name.Family.FirstOrDefault();

                if (firstName != null)
                    firstNames.Add(firstName);
                if (lastName != null)
                    lastNames.Add(lastName);
            }

            patient.FirstNames = firstNames;
            patient.LastNames = lastNames;

            // Patient Contact info
            var phone = source.Telecom.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Phone && t.Use == ContactPoint.ContactPointUse.Home); // Get contact method t where t is phone and of type home.
            if (phone != null)
                patient.Phone = phone.Value;

            var mobile = source.Telecom.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Phone && t.Use == ContactPoint.ContactPointUse.Mobile);
            if (mobile != null)
                patient.Mobile = mobile.Value;

            var email = source.Telecom.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Email && t.Use == ContactPoint.ContactPointUse.Home);
            if (email != null)
                patient.Email = email.Value;
            
            // Patient Gender
            var gender = source.Gender.GetValueOrDefault();
            switch (gender)
            {
                case AdministrativeGender.Unknown:
                    patient.Gender = GenderCode.Unknown;
                    break;
                case AdministrativeGender.Female:
                    patient.Gender = GenderCode.Female;
                    break;
                case AdministrativeGender.Male:
                    patient.Gender = GenderCode.Male;
                    break;
                case AdministrativeGender.Other:
                    patient.Gender = GenderCode.Undetermined;
                    break;
                default:
                    patient.Gender = GenderCode.Unknown;
                    break;
            }

            // Example for extension "nationality"
            var firstOrDefault = source.Extension.FirstOrDefault(x => x.Url == "http://www.englishclub.com/vocabulary/world-countries-nationality.htm");
            if (firstOrDefault != null)
            {
                var element = firstOrDefault.Value;
                var nationality = (FhirString)firstOrDefault.Value;
                patient.Nationality = nationality.Value;
            }

            // Patient birthday
            var birthday = source.BirthDate;
            if (birthday != null)
                patient.Birthday = DateTime.Parse(birthday);

            // Patient Address
            List<string> addressLines1 = new List<string>();
            List<string> addressLines2 = new List<string>();
            List<string> postalCodes = new List<string>();
            List<string> cities = new List<string>();
            List<string> countries = new List<string>();
            List<string> states = new List<string>();
            List<string> periodStarts = new List<string>();
            List<string> periodEnds = new List<string>();

            foreach (var address in source.Address)
            {
                string addLine1 = address.LineElement[0].Value;
                string addLine2 = string.Empty;
                if (address.LineElement.Count > 1)
                    addLine2 = address.LineElement[1].Value;
                string postalCode = address.PostalCode;
                string city = address.City;
                string country = address.Country;
                string state = address.State;
                var period = address.Period;
                string periodStart = string.Empty;
                string periodEnd = string.Empty;
                if (period != null)
                {
                    periodStart = address.Period.Start;
                    periodEnd = address.Period.End;
                }

                if (addLine1 != null)
                    addressLines1.Add(addLine1);
                if (addLine2 != null)
                    addressLines2.Add(addLine2);
                if (postalCode != null)
                    postalCodes.Add(postalCode);
                if (city != null)
                    cities.Add(city);
                if (country != null)
                    countries.Add(country);
                if (state != null)
                    states.Add(state);
                if (periodStart != null)
                    periodStarts.Add(periodStart);
                if (periodEnd != null)
                    periodEnds.Add(periodEnd);
            }
            
            patient.AddressLines1 = addressLines1;
            patient.AddressLines2 = addressLines2;
            patient.PostalCodes = postalCodes;
            patient.Cities = cities;
            patient.Countries = countries;
            patient.States = states;
            patient.PeriodStarts = periodStarts;
            patient.PeriodEnds = periodEnds;

            return patient;
        }

        /// <summary>
        /// Given a Patient POCO, maps the data to a Patient Resource.
        /// </summary>
        /// <param name="patient"></param>
        /// <returns></returns>
        public static Patient MapModel(Models.POCO.Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException("patient");
            }

            var resource = new Patient();

            resource.Id = patient.PatientId.ToString("D");

            // Patient bools
            resource.Active = patient.Active;
            resource.Deceased = new FhirBoolean(patient.Deceased);

            // Patient Names
            resource.Name = new List<HumanName>();
            List<HumanName> fhirNames = new List<HumanName>();

            for (int i = 0; i < patient.FirstNames.Count; i++)
            {
                HumanName fhirName = new HumanName()
                {
                    Family = new List<string>() { patient.FirstNames[i] },
                    Given = new List<string>() { patient.LastNames[i] },
                    Use = HumanName.NameUse.Official
                };
                fhirNames.Add(fhirName);
            }
            
            resource.Name = fhirNames;

            // Patient Birthday
            resource.BirthDate = patient.Birthday.ToString("s");

            // Patient Gender
            switch (patient.Gender)
            {
                case GenderCode.Female:
                    resource.Gender = AdministrativeGender.Female;
                    //resource.Gender = new CodeableConcept("http://hl7.org/fhir/v3/AdministrativeGender", "F", "Female");
                    break;

                case GenderCode.Male:
                    resource.Gender = AdministrativeGender.Male;
                    //resource.Gender = new CodeableConcept("http://hl7.org/fhir/v3/AdministrativeGender", "M", "Male");
                    break;

                case GenderCode.Undetermined:
                    resource.Gender = AdministrativeGender.Other;
                    //resource.Gender = new CodeableConcept("http://hl7.org/fhir/v3/AdministrativeGender", "U", "Undetermined");
                    break;

                default:
                    resource.Gender = AdministrativeGender.Unknown;
                    //resource.Gender = new CodeableConcept("http://hl7.org/fhir/v3/NullFlavor", "UNK", "Unknown");
                    break;
            }

            // Patient Telecom
            List<ContactPoint> contactPoints= new List<ContactPoint>();

            if (!patient.Phone.IsNullOrEmpty())
            {
                var phone = new ContactPoint
                {
                    Value = patient.Phone,
                    System = ContactPoint.ContactPointSystem.Phone,
                    Use = ContactPoint.ContactPointUse.Home
                };
                contactPoints.Add(phone);
            }
            if (!patient.Mobile.IsNullOrEmpty())
            {
                var mobile = new ContactPoint
                {
                    Value = patient.Mobile,
                    System = ContactPoint.ContactPointSystem.Phone,
                    Use = ContactPoint.ContactPointUse.Mobile
                };
                contactPoints.Add(mobile);
            }
            if (!patient.Email.IsNullOrEmpty())
            {
                var email = new ContactPoint()
                {
                    Value = patient.Email,
                    System = ContactPoint.ContactPointSystem.Email,
                    Use = ContactPoint.ContactPointUse.Home
                };
                contactPoints.Add(email);
            }

            if (!patient.Phone.IsNullOrEmpty() || !patient.Mobile.IsNullOrEmpty() || !patient.Email.IsNullOrEmpty())
            {
                resource.Telecom = contactPoints;
            }

            // Patient Address
            List<Address> fhirAddresses = null;

            if (!patient.AddressLines1.IsNullOrEmpty())
            {
                fhirAddresses = new List<Address>();

                for (int j = 0; j < patient.AddressLines1.Count; j++)
                {
                    Address fhirAddress = new Address();
                    if (!patient.Countries.IsNullOrEmpty())
                        fhirAddress.Country = patient.Countries[j];
                    if (!patient.Cities.IsNullOrEmpty())
                        fhirAddress.City = patient.Cities[j];
                    if (!patient.States.IsNullOrEmpty())
                        fhirAddress.State = patient.States[j];
                    if (!patient.PostalCodes.IsNullOrEmpty())
                        fhirAddress.PostalCode = patient.PostalCodes[j];
                    var addressLines = new List<string> {patient.AddressLines1[j]};
                    if (!patient.AddressLines2.IsNullOrEmpty())
                        addressLines.Add(patient.AddressLines2[j]);
                    fhirAddress.Line = addressLines;

                    if (!patient.PeriodStarts.IsNullOrEmpty() || !patient.PeriodEnds.IsNullOrEmpty())
                    {
                        Period period = new Period();
                        if (!patient.PeriodStarts.IsNullOrEmpty())
                            period.Start = patient.PeriodStarts[j];
                        if (!patient.PeriodEnds.IsNullOrEmpty())
                            period.End = patient.PeriodEnds[j];
                    }


                    fhirAddresses.Add(fhirAddress);
                }
            }

            resource.Address = fhirAddresses;

            // Make use of extensions ...
            //

            resource.Extension = new List<Extension>(1)
            {
                new Extension("http://www.englishclub.com/vocabulary/world-countries-nationality.htm",
                    new FhirString(patient.Nationality)
                )
            };
            return resource;
        }
    }
}