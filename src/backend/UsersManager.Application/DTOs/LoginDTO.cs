namespace UsersManager.Application.DTOs
{
    public class LoginDTO
    {
        public required string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public bool StateNotFounded { get; set; } = false;
        public bool StateWrongPassword { get; set; } = false;

        public UsuarioDTO? UsuarioLogado { get; set; } = null;
    }

}
