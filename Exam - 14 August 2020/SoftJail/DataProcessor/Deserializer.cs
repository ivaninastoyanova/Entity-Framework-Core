namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            List<Department> validDepartments = new List<Department>();
            
            ImportDepartmentCellsDTO[] departments = 
                         JsonConvert.DeserializeObject<ImportDepartmentCellsDTO[]>(jsonString);

            foreach (var currentDepartment in departments)
            {
                if(!IsValid(currentDepartment) || 
                    currentDepartment.Cells.Length == 0 ||
                    !currentDepartment.Cells.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Department department = new Department()
                {
                    Name = currentDepartment.Name,
                    Cells = currentDepartment.Cells.Select(c => new Cell
                    {
                        CellNumber = c.CellNumber,
                        HasWindow = c.HasWindow
                    })
                    .ToList()
                };

                validDepartments.Add(department);
                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
               
            }
            context.Departments.AddRange(validDepartments);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            List<Prisoner> prisonersToAdd = new List<Prisoner>();

            ImportPrisonersMailsDTO[] prisoners =
                  JsonConvert.DeserializeObject<ImportPrisonersMailsDTO[]>(jsonString);

            foreach (var currentPrisoner in prisoners)
            {
                if(!IsValid(currentPrisoner) || !currentPrisoner.Mails.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                DateTime incarcerationDate;
                bool isIncarcerationDateValid = DateTime.TryParseExact(
                     currentPrisoner.IncarcerationDate, 
                     "dd/MM/yyyy",
                     CultureInfo.InvariantCulture, 
                     DateTimeStyles.None, 
                     out incarcerationDate);

                if (!isIncarcerationDateValid)
                {
                    continue;
                }

               
                DateTime? releaseDate;
                if (!String.IsNullOrEmpty(currentPrisoner.ReleaseDate))
                {
                    DateTime prisonerReleaseDate;

                    bool isValidReleaseDate = DateTime.TryParseExact(
                                                   currentPrisoner.ReleaseDate, 
                                                   "dd/MM/yyyy", 
                                                   CultureInfo.InvariantCulture, 
                                                   DateTimeStyles.None, 
                                                   out prisonerReleaseDate);

                    if (!isValidReleaseDate)
                    {
                        sb.AppendLine("Invalid Data");
                        continue;
                    }

                    releaseDate = prisonerReleaseDate;
                }
                else
                {
                    releaseDate = null;
                }

                Prisoner prisoner = new Prisoner
                {
                    FullName = currentPrisoner.FullName,
                    Nickname = currentPrisoner.NickName,
                    Age=currentPrisoner.Age,
                    IncarcerationDate = incarcerationDate,
                    ReleaseDate = releaseDate,
                    Bail = currentPrisoner.Bail,
                    CellId = currentPrisoner.CellId,
                    Mails = currentPrisoner.Mails.Select(m => new Mail
                    {
                        Description = m.Description,
                        Sender= m.Sender,
                        Address = m.Address
                    }).ToArray()
                };

                prisonersToAdd.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }
            context.Prisoners.AddRange(prisonersToAdd);
            context.SaveChanges();
            return sb.ToString().TrimEnd();

        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            List<Officer> officersToBeAdd = new List<Officer>(); 

            ImportOfficersPrisonersXmlDTO[] officers =
                XmlHelper.Deserializer<ImportOfficersPrisonersXmlDTO>(xmlString, "Officers");

            foreach (var currentOfficer in officers)
            {
                if(!IsValid(currentOfficer))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Officer officer = new Officer
                {
                    FullName = currentOfficer.Name,
                    Salary = currentOfficer.Money,
                    Position = Enum.Parse<Position>(currentOfficer.Position),
                    Weapon = Enum.Parse<Weapon>(currentOfficer.Weapon),
                    DepartmentId = currentOfficer.DepartmentId,
                    OfficerPrisoners = currentOfficer.Prisoners.Select(p => new OfficerPrisoner
                    {
                        PrisonerId = p.Id
                    })
                    .ToList()
                };

                officersToBeAdd.Add(officer);
                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }

            context.Officers.AddRange(officersToBeAdd);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}