using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Hl7.Fhir.Model;
using static Hl7.Fhir.Model.Observation;
using ServerExperiment.Models.FHIR.Helpers.Observation;

namespace ServerExperiment.Models
{
    public class Observation
    {
        public Observation()
        {
            RecordNo = 0;
            Timestamp = DateTime.UtcNow;
            IsDeleted = false;

            ObservationId = 0;
            Version = 0;
        }

        // Each Record is immutable, in case of updates we create a new record and 
        // keep track of Version, Time of modification and action type like CREATE/UPDATE
        public int RecordNo { get; set; }
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public bool IsDeleted { get; set; }

        // Logical Identifier
        [Key]
        public int ObservationId { get; set; }

        // Observation properties
        public Status Status { get; set; }

        // Observation Category 
        private List<string> _categorySystem;
        public List<string> CategorySystem
        {
            get { return _categorySystem; }
            set { _categorySystem = value; }
        }
        public string CategorySystemSerialised
        {
            get { return String.Join(";", _categorySystem); }
            set { _categorySystem = value.Split(';').ToList(); }
        }
        private List<string> _categoryCode;
        public List<string> CategoryCode
        {
            get { return _categoryCode; }
            set { _categoryCode = value; }
        }
        public string CategoryCodeSerialised
        {
            get { return String.Join(";", _categoryCode); }
            set { _categoryCode = value.Split(';').ToList(); }
        }
        private List<string> _categoryDisplay;
        public List<string> CategoryDisplay
        {
            get { return _categoryDisplay; }
            set { _categoryDisplay = value; }
        }
        public string CategoryDisplaySerialised
        {
            get { return String.Join(";", _categoryDisplay); }
            set { _categoryDisplay = value.Split(';').ToList(); }
        }
        public string CategoryText { get; set; }

        // Observation Code
        private List<string> _codeSystem;
        public List<string> CodeSystem
        {
            get { return _codeSystem; }
            set { _codeSystem = value; }
        }
        public string CodeSystemSerialised
        {
            get { return String.Join(";", _codeSystem); }
            set { _codeSystem = value.Split(';').ToList(); }
        }
        private List<string> _codeCode;
        public List<string> CodeCode
        {
            get { return _codeCode; }
            set { _codeCode = value; }
        }
        public string CodeCodeSerialised
        {
            get { return String.Join(";", _codeCode); }
            set { _codeCode = value.Split(';').ToList(); }
        }
        private List<string> _codeDisplay;
        public List<string> CodeDisplay
        {
            get { return _codeDisplay; }
            set { _codeDisplay = value; }
        }
        public string CodeDisplaySerialised
        {
            get { return String.Join(";", _codeDisplay); }
            set { _codeDisplay = value.Split(';').ToList(); }
        }
        public string CodeText { get; set; }

        // Observation references to other resources
        public string PatientReference { get; set; }
        public string DeviceReference { get; set; }
        private List<string> _performerReferences;
        public List<string> PerformerReferences
        {
            get { return _performerReferences; }
            set { _performerReferences = value; }
        }
        public string PerformerReferencesSerialised
        {
            get { return String.Join(";", _performerReferences); }
            set { _performerReferences = value.Split(';').ToList(); }
        }

        // Observation effective times
        public DateTime EffectiveDateTime { get; set; }
        public DateTime EffectivePeriodStart { get; set; }
        public DateTime EffectivePeriodEnd { get; set; }
        public DateTime Issued { get; set; }

        // Observation Interpretation
        public string InterpretationSystem { get; set; }
        public string InterpretationCode { get; set; }
        public string InterpretationDisplay { get; set; }
        public string InterpretationText { get; set; }

        // Observation comments
        public string comments { get; set; }

        // Site of Body where Observation was made
        public string BodySiteSystem { get; set; }
        public string BodySiteCode { get; set; }
        public string BodySiteDisplay { get; set; }
        public string BodySiteText { get; set; }

