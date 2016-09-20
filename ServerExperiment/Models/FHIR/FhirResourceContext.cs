using System.Data.Entity;
using ServerExperiment.Models.POCO;

namespace ServerExperiment.Models
{
    public class FhirResourceContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public FhirResourceContext() : base("name=FhirResourceContext")
        {
            Database.SetInitializer<FhirResourceContext>(new DropCreateDatabaseAlways<FhirResourceContext>());
        }

        public System.Data.Entity.DbSet<Patient> Patients { get; set; }
        public System.Data.Entity.DbSet<Device> Devices { get; set; }
        public System.Data.Entity.DbSet<Observation> Observations { get; set; }
    }
}
