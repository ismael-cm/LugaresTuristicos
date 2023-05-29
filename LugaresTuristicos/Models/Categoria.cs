using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models
{
    public partial class Categoria
    {
        public int IdCategoria { get; set; }

        public string? NombreCategoria { get; set; }

        public bool? Estado { get; set; }

        public virtual ICollection<Lugare> Lugares { get; set; } = new List<Lugare>();
    }
}