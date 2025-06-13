using UsersManager.Data.Entities;

namespace UsersManager.Application.DTOs
{
    public class AcessoDTO
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public UsuarioDTO Usuario { get; set; } 

        public DateTime DataHora { get; set; }
        public string Ip { get; set; } = string.Empty;
        public string Agente { get; set; } = string.Empty;
        public bool Sucesso { get; set; }
    }
}
