using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.FHIR.Helpers.Observation
{
    // registered | preliminary | final | amended |
    public enum ObsStatus
    {
        registered,
        preliminary,
        final,
        amended,
        cancelled,
        entered_in_error,
        unknown
    }
}