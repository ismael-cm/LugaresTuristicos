using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class Valoracione
{
    public int IdValoracion { get; set; }

    public string? Valoracion { get; set; }

    public virtual ICollection<LugaresValoracione> LugaresValoraciones { get; set; } = new List<LugaresValoracione>();
}
