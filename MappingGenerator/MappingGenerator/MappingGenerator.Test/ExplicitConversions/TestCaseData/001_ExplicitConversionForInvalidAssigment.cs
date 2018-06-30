﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MappingGenerator.Test.ExplicitConversions.TestCaseData
{
    public class TestMapper
    {
        public AddressDTO Address { get; set; }

        public void DoSomething()
        {
            var addressEntity = new AddressEntity();
            this.Address = addressEntity;
        }
    }

    public class AddressDTO
    {
        public string FlatNo { get; set; }
        public string BuildtingNo { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
    }

    public class AddressEntity
    {
        public string FlatNo { get; set; }
        public string BuildtingNo { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
    }
}