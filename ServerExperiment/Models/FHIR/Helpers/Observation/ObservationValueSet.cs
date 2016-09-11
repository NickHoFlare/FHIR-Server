using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerExperiment.Models.FHIR.Helpers.Observation
{
    public struct ObservationValueSet
    {
        public CodeableConcept ComponentCode { get; set; }
        public Quantity ValueQuantity { get; set; }
        public CodeableConcept ValueCodeableConcept { get; set; }
        public string ValueString { get; set; }
        public Range ValueRange { get; set; }
        public SampledData ValueSampledData { get; set; }
    }
}