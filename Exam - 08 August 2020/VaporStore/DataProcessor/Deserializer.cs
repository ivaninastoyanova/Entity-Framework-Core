namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var sb = new StringBuilder();

			GamesJsonInputModel[] games = JsonConvert.DeserializeObject<GamesJsonInputModel[]>(jsonString);

			foreach (var currentGame in games)
			{
				if(!IsValid(currentGame) || currentGame.Tags.Length==0)
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				DateTime releaseDate;
				bool isReleaseDateValid = DateTime.TryParseExact(
					 currentGame.ReleaseDate,
					 "yyyy-MM-dd",
					 CultureInfo.InvariantCulture,
					 DateTimeStyles.None,
					 out releaseDate);

				if (!isReleaseDateValid)
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				var developer = context.Developers.FirstOrDefault(d => d.Name == currentGame.Developer);
				if(developer == null)
				{
					developer = new Developer
					{
						Name = currentGame.Developer
					};
				}
				//втори синтаксис за същата проверка 
				//var developer = context.Developers.FirstOrDefault(d => d.Name == currentGame.Developer)
				//		    ?? new Developer { Name = currentGame.Developer };

				var genre = context.Genres.FirstOrDefault(g => g.Name == currentGame.Genre);
				if (genre == null)
				{
					genre = new Genre
					{
						Name = currentGame.Genre
					};
				}

				//втори синтаксис за същата проверка 
				//var genre = context.Genres.FirstOrDefault(g => g.Name == currentGame.Genre)
				//		    ?? new Genre { Name = currentGame.Genre };

			
				Game game = new Game()
				{
					Name = currentGame.Name,
					Price = currentGame.Price,
					ReleaseDate = releaseDate,
					Developer = developer,
					Genre = genre,
				};

				foreach (var currentTag in currentGame.Tags)
				{
					Tag tag = context.Tags.FirstOrDefault(t => t.Name == currentTag);
					if (tag == null)
					{
						tag = new Tag()
						{
							Name = currentTag
						};
					}

				    game.GameTags
					   .Add(new GameTag
					   {
					      Tag = tag
				       });
				}

				sb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
				context.Games.Add(game);
				context.SaveChanges();
			}

			return sb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var sb = new StringBuilder();

			UserCardsJsonInputModel[] users = JsonConvert.DeserializeObject<UserCardsJsonInputModel[]>(jsonString);

			List<User> usersToAdd = new List<User>();
			foreach (var currentUser in users)
			{
				if(!IsValid(currentUser) || !currentUser.Cards.All(IsValid))
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				User user = new User
				{
					FullName = currentUser.FullName,
					Username = currentUser.Username,
					Email = currentUser.Email,
					Age = currentUser.Age,
					Cards = currentUser.Cards.Select(c => new Card
					{
						Number= c.Number,
						Cvc = c.CVC,
						Type = Enum.Parse<CardType>(c.Type)
					})
					.ToList()
				};

				usersToAdd.Add(user);
				sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
			}

			context.Users.AddRange(usersToAdd);
			context.SaveChanges();
			return sb.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var sb = new StringBuilder();

			List<Purchase> purchasesToAdd = new List<Purchase>();

			PurchaseXmlInputModel[] purchases = XmlConverter.Deserializer<PurchaseXmlInputModel>(xmlString, "Purchases");

			foreach (var currentPurchase in purchases)
			{
				if(!IsValid(currentPurchase))
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				Card card = context.Cards.FirstOrDefault(c => c.Number == currentPurchase.Card);

				Game game = context.Games.FirstOrDefault(g => g.Name == currentPurchase.gameTitle);

				DateTime date;
				bool isDateValid = DateTime.TryParseExact(
					 currentPurchase.Date,
					 "dd/MM/yyyy HH:mm",
					 CultureInfo.InvariantCulture,
					 DateTimeStyles.None,
					 out date);

				if (!isDateValid)
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				Purchase purchase = new Purchase
				{
					ProductKey = currentPurchase.Key,
					Type = Enum.Parse<PurchaseType>(currentPurchase.Type),
					Game = game,
					Card=card,
					Date = date
				};
				purchasesToAdd.Add(purchase);
				sb.AppendLine($"Imported {purchase.Game.Name} for {purchase.Card.User.Username}");
			}

			context.Purchases.AddRange(purchasesToAdd);
			context.SaveChanges();
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