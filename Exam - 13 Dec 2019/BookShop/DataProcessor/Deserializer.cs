namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {

            var sb = new StringBuilder();

            List<Book> booksToAdd = new List<Book>();

            var books = XmlConverter.Deserializer<BookXmlImportModel>(xmlString, "Books");

            foreach (var book in books)
            {
                if(!IsValid(book))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime publishedOn;
                bool isValidDate = DateTime.TryParseExact(
                     book.PublishedOn,
                     "MM/dd/yyyy",
                     CultureInfo.InvariantCulture,
                     DateTimeStyles.None,
                     out publishedOn);

                if (!isValidDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Book bookToAdd = new Book
                {
                    Name = book.Name,
                    Pages = book.Pages,
                    Price = book.Price,
                    PublishedOn = publishedOn,
                    Genre = (Genre)book.Genre
                };

                booksToAdd.Add(bookToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedBook , book.Name , book.Price));
            }

            context.Books.AddRange(booksToAdd);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var sb = new StringBuilder();

            AuthorJsonImportModel[] authors = JsonConvert.DeserializeObject<AuthorJsonImportModel[]>(jsonString);

            foreach (var author in authors)
            {
                List<Book> booksToAddToAuthor = new List<Book>();

                if(!IsValid(author) || !author.Books.Any() || !author.Books.All(IsValid) )
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (context.Authors.Any(a => a.Email == author.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                Author authorToDb = new Author
                {
                    FirstName = author.FirstName,
                    LastName = author.LastName,
                    Phone = author.Phone,
                    Email = author.Email,
                };


                foreach (var book in author.Books)
                {
                    if(!book.Id.HasValue)
                    {
                        continue;
                    }

                    Book bookToAdd = context.Books.FirstOrDefault(b => b.Id == book.Id);

                    if(bookToAdd == null)
                    {
                        continue;
                    }

                    booksToAddToAuthor.Add(bookToAdd);
                }

                if(booksToAddToAuthor.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                foreach (var currentBook in booksToAddToAuthor)
                {
                    authorToDb.AuthorsBooks.Add(new AuthorBook 
                    {
                        Book = currentBook,
                    });
                }

                context.Add(authorToDb);
                context.SaveChanges();
                sb.AppendLine(string.Format(SuccessfullyImportedAuthor,
                     authorToDb.FirstName + " " + authorToDb.LastName, authorToDb.AuthorsBooks.Count));
            }
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}