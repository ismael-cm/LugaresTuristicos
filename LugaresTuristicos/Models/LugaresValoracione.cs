using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class LugaresValoracione
{
    public int IdLValoracion { get; set; }

    public int? IdLugar { get; set; }

    public int? IdValoracion { get; set; }

    public int? IdUsuario { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Lugare? IdLugarNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual Valoracione? IdValoracionNavigation { get; set; }
}
