using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerExperiment.Models.POCO
{
    public interface IRecord
    {
        int RecordId { get; set; }

        // Record metadata
        int VersionId { get; set; }
        DateTime LastModified { get; set; }
        string Action { get; set; }
    }
}
