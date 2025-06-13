using System;
using System.Collections.Generic;

namespace UsersManager.Data.Entities
{
    public class Acesso
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public DateTime DataHora { get; set; }
        public string Ip { get; set; } = string.Empty;
        public string Navegador { get; set; } = string.Empty;

        public bool Falhou { get; set; } = false;
    }
}