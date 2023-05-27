using System;
using System.Collections.Generic;

namespace LugaresTuristicos.Models;

public partial class ComentarioOfensivo
{
    public int IdCofensivo { get; set; }

    public int? IdComentario { get; set; }

    public int? IdBlacklist { get; set; }

    public virtual Blacklist? IdBlacklistNavigation { get; set; }

    public virtual Comentario? IdComentarioNavigation { get; set; }
}
