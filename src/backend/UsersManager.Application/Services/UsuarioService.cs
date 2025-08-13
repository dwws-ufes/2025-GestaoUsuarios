using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net;
using System.Text;
using UsersManager.Application.DTOs;
using UsersManager.Application.Services;
using UsersManager.Application.Utils;
using UsersManager.Data;
using UsersManager.Data.Entities;
using UsersManager.Data.Repositories;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

public class UsuarioService : IUsuarioService
{
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Acesso> _acessoRepository;
    private readonly IRepository<Permissao> _permissaoRepository;
    private readonly IUnitOfWork _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IExternalDataService _externalDataService;

    // Adiciona um campo para a ontologia
    private readonly Graph _ontologyGraph;
    private readonly NamespaceMapper _nsMapper;
    private readonly Uri _ontologyNamespaceUri;

    public UsuarioService(IUnitOfWork context, IPasswordHasher<Usuario> passwordHasher, IRepository<Usuario> usuarioRepository, IRepository<Acesso> acessoRepository, IHttpContextAccessor httpContextAccessor, IRepository<Permissao> permissaoRepository, IConfiguration configuration, IExternalDataService externalDataService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _usuarioRepository = usuarioRepository;
        _acessoRepository = acessoRepository;
        _httpContextAccessor = httpContextAccessor;
        _permissaoRepository = permissaoRepository;
        _configuration = configuration;
        _externalDataService = externalDataService;

        var systemURL = _configuration["systemURL"];
        if (string.IsNullOrEmpty(systemURL))
        {
            Console.WriteLine("systemURL não configurado em appsettings.json. O RDF pode não ter URIs base.");
            systemURL = "http://localhost:5000"; // Fallback
        }
        _ontologyNamespaceUri = new Uri($"{systemURL}/ontology/vocabulary#");

        // Carrega a ontologia no construtor para reutilização
        _ontologyGraph = new Graph();
        FileInfo fi = new FileInfo("UsersManager.owl");
        FileInfo fiTemp = new FileInfo(Path.Combine(fi.Directory.FullName, Guid.NewGuid().ToString() + ".owl"));
        var content = File.ReadAllText(fi.FullName).Replace("http://www.meusite.com/UsersManager-vocabulario#", _ontologyNamespaceUri.AbsoluteUri);
        File.WriteAllText(fiTemp.FullName, content);
        _ontologyGraph.LoadFromFile(fiTemp.FullName);
        _nsMapper = (NamespaceMapper?)_ontologyGraph.NamespaceMap;
        File.Delete(fiTemp.FullName);
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

    public string SerializeUserFoaf(Task<UsuarioDTO?> userTask)
    {
        var systemURL = _configuration["systemURL"];

        var user = userTask.Result;
        if (user == null) return string.Empty;

        var g = new Graph();
        g.BaseUri = new Uri(systemURL!);

        // Adicionar os namespaces necessários
        g.NamespaceMap.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        g.NamespaceMap.AddNamespace("schema", new Uri("http://schema.org/"));
        g.NamespaceMap.AddNamespace("gr", new Uri("http://purl.org/goodrelations/v1#"));

        // Sujeito: URI do usuário
        var userUri = g.CreateUriNode(new Uri($"{systemURL}/Usuario/{user.Id}.rdf"));

        // Tipagem: o usuário é apenas uma Pessoa
        g.Assert(userUri, g.CreateUriNode("rdf:type"), g.CreateUriNode("foaf:Person"));

        // Propriedades FOAF
        g.Assert(userUri, g.CreateUriNode("foaf:name"), g.CreateLiteralNode(user.Nome));
        g.Assert(userUri, g.CreateUriNode("foaf:mbox"), g.CreateLiteralNode($"mailto:{user.Email}"));

        if (!string.IsNullOrEmpty(user.NomePerfil))
        {
            // 1. Criar um nó para a função de negócio
            // Usar um URN (urn:uuid) para ter um identificador único sem criar uma nova página na URL do sistema
            var businessFunctionNode = g.CreateUriNode("gr:BusinessFunction");

            // 2. Descrever a função de negócio
            g.Assert(businessFunctionNode, g.CreateUriNode("gr:description"), g.CreateLiteralNode(user.NomePerfil, "pt"));

            // 3. Conectar o usuário à função de negócio
            g.Assert(userUri, g.CreateUriNode("gr:hasBusinessFunction"), businessFunctionNode);

            // Opcional: Adicionar a propriedade schema:jobTitle para complementar
            g.Assert(userUri, g.CreateUriNode("schema:jobTitle"), g.CreateLiteralNode(user.NomePerfil));
        }

        // Serialização
        var writer = new CompressingTurtleWriter();
        using var sw = new System.IO.StringWriter();
        writer.Save(g, sw);
        return sw.ToString();
    }

    /// <summary>
    /// Serializa um objeto UsuarioDTO para o formato RDF (Turtle) usando a ontologia personalizada.
    /// </summary>
    /// <param name="usuarioDto">O DTO do usuário a ser serializado.</param>
    /// <returns>Uma string contendo a representação RDF do usuário.</returns>
    public async Task<string> SerializeUser(UsuarioDTO usuarioDto)
    {
        if (usuarioDto == null) return string.Empty;

        var systemURL = _configuration["systemURL"];
        if (string.IsNullOrEmpty(systemURL))
        {
            Console.WriteLine("systemURL não configurado em appsettings.json. O RDF pode não ter URIs base.");
            systemURL = "http://localhost:5000"; // Fallback
        }

        var g = new Graph();
        g.BaseUri = new Uri(systemURL);

        // Adicionando o namespace da sua ontologia e outros namespaces
        g.NamespaceMap.AddNamespace("um_voc", _ontologyNamespaceUri);
        g.NamespaceMap.AddNamespace("schema", new Uri("http://schema.org/"));

        // Sujeito: URI do usuário
        var usuarioUri = g.CreateUriNode(new Uri($"{systemURL}/Usuario/{usuarioDto.Id}.rdf"));

        // Tipagem da permissão (usando a classe 'Usuario' da sua ontologia)
        var usuarioClass = g.CreateUriNode("um_voc:Usuario");
        g.Assert(usuarioUri, g.CreateUriNode("rdf:type"), usuarioClass);

        // Propriedades do usuário (usando o vocabulário da sua ontologia)
        var hasNameProperty = g.CreateUriNode("um_voc:hasName");
        var hasEmailProperty = g.CreateUriNode("um_voc:hasEmail");
        var hasProfileProperty = g.CreateUriNode("um_voc:hasProfile");

        g.Assert(usuarioUri, hasNameProperty, g.CreateLiteralNode(usuarioDto.Nome));
        g.Assert(usuarioUri, hasEmailProperty, g.CreateLiteralNode(usuarioDto.Email));

        // Ligar o usuário a um recurso DBpedia para o 'nome'
        if (!string.IsNullOrEmpty(usuarioDto.Nome))
        {
            var dbpediaDescription = await _externalDataService.ObterDescricaoDbpedia(usuarioDto.Nome);
            if (!string.IsNullOrEmpty(dbpediaDescription))
            {
                var dbpediaResourceUri = new Uri($"http://dbpedia.org/resource/{usuarioDto.Nome.Replace(" ", "_")}");
                g.Assert(usuarioUri, g.CreateUriNode("schema:mentions"), g.CreateUriNode(dbpediaResourceUri));
            }
        }

        // Adicionar perfis como recursos associados ao usuário
        if (usuarioDto.Perfis != null)
        {
            foreach (var perfilDto in usuarioDto.Perfis)
            {
                if (perfilDto.Id.HasValue)
                {
                    var perfilUri = g.CreateUriNode(new Uri($"{systemURL}/Perfil/{perfilDto.Id}.rdf"));
                    g.Assert(usuarioUri, hasProfileProperty, perfilUri);
                }
            }
        }

        // Serializar o grafo para uma string RDF (formato Turtle)
        var writer = new CompressingTurtleWriter();
        using (var stream = new MemoryStream())
        {
            writer.Save(g, new StreamWriter(stream, Encoding.UTF8));
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
