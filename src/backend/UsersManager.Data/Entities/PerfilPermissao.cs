using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersManager.Data.Entities
{
    public class PerfilPermissao
    {
        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; } = null!;

        public int PermissaoId { get; set; }
        public Permissao Permissao { get; set; } = null!;
    }
}