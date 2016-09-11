﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.Observation;
using ServerExperiment.Models.FHIR.Helpers.Observation;

namespace ServerExperiment.Models.FHIR.Mappers
{
    public class ObservationMapper
    {
        /// <summary>
        /// Given a Observation Resource, maps the data in the resource to a Observation POCO.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static Models.Observation MapResource(Resource resource)
        {
            var source = resource as Hl7.Fhir.Model.Observation;
            if (source == null)
            {
                throw new ArgumentException("Resource in not a HL7 FHIR observation resouce");
            }

            Models.Observation observation = new Observation();

            // observation Status
            var status = source.Status.GetValueOrDefault();
            switch (status)
            {
                case ObservationStatus.Registered:
                    observation.Status = ObsStatus.registered;
                    break;
                case ObservationStatus.Preliminary:
                    observation.Status = ObsStatus.preliminary;
                    break;
                case ObservationStatus.Final:
                    observation.Status = ObsStatus.final;
                    break;
                case ObservationStatus.Amended:
                    observation.Status = ObsStatus.amended;
                    break;
                case ObservationStatus.Cancelled:
                    observation.Status = ObsStatus.cancelled;
                    break;
                case ObservationStatus.Unknown:
                    observation.Status = ObsStatus.unknown;
                    break;
                case ObservationStatus.EnteredInError:
                    observation.Status = ObsStatus.entered_in_error;
                    break;
                default:
                    observation.Status = ObsStatus.entered_in_error;
                    break;
            }

            // Observation Category
            foreach (Coding coding in source.Category.Coding)
            {
                observation.CategoryCode.Add(coding.Code);
                observation.CategoryDisplay.Add(coding.Display);
                observation.CategorySystem.Add(coding.System);
            }
            observation.CategoryText = source.Category.Text;

            // Observation Code
            foreach (Coding coding in source.Code.Coding)
            {
                observation.CodeCode.Add(coding.Code);
                observation.CodeDisplay.Add(coding.Display);
                observation.CodeSystem.Add(coding.System);
            }
            observation.CodeText = source.Code.Text;

            // Observation references to other resources
            observation.PatientReference = source.Subject.Reference;
            observation.DeviceReference = source.Device.Reference;
            foreach (var reference in source.Performer)
            {
                observation.PerformerReferences.Add(reference.Reference);
            }

            // Observation effective times
            if (source.Effective != null)
            {
                if (source.Effective is FhirDateTime)
                {
                    var effective = source.Effective as FhirDateTime;
                    observation.EffectiveDateTime = DateTime.Parse(effective.Value);
                }
                else
                {
                    var effective = source.Effective as Period;
                    observation.EffectivePeriodStart = DateTime.Parse(effective.Start);
                    observation.EffectivePeriodEnd = DateTime.Parse(effective.End);
                }
            }

            // Observation Interpretation
            observation.InterpretationCode = source.Interpretation.Coding.FirstOrDefault().Code;
            observation.InterpretationDisplay = source.Interpretation.Coding.FirstOrDefault().Display;
            observation.InterpretationSystem = source.Interpretation.Coding.FirstOrDefault().System;
            observation.InterpretationText = source.Interpretation.Text;

            // Observation Comments
            observation.Comments = source.Comments;

            // Site of Body where Observation was made
            observation.BodySiteCode = source.BodySite.Coding.FirstOrDefault().Code;
            observation.BodySiteDisplay = source.BodySite.Coding.FirstOrDefault().Display;
            observation.BodySiteSystem = source.BodySite.Coding.FirstOrDefault().System;
            observation.BodySiteText = source.BodySite.Text;

            // Observation values / components
            // If only one value, then simply a value type. If more than one, then Component type.
            if (source.Component == null) // Must be a value
            {
                if (source.Value is Quantity)
                {
                    var value = source.Value as Quantity;
                    observation.ValueQuantityCode.Add(value.Code);
                    observation.ValueQuantitySystem.Add(value.System);
                    observation.ValueQuantityUnit.Add(value.Unit);
                    observation.ValueQuantityValue.Add((decimal)value.Value);
                }
                else if (source.Value is CodeableConcept)
                {
                    var value = source.Value as CodeableConcept;
                    observation.ValueSystem.Add(value.Coding.FirstOrDefault().System);
                    observation.ValueCode.Add(value.Coding.FirstOrDefault().Code);
                    observation.ValueDisplay.Add(value.Coding.FirstOrDefault().Display);
                    observation.ValueText.Add(value.Text);
                }
                else if (source.Value is FhirString)
                {
                    var value = source.Value as FhirString;
                    observation.ValueString.Add(value.Value);
                }
                else if (source.Value is SampledData)
                {
                    var value = source.Value as SampledData;
                    observation.ValueSampledDataOriginCode.Add(value.Origin.Code);
                    observation.ValueSampledDataOriginSystem.Add(value.Origin.System);
                    observation.ValueSampledDataOriginUnit.Add(value.Origin.Unit);
                    observation.ValueSampledDataOriginValue.Add((decimal)value.Origin.Value);
                    observation.ValueSampledDataPeriod.Add((decimal)value.Period);
                    observation.ValueSampledDataDimensions.Add((int)value.Dimensions);
                    observation.ValueSampledDataData.Add(value.Data);
                }
                else if (source.Value is Period)
                {
                    var value = source.Value as Period;
                    observation.ValuePeriodStart.Add(DateTime.Parse(value.Start));
                    observation.ValuePeriodEnd.Add(DateTime.Parse(value.End));
                }
            }
            else // Must be composite(s)
            {
                foreach (var component in source.Component)
                {
                    observation.ComponentCodeCode.Add(component.Code.Coding.FirstOrDefault().Code);
                    observation.ComponentCodeSystem.Add(component.Code.Coding.FirstOrDefault().System);
                    observation.ComponentCodeDisplay.Add(component.Code.Coding.FirstOrDefault().Display);
                    observation.ComponentCodeText = component.Code.Text;

                    if (component.Value is Quantity)
                    {
                        var value = source.Value as Quantity;
                        observation.ValueQuantityCode.Add(value.Code);
                        observation.ValueQuantitySystem.Add(value.System);
                        observation.ValueQuantityUnit.Add(value.Unit);
                        observation.ValueQuantityValue.Add((decimal)value.Value);
                    }
                    else if (component.Value is CodeableConcept)
                    {
                        var value = source.Value as CodeableConcept;
                        observation.ValueSystem.Add(value.Coding.FirstOrDefault().System);
                        observation.ValueCode.Add(value.Coding.FirstOrDefault().Code);
                        observation.ValueDisplay.Add(value.Coding.FirstOrDefault().Display);
                        observation.ValueText.Add(value.Text);
                    }
                    else if (component.Value is FhirString)
                    {
                        var value = source.Value as FhirString;
                        observation.ValueString.Add(value.Value);
                    }
                    else if (component.Value is SampledData)
                    {
                        var value = source.Value as SampledData;
                        observation.ValueSampledDataOriginCode.Add(value.Origin.Code);
                        observation.ValueSampledDataOriginSystem.Add(value.Origin.System);
                        observation.ValueSampledDataOriginUnit.Add(value.Origin.Unit);
                        observation.ValueSampledDataOriginValue.Add((decimal)value.Origin.Value);
                        observation.ValueSampledDataPeriod.Add((decimal)value.Period);
                        observation.ValueSampledDataDimensions.Add((int)value.Dimensions);
                        observation.ValueSampledDataData.Add(value.Data);
                    }
                    else if (component.Value is Period)
                    {
                        var value = source.Value as Period;
                        observation.ValuePeriodStart.Add(DateTime.Parse(value.Start));
                        observation.ValuePeriodEnd.Add(DateTime.Parse(value.End));
                    }
                }
            }

            return observation;
        }

