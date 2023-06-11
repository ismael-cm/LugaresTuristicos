namespace LugaresTuristicos.Models
{
    public class CommonProfile
    {
        public List<Lugare> Lugares { get; set; }   
        public Usuario Usuario { get; set; }

        public List<Comentario> Comentario { get; set; }

        public List<string> blacklist { get; set; }
    }
}