        // Observation Values
        // This can apply for both single value (only one item in list) and Componentised values.
        private List<string> _componentCodeSystem;
        public List<string> ComponentCodeSystem
        {
            get { return _componentCodeSystem; }
            set { _componentCodeSystem = value; }
        }
        public string ComponentCodeSystemSerialised
        {
            get { return String.Join(";", _componentCodeSystem); }
            set { _componentCodeSystem = value.Split(';').ToList(); }
        }

        private List<string> _componentCodeCode;
        public List<string> ComponentCodeCode
        {
            get { return _componentCodeCode; }
            set { _componentCodeCode = value; }
        }
        public string ComponentCodeCodeSerialised
        {
            get { return String.Join(";", _componentCodeCode); }
            set { _componentCodeCode = value.Split(';').ToList(); }
        }

        private List<string> _componentCodeDisplay;
        public List<string> ComponentCodeDisplay
        {
            get { return _componentCodeDisplay; }
            set { _componentCodeDisplay = value; }
        }
        public string ComponentCodeDisplaySerialised
        {
            get { return String.Join(";", _componentCodeDisplay); }
            set { _componentCodeDisplay = value.Split(';').ToList(); }
        }
        public string ComponentCodeText { get; set; }

        private List<string> _valueQuantitySystem;
        public List<string> ValueQuantitySystem
        {
            get { return _valueQuantitySystem; }
            set { _valueQuantitySystem = value; }
        }
        public string ValueQuantitySystemSerialised
        {
            get { return String.Join(";", _valueQuantitySystem); }
            set { _valueQuantitySystem = value.Split(';').ToList(); }
        }

        private List<string> _valueQuantityCode;
        public List<string> ValueQuantityCode
        {
            get { return _valueQuantityCode; }
            set { _valueQuantityCode = value; }
        }
        public string ValueQuantityCodeSerialised
        {
            get { return String.Join(";", _valueQuantityCode); }
            set { _valueQuantityCode = value.Split(';').ToList(); }
        }

        private List<string> _valueQuantityUnit;
        public List<string> ValueQuantityUnit
        {
            get { return _valueQuantityUnit; }
            set { _valueQuantityUnit = value; }
        }
        public string ValueQuantityUnitSerialised
        {
            get { return String.Join(";", _valueQuantityUnit); }
            set { _valueQuantityUnit = value.Split(';').ToList(); }
        }

        private List<decimal> _valueQuantityValue;
        public List<decimal> ValueQuantityValue
        {
            get { return _valueQuantityValue; }
            set { _valueQuantityValue = value; }
        }
        public string ValueQuantityValueSerialised
        {
            get { return String.Join(";", _valueQuantityValue); }
            set { _valueQuantityValue = value.Split(';').ToList().Select(decimal.Parse).ToList(); }
        }

        private List<string> _valueSystem;
        public List<string> ValueSystem
        {
            get { return _valueSystem; }
            set { _valueSystem = value; }
        }
        public string ValueSystemSerialised
        {
            get { return String.Join(";", _valueSystem); }
            set { _valueSystem = value.Split(';').ToList(); }
        }

        private List<string> _valueCode;
        public List<string> ValueCode
        {
            get { return _valueCode; }
            set { _valueCode = value; }
        }
        public string ValueCodeSerialised
        {
            get { return String.Join(";", _valueCode); }
            set { _valueCode = value.Split(';').ToList(); }
        }

        private List<string> _valueDisplay;
        public List<string> ValueDisplay
        {
            get { return _valueDisplay; }
            set { _valueDisplay = value; }
        }
        public string ValueDisplaySerialised
        {
            get { return String.Join(";", _valueDisplay); }
            set { _valueDisplay = value.Split(';').ToList(); }
        }

        private List<string> _ValueText;
        public List<string> ValueText
        {
            get { return _ValueText; }
            set { _ValueText = value; }
        }
        public string ValueTextSerialised
        {
            get { return String.Join(";", _ValueText); }
            set { _ValueText = value.Split(';').ToList(); }
        }

        private List<string> _valueString;
        public List<string> ValueString
        {
            get { return _valueString; }
            set { _valueString = value; }
        }
        public string ValueStringSerialised
        {
            get { return String.Join(";", _valueString); }
            set { _valueString = value.Split(';').ToList(); }
        }

