﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.Data.Models
{
    public class User
    {
        public User()
        {
            this.Cards = new HashSet<Card>();
        }
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Username  { get; set; }

        [Required]
        public string FullName  { get; set; }

        [Required]
        public string  Email { get; set; }

        public int Age { get; set; }

        public virtual ICollection<Card> Cards { get; set; }
    }
}
