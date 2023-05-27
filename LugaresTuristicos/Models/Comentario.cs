using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class Comentario
{
    public int IdComentario { get; set; }

    public int? IdLugar { get; set; }

    public int? IdUsuario { get; set; }

    public string? Comentario1 { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual ICollection<ComentarioOfensivo> ComentarioOfensivos { get; set; } = new List<ComentarioOfensivo>();

    public virtual Lugare? IdLugarNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
