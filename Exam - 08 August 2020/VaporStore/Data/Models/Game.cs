using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.Data.Models
{
    public class Game
    {
        public Game()
        {
            this.GameTags = new HashSet<GameTag>();
            this.Purchases = new HashSet<Purchase>();
        }
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal Price  { get; set; }

        public DateTime ReleaseDate  { get; set; }

        public int DeveloperId  { get; set; }

        public Developer Developer  { get; set; }

        public int GenreId  { get; set; }

        public Genre Genre { get; set; }

        public virtual ICollection<Purchase> Purchases  { get; set; }

        public virtual ICollection<GameTag> GameTags  { get; set; }
    }
}
