using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using UsersManager.Application.DTOs;
using UsersManager.Application.Services;
using UsersManager.Application.Utils;
using UsersManager.Data;
using UsersManager.Data.Entities;
using UsersManager.Data.Repositories;

public class UsuarioService : IUsuarioService
{
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Acesso> _acessoRepository;
    private readonly IRepository<Permissao> _permissaoRepository;
    private readonly IUnitOfWork _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioService(IUnitOfWork context, IPasswordHasher<Usuario> passwordHasher, IRepository<Usuario> usuarioRepository, IRepository<Acesso> acessoRepository, IHttpContextAccessor httpContextAccessor, IRepository<Permissao> permissaoRepository)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _usuarioRepository = usuarioRepository;
        _acessoRepository = acessoRepository;
        _httpContextAccessor = httpContextAccessor;
        _permissaoRepository = permissaoRepository;
    }

    private async Task<IEnumerable<Permissao>> _GetPermissoesByUsuarioAsync(Usuario usuario)
    {
        var permissoes = await _permissaoRepository.FindAsync(
            predicate: p => p.Id > 0,
            include: query => query
                .Include(pp => pp.PerfisPermissao)
                .ThenInclude(pu => pu.Perfil)
                .ThenInclude(pp => pp.PerfisUsuario)
        );

        if (usuario.PerfisUsuario.Any(x => x.PerfilId.Equals(1)))//Eh Administrador
            return await _permissaoRepository.GetAllAsync();


        var permissoesUsuario = permissoes.Where(x => x.PerfisPermissao.Any(p => usuario.PerfisUsuario.Any(pu => p.PerfilId.Equals(pu.PerfilId)))).Distinct();

        return permissoesUsuario;
    }

    public async Task<LoginDTO> Login(LoginDTO dto)
    {
        var usuario = (from u in await _usuarioRepository.GetAllAsync()
                       where u.Email == dto.Email
                       select u).FirstOrDefault();

        if (usuario == null)
            dto.StateNotFounded = true;

        var resultado = usuario != null ? _passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, dto.Senha) : PasswordVerificationResult.Failed;

        if (resultado != PasswordVerificationResult.Success)
            dto.StateWrongPassword = true;

        dto.Senha = string.Empty;

        // --- Obter IP e Navegador ---
        string clientIp = "N/A";
        string userAgent = "N/A";

        if (_httpContextAccessor.HttpContext != null)
        {
            // Obter IP
            IPAddress? remoteIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            clientIp = remoteIpAddress?.ToString() ?? "N/A";

            // Lógica para obter o IP real atrás de um proxy (se configurado com ForwardedHeaders)
            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                string forwardedFor = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
                clientIp = forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? clientIp;
            }
            else if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Real-IP"))
            {
                clientIp = _httpContextAccessor.HttpContext.Request.Headers["X-Real-IP"].ToString().Trim() ?? clientIp;
            }

            // Obter User-Agent
            userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString() ?? "N/A";

        }

        // --- Registrar Acesso ---
        // Verifique se usuario não é nulo antes de tentar acessar seu Id
        if (usuario != null)
        {
            await _acessoRepository.AddAsync(new Acesso
            {
                Falhou = dto.StateWrongPassword,
                DataHora = DateTime.Now,
                UsuarioId = usuario.Id,
                Ip = clientIp,
                Navegador = userAgent
            });
            //await _context.CommitAsync();
        }
        var permissoes = await _GetPermissoesByUsuarioAsync(usuario);
        // Adicionada verificação de nulo para 'usuario' e 'PerfisUsuario' antes de popular 'UsuarioLogado'
        dto.UsuarioLogado = new UsuarioDTO
        {
            Email = dto.Email,
            NomePerfil = usuario?.PerfisUsuario?.FirstOrDefault()?.Perfil?.Nome, // Safe navigation
            Nome = usuario?.Nome,
            Id = usuario?.Id ?? 0,
            PerfilId = usuario?.PerfisUsuario?.FirstOrDefault()?.PerfilId ?? 0,
            Recursos = permissoes.Select(x => x.Recurso).ToList(),
            Perfis = usuario.PerfisUsuario.Select(x => new PerfilDTO { Descricao = x.Perfil.Descricao, Id = x.PerfilId, Nome = x.Perfil.Nome, Permissoes = x.Perfil.PerfisPermissao.Select(pe => new PermissaoDTO { Id = pe.Permissao.Id, Acao = pe.Permissao.Acao.ToActionEnum(), Nome = pe.Permissao.Nome, Recurso = pe.Permissao.Recurso }).ToList() }).ToList()

        };

        return dto;
    }

    public async Task<IEnumerable<UsuarioDTO>> ListarTodosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return usuarios
            .Select(static u => new UsuarioDTO
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                PerfilId = u.PerfisUsuario.FirstOrDefault(new PerfilUsuario { }).PerfilId,
                NomePerfil = u.PerfisUsuario.FirstOrDefault(new PerfilUsuario { Perfil = new() }).Perfil.Nome
            })
            .ToList();
    }

    public async Task<IEnumerable<AcessoDTO>> ListarAcessosAsync(DateTime dataInicial, DateTime dataFinal, bool falhou, bool sucesso, string sort)
    {
        // Inicia a query base sem filtro de data ou sucesso/falha
        var query = await _acessoRepository.FindAsync(

      predicate: p => p.Id > 0,

      include: query => query

        .Include(u => u.Usuario)

    );

        // Converte para IQueryable para aplicar filtros e ordenação antes de executar a query no banco
        var acessosQuery = (query).AsQueryable();

        // 1. Aplicar filtro de data
        if (dataInicial != DateTime.MinValue && dataFinal != DateTime.MinValue)
            acessosQuery = acessosQuery.Where(p => p.DataHora >= dataInicial && p.DataHora <= dataFinal);

        // 2. Aplicar filtro de sucesso/falha
        if (falhou && !sucesso) // Se só quer falha
        {
            acessosQuery = acessosQuery.Where(p => p.Falhou);
        }
        else if (sucesso && !falhou) // Se só quer sucesso
        {
            acessosQuery = acessosQuery.Where(p => !p.Falhou);
        }
        // Se ambos são true ou ambos são false, nenhum filtro de sucesso/falha é aplicado, retornando todos.

        // 3. Aplicar ordenação (sort)
        if (!string.IsNullOrWhiteSpace(sort))
        {
            switch (sort.ToLower())
            {
                case "datahora_asc":
                    acessosQuery = acessosQuery.OrderBy(a => a.DataHora);
                    break;
                case "datahora_desc":
                    acessosQuery = acessosQuery.OrderByDescending(a => a.DataHora);
                    break;
                case "usuario_asc":
                    acessosQuery = acessosQuery.OrderBy(a => a.Usuario.Nome);
                    break;
                case "usuario_desc":
                    acessosQuery = acessosQuery.OrderByDescending(a => a.Usuario.Nome);
                    break;
                // Adicione mais casos de ordenação conforme necessário (ex: IP, Agente)
                default:
                    acessosQuery = acessosQuery.OrderByDescending(a => a.DataHora); // Ordem padrão
                    break;
            }
        }
        else
        {
            // Ordem padrão se nenhum 'sort' for especificado
            acessosQuery = acessosQuery.OrderByDescending(a => a.DataHora);
        }

        // 4. Mapear para AcessoDTO e retornar
        return acessosQuery
            .Select(a => new AcessoDTO
            {
                Agente = a.Navegador,
                DataHora = a.DataHora,
                Ip = a.Ip,
                Usuario = new UsuarioDTO { Nome = a.Usuario.Nome, Id = a.Usuario.Id },
                Sucesso = !a.Falhou, // Sucesso é o oposto de Falhou
                UsuarioId = a.UsuarioId
            })
            .ToList();
    }

    public async Task<UsuarioDTO?> ObterPorIdAsync(int id)
    {
        var usuario = (from u in await _usuarioRepository.GetAllAsync()
                       where u.Id == id
                       select u).FirstOrDefault();

        if (usuario == null) return null;

        return new UsuarioDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            PerfilId = usuario.PerfisUsuario.FirstOrDefault()?.PerfilId ?? 0,
            NomePerfil = usuario.PerfisUsuario.FirstOrDefault()?.Perfil?.Nome ?? string.Empty,
            Perfis = usuario.PerfisUsuario.Select(x => new PerfilDTO { Id = x.Perfil.Id, Descricao = x.Perfil.Descricao, Nome = x.Perfil.Nome }).ToList()
        };
    }


    public async Task<UsuarioDTO> SaveAsync(UsuarioDTO dto)
    {
        Usuario usuario;

        if (dto.Id > 0) // Cenário de ATUALIZAÇÃO
        {
            // Carrega o usuário existente e seus perfis associados.
            usuario = (await _usuarioRepository.GetAllAsync()).FirstOrDefault(u => u.Id == dto.Id);
            if (usuario == null)
            {
                throw new Exception($"Usuário com ID {dto.Id} não encontrado.");
            }

            // === ADICIONAR ESTA LÓGICA PARA ATUALIZAÇÃO DE SENHA ===
            // A senha só deve ser atualizada se o DTO contiver um valor para ela.
            // O frontend já garante que 'dto.Senha' será vazio ou null se o campo não foi preenchido.
            if (!string.IsNullOrEmpty(dto.Senha))
            {

                usuario.Senha = _passwordHasher.HashPassword(new Usuario(), dto.Senha);
            }
            // =======================================================
        }
        else // Cenário de NOVO USUÁRIO
        {
            usuario = new Usuario();
            usuario.PerfisUsuario = new List<PerfilUsuario>(); // Inicializa a coleção para novo usuário
            usuario.Senha = _passwordHasher.HashPassword(new Usuario(), dto.Senha);
            usuario.DataCadastro = DateTime.Now;
            usuario.Status = "Ativo";
        }

        // Atualiza as propriedades básicas do usuário
        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;

        // === Lógica para SINCRONIZAR os relacionamentos de PerfilUsuario ===
        var newProfileIds = dto.Perfis.Select(p => p.Id.Value).ToList();
        var existingProfileIds = usuario.PerfisUsuario.Select(pu => pu.PerfilId).ToList();

        var profilesToRemove = usuario.PerfisUsuario
            .Where(pu => !newProfileIds.Contains(pu.PerfilId))
            .ToList();

        foreach (var puToRemove in profilesToRemove)
        {
            usuario.PerfisUsuario.Remove(puToRemove);
        }

        var profilesToAdd = newProfileIds
            .Where(id => !existingProfileIds.Contains(id))
            .ToList();

        foreach (var idToAdd in profilesToAdd)
        {
            usuario.PerfisUsuario.Add(new PerfilUsuario { PerfilId = idToAdd });
        }
        // ====================================================================

        // Persiste as mudanças no banco de dados
        if (dto.Id > 0)
        {
            await _usuarioRepository.UpdateAsync(usuario);
        }
        else
        {
            await _usuarioRepository.AddAsync(usuario);
        }

        dto.Id = usuario.Id;
        return dto;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario != null)
                await _usuarioRepository.RemoveAsync(usuario);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

    }
}
