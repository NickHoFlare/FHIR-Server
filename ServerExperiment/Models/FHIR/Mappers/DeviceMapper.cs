using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;

namespace ServerExperiment.Models.FHIR
{
    public class DeviceMapper
    {
        /// <summary>
        /// Given a Device Resource, maps the data in the resource to a Device POCO.
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

            patient.Active = source.Active ?? true;
            var deceased = source.Deceased as FhirBoolean;
            if (deceased != null)
                patient.Deceased = deceased.Value ?? false;

            // Patient name
            patient.FirstName = source.Name[0].Given.FirstOrDefault();
            patient.LastName = source.Name[0].Family.FirstOrDefault();

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
            var addLine1 = source.Address[0].LineElement[0].Value;
            var addLine2 = source.Address[0].LineElement[1].Value;
            var city = source.Address[0].City;
            var district = source.Address[0].District;
            var state = source.Address[0].State;
            var postalCode = source.Address[0].PostalCode;
            var periodStart = source.Address[0].Period.Start;
            var periodEnd = source.Address[0].Period.End;
            patient.AddressLine1 = addLine1;
            patient.AddressLine2 = addLine2;
            patient.City = city;
            patient.District = district;
            patient.State = state;
            patient.PeriodStart = periodStart;
            patient.PeriodEnd = periodEnd;

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

            resource.Id = patient.PatientId.ToString("D");

            resource.Active = patient.Active;
            resource.Deceased = new FhirBoolean(patient.Deceased);

            resource.Name = new List<HumanName>();
            var name = new HumanName()
            {
                Family = new[] { patient.LastName },
                Given = new[] { patient.FirstName },
                Use = HumanName.NameUse.Official
            };
            resource.Name.Add(name);

            resource.BirthDate = patient.Birthday.ToString("s");

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

            resource.Address = new List<Address>
            {
                new Address()
                {
                    Country = patient.Country,
                    City = patient.City,
                    District = patient.District,
                    State = patient.State,
                    PostalCode = patient.PostalCode,
                    Line = new[]
                    {
                        patient.AddressLine1,
                        patient.AddressLine2
                    },
                    Period = new Period
                    {
                        Start = patient.PeriodStart,
                        End = patient.PeriodEnd
                    }
                }
            };

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