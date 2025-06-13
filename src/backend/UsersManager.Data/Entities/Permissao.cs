using System.Collections.Generic;

namespace UsersManager.Data.Entities
{
    public class Permissao
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Recurso { get; set; } = string.Empty; 
        public string Acao { get; set; } = string.Empty; 

        public ICollection<PerfilPermissao> PerfisPermissao { get; set; } = new List<PerfilPermissao>();
    }
}