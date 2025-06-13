using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersManager.Data.Entities
{
    public class PerfilUsuario
    {
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; } = null!;
    }
}
