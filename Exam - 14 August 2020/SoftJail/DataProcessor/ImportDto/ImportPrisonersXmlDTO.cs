﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Prisoner")]
    public class ImportPrisonersXmlDTO
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}
