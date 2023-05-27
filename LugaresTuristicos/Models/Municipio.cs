using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class Municipio
{
    public int IdMunicipio { get; set; }

    public int? IdDepto { get; set; }

    public string? Municipio1 { get; set; }

    public virtual Departamento? IdDeptoNavigation { get; set; }

    public virtual ICollection<Lugare> Lugares { get; set; } = new List<Lugare>();
}
