namespace VaporStore.DataProcessor
{
	using System;
    using System.Globalization;
    using System.Linq;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
		{
			var genres = context.Genres
				   .ToList()
				   .Where(g => genreNames.Contains(g.Name))
				   .Select(g => new
				   {
					   Id = g.Id,
					   Genre = g.Name,
					   Games = g.Games.Select(ga => new
					   {
						   Id = ga.Id,
						   Title = ga.Name,
						   Developer = ga.Developer.Name,
						   Tags = string.Join(", ", ga.GameTags.Select(t => t.Tag.Name)),
						   Players = ga.Purchases.Count
					   })
					   .Where( ga => ga.Players >0 )
					   .OrderByDescending(ga => ga.Players)
					   .ThenBy(ga => ga.Id)
					   .ToList(),
					   TotalPlayers = g.Games.Sum(s => s.Purchases.Count)
				   })
				   .OrderByDescending(g => g.TotalPlayers)
				   .ThenBy(g => g.Id)
				   .ToList();

			 string json = JsonConvert.SerializeObject(genres, Formatting.Indented);
			 return json;
		}

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
		{
			var data = context.Users
				.ToArray()
				.Where(u => u.Cards.Any(c => c.Purchases.Any(p => p.Type.ToString() == storeType)))
				.Select(u => new UserPurchasesXmlModel
				{
					Username = u.Username,
					Purchases = u.Cards.SelectMany(c => c.Purchases)
					        .Where(c => c.Type.ToString() == storeType)
					        .Select(p => new PurchaseXmlModel
					        {
					           Card = p.Card.Number,
						       Cvc = p.Card.Cvc,
						       Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
						       Game = new GameXmlModel
						       {
							      Title = p.Game.Name,
							      Genre = p.Game.Genre.Name,
							      Price = p.Game.Price
						       }
					        })
					        .OrderBy( p => p.Date)
					        .ToArray(),
				    TotalSpent = u.Cards.SelectMany( c => c.Purchases)
					          .Where(c => c.Type.ToString() == storeType)
							  .Sum( p => p.Game.Price)
				})
				.OrderByDescending( u => u.TotalSpent)
				.ThenBy (u => u.Username)
				.ToArray();

			var xml = XmlConverter.Serialize(data, "Users");
			return xml;
		}
	}
}