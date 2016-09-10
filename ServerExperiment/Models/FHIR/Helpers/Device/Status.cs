using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.FHIR.Helpers.Device
{
    // available | not-available | entered-in-error
    public enum Status
    {
        available,
        not_available,
        entered_in_error
    }
}