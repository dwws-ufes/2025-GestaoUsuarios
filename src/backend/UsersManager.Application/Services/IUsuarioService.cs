using UsersManager.Application.DTOs;

namespace UsersManager.Application.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDTO>> ListarTodosAsync();
        Task<UsuarioDTO?> ObterPorIdAsync(int id);
        Task<UsuarioDTO> SaveAsync(UsuarioDTO dto);
        Task<bool> RemoverAsync(int id);

        Task<LoginDTO> Login(LoginDTO dto);

        Task<IEnumerable<AcessoDTO>> ListarAcessosAsync(DateTime dataInicial, DateTime dataFinal, bool falhou, bool sucesso, string sort);
    }

}
