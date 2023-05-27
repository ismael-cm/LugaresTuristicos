using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class Lugare
{
    public int IdLugar { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdCategoria { get; set; }

    public string? NombreLugar { get; set; }

    public string? Descripcion { get; set; }

    public int? IdMunicipio { get; set; }

    public decimal? Precio { get; set; }

    public bool Estado { get; set; }

    public byte[]? Imagen { get; set; }
    public DateTime? FechaPublicacion { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual Categoria? IdCategoriaNavigation { get; set; }

    public virtual Municipio? IdMunicipioNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<LugaresValoracione> LugaresValoraciones { get; set; } = new List<LugaresValoracione>();
}
