using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using ServerExperiment.Models.FHIR.Helpers.Patient;

namespace ServerExperiment.Models.FHIR.Mappers
{
    public class PatientMapper
    {
        /// <summary>
        /// Given a Patient Resource, maps the data in the resource to a Patient POCO.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static Models.Patient MapResource(Resource resource)
        {
            var source = resource as Hl7.Fhir.Model.Patient;
            if (source == null)
            {
                throw new ArgumentException("Resource in not a HL7 FHIR Patient resouce");
            }

            Models.Patient patient = new Models.Patient();

            patient.Active = source.Active ?? true; // Set to value of source.Active. if null, set to true.
            var deceased = source.Deceased as FhirBoolean;
            if (deceased != null)
                patient.Deceased = deceased.Value ?? false;

            // Patient names
            List<string> firstNames = new List<string>();
            List<string> lastNames = new List<string>();
            
            for (int i = 0; i < source.Name.Count; i++)
            {
                firstNames[i] = source.Name[i].Given.FirstOrDefault();
                lastNames[i] = source.Name[i].Family.FirstOrDefault();
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

            for (int i = 0; i < source.Address.Count; i++)
            {
                addressLines1[i] = source.Address[i].LineElement[0].Value;
                addressLines2[i] = source.Address[i].LineElement[1].Value;
                postalCodes[i] = source.Address[i].PostalCode;
                cities[i] = source.Address[i].City;
                countries[i] = source.Address[i].Country;
                states[i] = source.Address[i].State;
                periodStarts[i] = source.Address[i].Period.Start;
                periodEnds[i] = source.Address[i].Period.End;
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
        public static Hl7.Fhir.Model.Patient MapModel(Models.Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException("patient");
            }

            var resource = new Hl7.Fhir.Model.Patient();

            resource.Id = patient.PatientId.ToString("D"); // wtf does this line do

            // Patient bools
            resource.Active = patient.Active;
            resource.Deceased = new FhirBoolean(patient.Deceased);

            // Patient Names
            resource.Name = new List<HumanName>();
            List<HumanName> fhirNames = new List<HumanName>();

            for (int j = 0; j < patient.FirstNames.Count; j++)
            {
                HumanName fhirName = new HumanName()
                {
                    Family = new[] { patient.FirstNames[j] },
                    Given = new[] { patient.LastNames[j] },
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
            resource.Telecom = new List<ContactPoint>
            {
                new ContactPoint() {
                    Value = patient.Phone,
                    System = ContactPoint.ContactPointSystem.Phone,
                    Use = ContactPoint.ContactPointUse.Home
                },
                new ContactPoint() {
                    Value = patient.Mobile,
                    System = ContactPoint.ContactPointSystem.Phone,
                    Use = ContactPoint.ContactPointUse.Mobile
                },
                new ContactPoint() {
                    Value = patient.Email,
                    System = ContactPoint.ContactPointSystem.Email,
                    Use = ContactPoint.ContactPointUse.Home
                },
            };

            // Patient Address
            resource.Address = new List<Address>();
            List<Address> fhirAddresses = new List<Address>();

            for (int j = 0; j < patient.Countries.Count; j++)
            {
                Address fhirAddress = new Address()
                {
                    Country = patient.Countries[j],
                    City = patient.Cities[j],
                    State = patient.States[j],
                    PostalCode = patient.PostalCodes[j],
                    Line = new[]
                    {
                        patient.AddressLines1[j],
                        patient.AddressLines2[j],
                    },
                    Period = new Period
                    {
                        Start = patient.PeriodStarts[j],
                        End = patient.PeriodEnds[j],
                    }
                };

                fhirAddresses.Add(fhirAddress);
            }

            resource.Address = fhirAddresses;

            // Make use of extensions ...
            //
            resource.Extension = new List<Extension>(1);
            resource.Extension.Add(new Extension("http://www.englishclub.com/vocabulary/world-countries-nationality.htm",
                                                    new FhirString(patient.Nationality)
                                                    ));

            return resource;
        }
    }
}