        private List<string> _valueSampledDataOriginSystem;
        public List<string> ValueSampledDataOriginSystem
        {
            get { return _valueSampledDataOriginSystem; }
            set { _valueSampledDataOriginSystem = value; }
        }
        public string ValueSampledDataOriginSystemSerialised
        {
            get { return String.Join(";", _valueSampledDataOriginSystem); }
            set { _valueSampledDataOriginSystem = value.Split(';').ToList(); }
        }

        private List<string> _valueSampledDataOriginCode;
        public List<string> ValueSampledDataOriginCode
        {
            get { return _valueSampledDataOriginCode; }
            set { _valueSampledDataOriginCode = value; }
        }
        public string ValueSampledDataOriginCodeSerialised
        {
            get { return String.Join(";", _valueSampledDataOriginCode); }
            set { _valueSampledDataOriginCode = value.Split(';').ToList(); }
        }

        private List<string> _ValueSampledDataOriginUnit;
        public List<string> ValueSampledDataOriginUnit
        {
            get { return _ValueSampledDataOriginUnit; }
            set { _ValueSampledDataOriginUnit = value; }
        }
        public string ValueSampledDataOriginUnitSerialised
        {
            get { return String.Join(";", _ValueSampledDataOriginUnit); }
            set { _ValueSampledDataOriginUnit = value.Split(';').ToList(); }
        }

        private List<decimal> _ValueSampledDataOriginValue;
        public List<decimal> ValueSampledDataOriginValue
        {
            get { return _ValueSampledDataOriginValue; }
            set { _ValueSampledDataOriginValue = value; }
        }
        public string ValueSampledDataOriginValueSerialised
        {
            get { return String.Join(";", _ValueSampledDataOriginValue); }
            set { _ValueSampledDataOriginValue = value.Split(';').ToList().Select(decimal.Parse).ToList(); }
        }

        private List<decimal> _valueSampledDataPeriod;
        public List<decimal> ValueSampledDataPeriod
        {
            get { return _valueSampledDataPeriod; }
            set { _valueSampledDataPeriod = value; }
        }
        public string ValueSampledDataPeriodSerialised
        {
            get { return String.Join(";", _valueSampledDataPeriod); }
            set { _valueSampledDataPeriod = value.Split(';').ToList().Select(decimal.Parse).ToList(); }
        }

        private List<int> _valueSampledDataDimensions;
        public List<int> ValueSampledDataDimensions
        {
            get { return _valueSampledDataDimensions; }
            set { _valueSampledDataDimensions = value; }
        }
        public string ValueSampledDataDimensionsSerialised
        {
            get { return String.Join(";", _valueSampledDataDimensions); }
            set { _valueSampledDataDimensions = value.Split(';').ToList().Select(int.Parse).ToList(); }
        }

        private List<string> _ValueSampledDataData;
        public List<string> ValueSampledDataData
        {
            get { return _ValueSampledDataData; }
            set { _ValueSampledDataData = value; }
        }
        public string ValueSampledDataDataSerialised
        {
            get { return String.Join(";", _ValueSampledDataData); }
            set { _ValueSampledDataData = value.Split(';').ToList(); }
        }

        private List<DateTime> _valuePeriodStart;
        public List<DateTime> ValuePeriodStart
        {
            get { return _valuePeriodStart; }
            set { _valuePeriodStart = value; }
        }
        public string ValuePeriodStartSerialised
        {
            get { return String.Join(";", _valuePeriodStart); }
            set { _valuePeriodStart = value.Split(';').ToList().Select(DateTime.Parse).ToList(); }
        }

        private List<DateTime> _valuePeriodEnd;
        public List<DateTime> ValuePeriodEnd
        {
            get { return _valuePeriodEnd; }
            set { _valuePeriodEnd = value; }
        }
        public string ValuePeriodEndSerialised
        {
            get { return String.Join(";", _valuePeriodEnd); }
            set { _valuePeriodEnd = value.Split(';').ToList().Select(DateTime.Parse).ToList(); }
        }
    }
}