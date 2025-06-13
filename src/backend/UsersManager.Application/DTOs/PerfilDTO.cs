namespace UsersManager.Application.DTOs
{
    public class PerfilDTO
    {
        private int? id;
        public int? Id { get { return id == null ? 0 : id; } set { id = value; } }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;


        public List<PermissaoDTO> Permissoes { get; set; } = new();
    }

}
