﻿using ServerExperiment.Models.FHIR.Helpers.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ServerExperiment.Models.POCO
{
    public class Patient : IResource
	{
        public Patient()
        {
            IsDeleted = false;

            PatientId = 0;
            Birthday = DateTime.UtcNow;
            Gender = GenderCode.Unknown;
            Active = true;
            Deceased = false;
            
            _firstNames = new List<string>();
            _lastNames = new List<string>();

            _addressLines1 = new List<string>();
            _addressLines2 = new List<string>();
            _postalCodes = new List<string>();
            _cities = new List<string>();
            _countries = new List<string>();
            _states = new List<string>();
            _periodStarts = new List<string>();
            _periodEnds = new List<string>();
        }

        public bool IsDeleted { get; set; }
        public int VersionId { get; set; }

        // Logical Identifier
        [Key]
        public int PatientId { get; set; }
        
        // Patient properties
        // Patient Name
        // EF DOES NOT SERIALISE/DESERIALISE COLLECTIONS FOR ME, MUST DO IT MYSELF
        private List<string> _firstNames;
        public List<string> FirstNames
        {
            get { return _firstNames; }
            set { _firstNames = value; }
        }
        public string FirstNamesSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_firstNames); }
            set { _firstNames = ModelUtils.DeserialiseString(value); }
        }

        private List<string> _lastNames;
        public List<string> LastNames
        {
            get { return _lastNames; }
            set { _lastNames = value; }
        }
        public string LastNamesSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_lastNames); }
            set { _lastNames = ModelUtils.DeserialiseString(value); }
        }

        // Patient Address
        private List<string> _addressLines1;
        public List<string> AddressLines1
        {
            get { return _addressLines1; }
            set { _addressLines1 = value; }
        }
        public string AddressLines1Serialised
        {
            get { return ModelUtils.ReturnSerialisedString(_addressLines1); }
            set { _addressLines1 = ModelUtils.DeserialiseString(value); }
        }
        private List<string> _addressLines2;
        public List<string> AddressLines2
        {
            get { return _addressLines2; }
            set { _addressLines2 = value; }
        }
        public string AddressLines2Serialised
        {
            get { return ModelUtils.ReturnSerialisedString(_addressLines2); }
            set { _addressLines2 = ModelUtils.DeserialiseString(value); }
        }
        private List<string> _postalCodes;
        public List<string> PostalCodes
        {
            get { return _postalCodes; }
            set { _postalCodes = value; }
        }
        public string PostalCodesSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_postalCodes); }
            set { _postalCodes = ModelUtils.DeserialiseString(value); }
        }
        private List<string> _cities;
        public List<string> Cities
        {
            get { return _cities; }
            set { _cities = value; }
        }
        public string CitiesSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_cities); }
            set { _cities = ModelUtils.DeserialiseString(value); }
        }
        private List<string> _countries;
        public List<string> Countries
        {
            get { return _countries; }
            set { _countries = value; }
        }
        public string CountriesSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_countries); }
            set { _countries = ModelUtils.DeserialiseString(value); }
        }
        private List<string> _states;
        public List<string> States
        {
            get { return _states; }
            set { _states = value; }
        }
        public string StatesSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_states); }
            set { _states = ModelUtils.DeserialiseString(value); }
        }
        private List<string> _periodStarts;
        public List<string> PeriodStarts
        {
            get { return _periodStarts; }
            set { _periodStarts = value; }
        }
        public string PeriodStartsSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_periodStarts); }
            set { _periodStarts = ModelUtils.DeserialiseString(value); }
        }
        private List<string> _periodEnds;
        public List<string> PeriodEnds
        {
            get { return _periodEnds; }
            set { _periodEnds = value; }
        }
        public string PeriodEndsSerialised
        {
            get { return ModelUtils.ReturnSerialisedString(_periodEnds); }
            set { _periodEnds = ModelUtils.DeserialiseString(value); }
        }

        // Patient Birthday/Gender
        public DateTime Birthday { get; set; }
        public GenderCode Gender { get; set; }

        // Patient Contact
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }

        // Patient Bools
        public bool Active { get; set; }
        public bool Deceased { get; set; }

        // Property that needs to be mapped to an extension, defintions for the nationalities can be found here:
        // http://www.englishclub.com/vocabulary/world-countries-nationality.htm
        //
        public string Nationality { get; set; }
    }
}