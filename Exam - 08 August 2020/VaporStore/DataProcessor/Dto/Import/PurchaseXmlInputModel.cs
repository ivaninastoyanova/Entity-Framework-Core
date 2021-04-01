﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;
using VaporStore.Data.Models.Enums;

namespace VaporStore.DataProcessor.Dto.Import
{
    [XmlType("Purchase")]
    public class PurchaseXmlInputModel
    {
        [XmlAttribute("title")]
        [Required]
        public string gameTitle { get; set; }

        [XmlElement("Type")]
        [EnumDataType(typeof(PurchaseType))]
        [Required]
        public string Type { get; set; }

        [XmlElement("Key")]
        [RegularExpression(@"^([A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4})$")]
        public string Key { get; set; }

        [XmlElement("Card")]
        [Required]
        public string Card { get; set; }

        [XmlElement("Date")]
        [Required]
        public string Date { get; set; }
    }
}
