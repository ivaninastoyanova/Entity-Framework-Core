using SoftJail.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class ImportOfficersPrisonersXmlDTO
    {
        [XmlElement("Name")]
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string Name { get; set; }


        [XmlElement("Money")]
        [Range(typeof(decimal) , "0" , "79228162514264337593543950335")]
        public decimal Money { get; set; }

       
        [XmlElement("Position")]
        [EnumDataType(typeof(Position))]
        public string Position { get; set; }

       
        [XmlElement("Weapon")]
        [EnumDataType(typeof(Weapon))]
        public string Weapon { get; set; }


        [XmlElement("DepartmentId")]
        public int DepartmentId { get; set; }

        [XmlArray("Prisoners")]
        public ImportPrisonersXmlDTO[] Prisoners { get; set; }
    }
}