        /// <summary>
        /// Given a observation POCO, maps the data to an Observation Resource.
        /// </summary>
        /// <param name="observation"></param>
        /// <returns></returns>
        public static Hl7.Fhir.Model.Observation MapModel(Models.Observation observation)
        {
            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            var resource = new Hl7.Fhir.Model.Observation();

            resource.Id = observation.ObservationId.ToString("D");

            // Observation Status
            switch (observation.Status)
            {
                case ObsStatus.amended:
                    resource.Status = ObservationStatus.Amended;
                    break;
                case ObsStatus.cancelled:
                    resource.Status = ObservationStatus.Cancelled;
                    break;
                case ObsStatus.entered_in_error:
                    resource.Status = ObservationStatus.EnteredInError;
                    break;
                case ObsStatus.final:
                    resource.Status = ObservationStatus.Final;
                    break;
                case ObsStatus.preliminary:
                    resource.Status = ObservationStatus.Preliminary;
                    break;
                case ObsStatus.registered:
                    resource.Status = ObservationStatus.Registered;
                    break;
                case ObsStatus.unknown:
                    resource.Status = ObservationStatus.Unknown;
                    break;
                default:
                    resource.Status = ObservationStatus.EnteredInError;
                    break;
            }

            // Observation Category
            if (observation.CategoryCode[0] != string.Empty || observation.CategoryDisplay[0] != string.Empty || observation.CategorySystem[0] != string.Empty || observation.CategoryText != null)
            {
                CodeableConcept observationCategory = new CodeableConcept();
                List<Coding> observationCodings = new List<Coding>();

                if (observation.CategoryCode[0] != string.Empty || observation.CategoryDisplay[0] != string.Empty || observation.CategorySystem[0] != string.Empty)
                {
                    for (int i = 0; i < observation.CategoryCode.Count ; i++)
                    {
                        Coding observationCoding = new Coding()
                        {
                            System = observation.CategorySystem[i],
                            Display = observation.CategoryDisplay[i],
                            Code = observation.CategoryCode[i]
                        };
                        observationCodings.Add(observationCoding);
                    }
                    observationCategory.Coding = observationCodings;
                }
                observationCategory.Text = observation.CategoryText;
                
                resource.Category = observationCategory;
            }

            // Observation Code
            if (observation.CodeCode[0] != string.Empty || observation.CodeDisplay[0] != string.Empty || observation.CodeSystem[0] != string.Empty || observation.CodeText != null)
            {
                CodeableConcept observationCode = new CodeableConcept();
                List<Coding> observationCodings = new List<Coding>();

                if (observation.CodeCode[0] != string.Empty || observation.CodeDisplay[0] != string.Empty || observation.CodeSystem[0] != string.Empty)
                {
                    for (int i = 0; i < observation.CodeCode.Count; i++)
                    {
                        Coding observationCoding = new Coding()
                        {
                            System = observation.CodeSystem[i],
                            Display = observation.CodeDisplay[i],
                            Code = observation.CodeCode[i]
                        };
                        observationCodings.Add(observationCoding);
                    }
                    observationCode.Coding = observationCodings;
                }
                observationCode.Text = observation.CategoryText;

                resource.Code = observationCode;
            }

            // Observation references to other resources
            if (observation.PatientReference != null)
            {
                resource.Subject = new ResourceReference();
                resource.Subject.Reference = observation.PatientReference;
            }
            if (observation.DeviceReference != null)
            {
                resource.Device = new ResourceReference();
                resource.Device.Reference = observation.DeviceReference;
            }

            if (observation.PerformerReferences[0] != string.Empty)
            {
                foreach (var reference in observation.PerformerReferences)
                {
                    ResourceReference performerReference = new ResourceReference();
                    performerReference.Reference = reference;
                    resource.Performer.Add(performerReference);
                }
            }

            // Observation Effective times
            // The choice of Type is DateTime.
            if (observation.EffectiveDateTime != null) 
            {
                FhirDateTime dateTime = new FhirDateTime(observation.EffectiveDateTime);
                resource.Effective = dateTime;
            }
            // The choice of Type is Period.
            else
            {
                Period period = new Period
                {
                    Start = observation.EffectivePeriodStart.ToString(),
                    End = observation.EffectivePeriodEnd.ToString()
                };
                resource.Effective = period;
            }

            resource.Issued = observation.Issued;

            // Observation Comments
            resource.Comments = observation.Comments;

            // Site of Body where Observation was made
            if (observation.BodySiteCode != null || observation.BodySiteDisplay != null || observation.BodySiteSystem != null || observation.BodySiteText != null)
            {
                CodeableConcept observationBodySite = new CodeableConcept();
                List<Coding> observationCodings = new List<Coding>();

                if (observation.BodySiteCode != null || observation.BodySiteDisplay != null || observation.BodySiteSystem != null)
                {
                    Coding observationCoding = new Coding()
                    {
                        System = observation.BodySiteSystem,
                        Display = observation.BodySiteDisplay,
                        Code = observation.BodySiteCode
                    };
                    observationCodings.Add(observationCoding);
                    observationBodySite.Coding = observationCodings;
                }
                observationBodySite.Text = observation.BodySiteText;

                resource.BodySite = observationBodySite;
            }

            // Observation Interpretation
            if (observation.InterpretationCode != null || observation.InterpretationDisplay != null || observation.InterpretationSystem != null || observation.InterpretationText != null)
            {
                CodeableConcept observationInterpretation = new CodeableConcept();
                List<Coding> observationCodings = new List<Coding>();

                if (observation.InterpretationCode != null || observation.InterpretationDisplay != null || observation.InterpretationSystem != null)
                {
                    Coding observationCoding = new Coding()
                    {
                        System = observation.InterpretationSystem,
                        Display = observation.InterpretationDisplay,
                        Code = observation.InterpretationCode
                    };
                    observationCodings.Add(observationCoding);
                    observationInterpretation.Coding = observationCodings;
                }
                observationInterpretation.Text = observation.InterpretationText;

                resource.Interpretation = observationInterpretation;
            }

            // Observation Values
            // Values are componentised
            if (observation.ComponentCodeCode[0] != string.Empty || observation.CodeText != null) 
            {
                for (int i = 0; i < observation.ComponentCodeCode.Count; i++)
                {
                    Coding coding = new Coding();
                    coding.Code = observation.ComponentCodeCode[i];

                    resource.Component[i].Code.Coding.Add(coding);

                    // value is of Type Quantity
                    if (observation.ValueQuantityCode[0] != string.Empty)
                    {
                        Quantity quantity = new Quantity();
                        quantity.Code = observation.ValueQuantityCode[i];
                        quantity.System = observation.ValueQuantitySystem[i];
                        quantity.Unit = observation.ValueQuantityUnit[i];
                        quantity.Value = observation.ValueQuantityValue[i];

                        resource.Component[i].Value = quantity;
                    }
                    // value is of Type CodeableConcept
                    else if (observation.ValueCode[0] != string.Empty)
                    {
                        CodeableConcept concept = new CodeableConcept();
                        coding = new Coding();
                        coding.Code = observation.ValueCode[i];
                        coding.Display = observation.ValueDisplay[i];
                        coding.System = observation.ValueSystem[i];

                        concept.Coding.Add(coding);
                        concept.Text = observation.ValueText[i];
                        resource.Component[i].Value = concept;
                    }
                    // value is of Type String
                    else if (observation.ValueString != null)
                    {
                        FhirString fhirString = new FhirString();
                        fhirString.Value = observation.ValueString[i];

                        resource.Component[i].Value = fhirString;
                    }
                    // value is of Type SampledData
                    else if (observation.ValueSampledDataOriginValue[0] != 0)
                    {
                        SampledData sampleData = new SampledData();
                        SimpleQuantity quantity = new SimpleQuantity();
                        quantity.Code = observation.ValueSampledDataOriginCode[i];
                        quantity.System = observation.ValueSampledDataOriginSystem[i];
                        quantity.Unit = observation.ValueSampledDataOriginUnit[i];
                        quantity.Value = observation.ValueSampledDataOriginValue[i];

                        sampleData.Origin = quantity;
                        sampleData.Data = observation.ValueSampledDataData[i];
                        sampleData.Dimensions = observation.ValueSampledDataDimensions[i];
                        sampleData.Period = observation.ValueSampledDataPeriod[i];

                        resource.Component[i].Value = sampleData;
                    }
                    // value is of Type Period 
                    // HOW TO CHECK FOR DEFAULT DATETIME VALUE?
                    else
                    {
                        Period period = new Period();
                        period.Start = observation.ValuePeriodStart[i].ToString();
                        period.End = observation.ValuePeriodEnd[i].ToString();

                        resource.Component[i].Value = period;
                    }
                }
            }
            //There is only one "set" of values
            else
            {
                // value is of Type Quantity
                if (observation.ValueQuantityCode[0] != string.Empty)
                {
                    Quantity quantity = new Quantity();
                    quantity.Code = observation.ValueQuantityCode[0];
                    quantity.System = observation.ValueQuantitySystem[0];
                    quantity.Unit = observation.ValueQuantityUnit[0];
                    quantity.Value = observation.ValueQuantityValue[0];

                    resource.Component[0].Value = quantity;
                }
                // value is of Type CodeableConcept
                else if (observation.ValueCode[0] != string.Empty)
                {
                    CodeableConcept concept = new CodeableConcept();
                    Coding coding = new Coding();
                    coding.Code = observation.ValueCode[0];
                    coding.Display = observation.ValueDisplay[0];
                    coding.System = observation.ValueSystem[0];

                    concept.Coding.Add(coding);
                    concept.Text = observation.ValueText[0];
                    resource.Component[0].Value = concept;
                }
                // value is of Type String
                else if (observation.ValueString != null)
                {
                    FhirString fhirString = new FhirString();
                    fhirString.Value = observation.ValueString[0];

                    resource.Component[0].Value = fhirString;
                }
                // value is of Type SampledData
                else if (observation.ValueSampledDataOriginValue[0] != 0)
                {
                    SampledData sampleData = new SampledData();
                    SimpleQuantity quantity = new SimpleQuantity();
                    quantity.Code = observation.ValueSampledDataOriginCode[0];
                    quantity.System = observation.ValueSampledDataOriginSystem[0];
                    quantity.Unit = observation.ValueSampledDataOriginUnit[0];
                    quantity.Value = observation.ValueSampledDataOriginValue[0];

                    sampleData.Origin = quantity;
                    sampleData.Data = observation.ValueSampledDataData[0];
                    sampleData.Dimensions = observation.ValueSampledDataDimensions[0];
                    sampleData.Period = observation.ValueSampledDataPeriod[0];

                    resource.Component[0].Value = sampleData;
                }
                // value is of Type Period 
                // HOW TO CHECK FOR DEFAULT DATETIME VALUE?
                else
                {
                    Period period = new Period();
                    period.Start = observation.ValuePeriodStart[0].ToString();
                    period.End = observation.ValuePeriodEnd[0].ToString();

                    resource.Component[0].Value = period;
                }
            }

            return resource;
        }
    }
}