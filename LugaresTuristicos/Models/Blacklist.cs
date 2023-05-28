using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class Blacklist
{
    public int IdBlacklist { get; set; }

    public string? Palabra { get; set; }
    public bool? Estado { get; set; }

    public virtual ICollection<ComentarioOfensivo> ComentarioOfensivos { get; set; } = new List<ComentarioOfensivo>();
}
