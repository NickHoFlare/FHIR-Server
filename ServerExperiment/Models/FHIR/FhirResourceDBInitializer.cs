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
            IList<Patient> defaultPatients = new List<Patient>();

            string firstNames = "Nicholas;DingJin";
            string lastNames = "Ho;He";

            defaultPatients.Add(new Patient());
            defaultPatients.Add(new Patient() { RecordNo = 1, Nationality = "Blablabla", Timestamp = DateTime.UtcNow, IsDeleted = false, Version = 1, FirstNamesSerialised = firstNames, LastNamesSerialised = lastNames, Birthday = DateTime.Parse("22/03/1991"), Gender = GenderCode.Male, Active = true, Deceased = false });
            defaultPatients.Add(new Patient() { RecordNo = 2, Nationality = "Australian", Timestamp = DateTime.UtcNow, IsDeleted = false, Version = 1, FirstNamesSerialised = firstNames, LastNamesSerialised = lastNames, Birthday = DateTime.Parse("22/03/1991"), Gender = GenderCode.Male, Active = true, Deceased = false });

            foreach (Patient patient in defaultPatients)
                context.Patients.Add(patient);

            base.Seed(context);
        }
    }
}