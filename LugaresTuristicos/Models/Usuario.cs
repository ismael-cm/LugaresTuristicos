using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int? IdRol { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public int? Edad { get; set; }

    public string? Correo { get; set; }

    public string? Password { get; set; }

    public bool Estado { get; set; }

    public byte[]? Imagen { get; set; }
    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual Rol? Rol { get; set; }

    public virtual ICollection<Lugare> Lugares { get; set; } = new List<Lugare>();

    public virtual ICollection<LugaresValoracione> LugaresValoraciones { get; set; } = new List<LugaresValoracione>();
}
