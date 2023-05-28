using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class Departamento
{
    public int IdDepto { get; set; }

    public string? Departamento1 { get; set; }
    public bool? Estado { get; set; }

    public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
}
