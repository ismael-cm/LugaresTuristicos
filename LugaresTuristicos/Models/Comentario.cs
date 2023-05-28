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

    public bool? Estado { get; set; }

    public string? Revision { get; set; }
    public virtual Lugare? IdLugarNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
