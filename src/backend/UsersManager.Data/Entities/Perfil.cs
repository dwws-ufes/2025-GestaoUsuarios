using System.Collections.Generic;

namespace UsersManager.Data.Entities
{
    public class Perfil
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty; 
        public ICollection<PerfilPermissao> PerfisPermissao { get; set; } = new List<PerfilPermissao>();


        public ICollection<PerfilUsuario> PerfisUsuario { get; set; } = new List<PerfilUsuario>();
    }
}