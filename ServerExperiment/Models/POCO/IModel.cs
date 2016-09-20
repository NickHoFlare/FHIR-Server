using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerExperiment.Models.POCO
{
    public interface IModel
    {
        // Each Record is immutable, in case of updates we create a new record and 
        // keep track of Version, Time of modification and action type like CREATE/UPDATE
        int RecordId { get; set; }
        int VersionId { get; set; }
        DateTime LastModified { get; set; }
        string Action { get; set; }
        bool IsDeleted { get; set; }
    }
}
