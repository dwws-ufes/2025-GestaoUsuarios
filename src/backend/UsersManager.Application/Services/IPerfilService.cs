using UsersManager.Application.DTOs;

namespace UsersManager.Application.Services
{
    public interface IPerfilService
    {
        Task<IEnumerable<PerfilDTO>> ListarTodosAsync();
        Task<PerfilDTO?> ObterPorIdAsync(int id);
        Task<PerfilDTO> SaveAsync(PerfilDTO dto);
        Task<bool> RemoverAsync(int id);
        Task<IEnumerable<PermissaoDTO>> SavePermissoesAsync(params PermissaoDTO[] dtos);
        Task<IEnumerable<PermissaoDTO>> ListarTodasPermissoes();

        Task<bool> DeletePermissoesAsync(params PermissaoDTO[] dtos);

        Task<string> SerializePerfil(PerfilDTO perfilDTO);

        Task<string> SerializePermissao(PermissaoDTO permissaoDTO);
    }

}
