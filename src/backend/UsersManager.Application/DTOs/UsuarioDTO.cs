namespace UsersManager.Application.DTOs
{
    public class UsuarioDTO
    {
        private int? id;
        public int? Id { get { return id == null ? 0 : id; } set { id = value; } }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int PerfilId { get; set; }
        public string? NomePerfil { get; set; }
        public string Senha { get; set; } = string.Empty;

        public List<PerfilDTO> Perfis { get; set; } = new();

        public List<String> Recursos { get; set; } = new();

    }

}
