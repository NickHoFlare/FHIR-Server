using System;

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
