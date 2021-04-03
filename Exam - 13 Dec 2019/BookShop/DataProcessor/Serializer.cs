namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors
                .Select(a => new
            {
                AuthorName = a.FirstName + ' ' + a.LastName,
                Books = a.AuthorsBooks
                  .OrderByDescending(b => b.Book.Price)
                  .Select(b => new
                  {
                      BookName = b.Book.Name,
                      BookPrice =  b.Book.Price.ToString("f2")
                  })
                  .ToList()
            })
             .ToList()
             .OrderByDescending(a => a.Books.Count())
             .ThenBy(a => a.AuthorName)
             .ToList();

            string json = JsonConvert.SerializeObject(authors, Formatting.Indented);
            return json;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var data = context.Books
                .Where(b => b.PublishedOn < date && b.Genre == Genre.Science)
                .OrderByDescending(b => b.Pages)
                .ThenByDescending(b => b.PublishedOn)
                .Select(b => new BookXmlExportModel
                {
                    Pages = b.Pages,
                    Name = b.Name,
                    Date = b.PublishedOn.ToString("d" , CultureInfo.InvariantCulture)
                })
                .Take(10)
                .ToArray();

            string xml = XmlConverter.Serialize(data, "Books");
            return xml;
        }
    }
}