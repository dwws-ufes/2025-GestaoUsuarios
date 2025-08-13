using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using UsersManager.Application.DTOs;
using UsersManager.Application.Services;
using UsersManager.Application.Utils;
using UsersManager.Data;
using UsersManager.Data.Entities;
using UsersManager.Data.Repositories;
using VDS.RDF;
using VDS.RDF.Writing;
using static UsersManager.Application.DTOs.PermissaoDTO;
using static UsersManager.Data.IUnitOfWork;
using IUnitOfWork = UsersManager.Data.IUnitOfWork;

namespace UsersManager.Application.Services
{
    public class PerfilService : IPerfilService
    {
        private readonly IRepository<Perfil> _perfilRepository;
        private readonly IRepository<Permissao> _permissaoRepository;
        private readonly IUnitOfWork _context;
        private readonly IConfiguration _configuration;
        private readonly IExternalDataService _externalDataService;

        // Adiciona um campo para a ontologia
        private readonly Graph _ontologyGraph;
        private readonly NamespaceMapper _nsMapper;
        private readonly Uri _ontologyNamespaceUri ;

        public PerfilService(IUnitOfWork unitOfWork, IRepository<Perfil> perfilRepository, IRepository<Permissao> permissaoRepository, IConfiguration configuration, IExternalDataService externalDataService)
        {
            _context = unitOfWork;
            _perfilRepository = perfilRepository;
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
            FileInfo fiTemp = new FileInfo(Path.Combine(fi.Directory.FullName, Guid.NewGuid().ToString()+".owl"));
            var content = File.ReadAllText(fi.FullName).Replace("http://www.meusite.com/UsersManager-vocabulario#", _ontologyNamespaceUri.AbsoluteUri);
            File.WriteAllText(fiTemp.FullName, content);
            _ontologyGraph.LoadFromFile(fiTemp.FullName);
            _nsMapper = (NamespaceMapper?)_ontologyGraph.NamespaceMap;
            File.Delete(fiTemp.FullName);
        }

        public async Task<IEnumerable<PerfilDTO>> ListarTodosAsync()
        {
            var perfis = await _perfilRepository.GetAllAsync();
            return perfis.Select(p => new PerfilDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                Permissoes = p.PerfisPermissao.Select(per => new PermissaoDTO
                {
                    Id = per.Permissao.Id,
                    Nome = per.Permissao.Nome,
                    Recurso = per.Permissao.Recurso,
                    Acao = per.Permissao.Acao.ToActionEnum()
                }).ToList()
            });
        }

        public async Task<PerfilDTO?> ObterPorIdAsync(int id)
        {
            var perfil = await _perfilRepository.GetByIdWithIncludeAsync(id, include:
                query => query
                .Include(p => p.PerfisPermissao)
                .ThenInclude(p => p.Permissao)
                );

            if (perfil == null) return null;

            return new PerfilDTO
            {
                Id = perfil.Id,
                Nome = perfil.Nome,
                Descricao = perfil.Descricao,
                Permissoes = perfil.PerfisPermissao.Select(per => new PermissaoDTO
                {
                    Id = per.Permissao.Id,
                    Nome = per.Permissao.Nome,
                    Recurso = per.Permissao.Recurso,
                    Acao = per.Permissao.Acao.ToActionEnum()
                }).ToList()
            };
        }

        public async Task<PerfilDTO> SaveAsync(PerfilDTO dto)
        {
            // ... (restante do método SaveAsync permanece o mesmo) ...

            // 1. Carrega o perfil existente ou cria um novo
            Perfil perfil;
            if (dto.Id.HasValue && dto.Id.Value > 0)
            {
                var perfis = await _perfilRepository.FindAsync(
            predicate: p => p.Id == dto.Id,
            include: query => query
                .Include(u => u.PerfisPermissao)
                .ThenInclude(p => p.Permissao)
        );
                perfil = perfis.FirstOrDefault();


                if (perfil == null)
                {
                    throw new InvalidOperationException($"Perfil com ID {dto.Id.Value} não encontrado para atualização.");
                }
            }
            else
            {
                // É uma nova criação
                perfil = new Perfil();
            }

            // 2. Atualiza as propriedades básicas do perfil
            perfil.Nome = dto.Nome;
            perfil.Descricao = dto.Descricao;

            // --- Lógica para sincronizar PerfisPermissao ---

            // As permissões que vieram no DTO (serão as permissões finais do perfil)
            var permissoesDoDtoIds = dto.Permissoes.Where(p => p.Id.HasValue).Select(p => p.Id.Value).ToHashSet();

            if (perfil.PerfisPermissao == null)
            {
                perfil.PerfisPermissao = new List<PerfilPermissao>(); // Garante que não é null
            }
            var currentPermissoesIds = perfil.PerfisPermissao.Select(pp => pp.Permissao.Id).ToHashSet();

            // 2.1. Permissões a Remover (Estão no banco, mas não no DTO)
            var permissoesToRemove = perfil.PerfisPermissao
                .Where(pp => !permissoesDoDtoIds.Contains(pp.PermissaoId)) // Use PermissaoId diretamente
                .ToList();

            foreach (var pp in permissoesToRemove)
            {
                perfil.PerfisPermissao.Remove(pp);
            }

            // 2.2. Permissões a Adicionar (Estão no DTO, mas não no banco)
            foreach (var permissaoDto in dto.Permissoes)
            {
                if (permissaoDto.Id.HasValue && permissaoDto.Id.Value > 0)
                {
                    // Se a permissão já existe no perfil, não precisamos adicioná-la novamente.
                    if (!currentPermissoesIds.Contains(permissaoDto.Id.Value))
                    {
                        var permissao = await _permissaoRepository.GetByIdAsync(permissaoDto.Id.Value);
                        if (permissao != null)
                        {
                            perfil.PerfisPermissao.Add(new PerfilPermissao { Permissao = permissao, Perfil = perfil });
                        }
                        else
                        {
                            Console.WriteLine($"Aviso: Permissão com ID {permissaoDto.Id.Value} não encontrada e não será adicionada ao perfil.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Aviso: Permissão sem ID fornecida no DTO. Ignorada para associação.");
                }
            }

            // --- Salvar as mudanças no perfil e suas associações ---
            if (perfil.Id > 0)
            {
                await _perfilRepository.UpdateAsync(perfil); // O Update Async deve salvar as mudanças na coleção
            }
            else
            {
                await _perfilRepository.AddAsync(perfil); // O Add Async deve salvar o perfil e suas novas associações
            }

            // 3. Mapeia o ID de volta para o DTO e retorna
            dto.Id = perfil.Id;
            return dto;
        }

        public async Task<bool> RemoverAsync(int id)
        {
            try
            {
                var perfil = await _perfilRepository.GetByIdAsync(id);
                if (perfil == null) return false;

                await _perfilRepository.RemoveAsync(perfil);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

        }

        public async Task<IEnumerable<PermissaoDTO>> SavePermissoesAsync(params PermissaoDTO[] dtos)
        {
            try
            {
                var list = new List<PermissaoDTO>();
                foreach (var dto in dtos)
                {
                    var permissao = await _permissaoRepository.GetByIdAsync(dto.Id.Value);
                    permissao ??= new Permissao();

                    permissao.Recurso = dto.Recurso;
                    permissao.Acao = dto.Acao.ToString();
                    permissao.Nome = dto.Nome;
                    if (permissao.Id > 0)
                        await _permissaoRepository.UpdateAsync(permissao);
                    else
                        await _permissaoRepository.AddAsync(permissao);
                    dto.Id = permissao.Id;
                    list.Add(dto);
                }
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }


        }

        public async Task<bool> DeletePermissoesAsync(params PermissaoDTO[] dtos) // Recebe um array de IDs a serem excluídos
        {
            try
            {

                foreach (var dto in dtos)
                {
                    var permissao = await _permissaoRepository.GetByIdAsync(dto.Id.Value);
                    permissao ??= new Permissao();

                    permissao.Recurso = dto.Recurso;
                    permissao.Acao = dto.Acao.ToString();
                    permissao.Nome = dto.Nome;
                    if (permissao.Id > 0)
                        await _permissaoRepository.RemoveAsync(permissao);

                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao excluir permissões: {ex.ToString()}");
                return false;
            }
        }

        public async Task<IEnumerable<PermissaoDTO>> ListarTodasPermissoes()
        {

            var permissoes = from p in await _permissaoRepository.GetAllAsync()
                             select new PermissaoDTO { Recurso = p.Recurso, Id = p.Id, Nome = p.Nome, Acao = p.Acao.ToActionEnum() };
            return permissoes;
        }

        // Implementação do método SerializePerfil
        public async Task<string> SerializePerfil(PerfilDTO perfilDTO)
        {
            var perfil = await _perfilRepository.GetByIdWithIncludeAsync(perfilDTO.Id.Value, include:
                query => query
                .Include(p => p.PerfisPermissao)
                .ThenInclude(p => p.Permissao)
                );
            if (perfil == null) return string.Empty;

            var systemURL = _configuration["systemURL"] ?? "http://localhost:5000";

            var g = new Graph();
            g.BaseUri = new Uri(systemURL);

            // Adicionando o namespace da sua ontologia
            g.NamespaceMap.AddNamespace("my_voc", _ontologyNamespaceUri);
            // CORREÇÃO: Adicionando o namespace 'schema' ao grafo local
            g.NamespaceMap.AddNamespace("schema", new Uri("http://schema.org/"));

            // Sujeito: URI do perfil
            var perfilUri = g.CreateUriNode(new Uri($"{systemURL}/Perfil/{perfil.Id}.rdf"));

            // Tipagem mais específica: agora usamos a classe 'Perfil' da sua ontologia
            var perfilClass = g.CreateUriNode("my_voc:Perfil");
            g.Assert(perfilUri, g.CreateUriNode("rdf:type"), perfilClass);

            // Propriedades do Perfil (usando o vocabulário)
            var hasNameProperty = g.CreateUriNode("my_voc:hasName");
            var hasDescriptionProperty = g.CreateUriNode("my_voc:hasDescription");
            var hasPermissionProperty = g.CreateUriNode("my_voc:hasPermission");

            g.Assert(perfilUri, hasNameProperty, g.CreateLiteralNode(perfil.Nome));
            g.Assert(perfilUri, hasDescriptionProperty, g.CreateLiteralNode(perfil.Descricao));

            // Ligar com recurso da DBpedia
            if (!string.IsNullOrEmpty(perfil.Nome))
            {
                var dbpediaDescription = await _externalDataService.ObterDescricaoDbpedia(perfil.Nome);
                if (!string.IsNullOrEmpty(dbpediaDescription))
                {
                    var dbpediaResourceUri = new Uri($"http://dbpedia.org/resource/{perfil.Nome.Replace(" ", "_")}");
                    g.Assert(perfilUri, g.CreateUriNode("schema:mentions"), g.CreateUriNode(dbpediaResourceUri));
                }
            }

            // Adicionar permissões como ações associadas ao perfil
            if (perfil.PerfisPermissao != null)
            {
                foreach (var pp in perfil.PerfisPermissao)
                {
                    if (pp.Permissao != null)
                    {
                        var permissaoUri = g.CreateUriNode(new Uri($"{systemURL}/Permissao/{pp.Permissao.Id}.rdf"));

                        // Relação 1: O perfil tem uma permissão (usando a propriedade 'hasPermission' da sua ontologia)
                        g.Assert(perfilUri, hasPermissionProperty, permissaoUri);

                        // Descreve a permissão (usando o vocabulário)
                        var permissaoClass = g.CreateUriNode("my_voc:Permissao");
                        var hasResourceProperty = g.CreateUriNode("my_voc:hasResource");
                        var hasActionProperty = g.CreateUriNode("my_voc:hasAction");

                        g.Assert(permissaoUri, g.CreateUriNode("rdf:type"), permissaoClass);
                        g.Assert(permissaoUri, hasNameProperty, g.CreateLiteralNode(pp.Permissao.Nome));
                        g.Assert(permissaoUri, hasResourceProperty, g.CreateLiteralNode(pp.Permissao.Recurso));
                        g.Assert(permissaoUri, hasActionProperty, g.CreateLiteralNode(pp.Permissao.Acao));
                    }
                }
            }

            var writer = new CompressingTurtleWriter();
            using (var stream = new MemoryStream())
            {
                writer.Save(g, new StreamWriter(stream, Encoding.UTF8));
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public async Task<string> SerializePermissao(PermissaoDTO permissaoDto)
        {
            if (permissaoDto == null) return string.Empty;

            var systemURL = _configuration["systemURL"];
            if (string.IsNullOrEmpty(systemURL))
            {
                Console.WriteLine("systemURL não configurado em appsettings.json. O RDF pode não ter URIs base.");
                systemURL = "http://localhost:5000"; // Fallback
            }

            var g = new Graph();
            g.BaseUri = new Uri(systemURL);

            // Adicionando o namespace da sua ontologia
            g.NamespaceMap.AddNamespace("my_voc", _ontologyNamespaceUri);
      
            g.NamespaceMap.AddNamespace("schema", new Uri("http://schema.org/"));

            // Sujeito: URI da permissão
            var permissaoUri = g.CreateUriNode(new Uri($"{systemURL}/Permissao/{permissaoDto.Id}"));

            // Tipagem da permissão (usando a classe 'Permissao' da sua ontologia)
            var permissaoClass = g.CreateUriNode("my_voc:Permissao");
            g.Assert(permissaoUri, g.CreateUriNode("rdf:type"), permissaoClass);

            // Propriedades da Permissão (usando o vocabulário)
            var hasNameProperty = g.CreateUriNode("my_voc:hasName");
            var hasResourceProperty = g.CreateUriNode("my_voc:hasResource");
            var hasActionProperty = g.CreateUriNode("my_voc:hasAction");

            g.Assert(permissaoUri, hasNameProperty, g.CreateLiteralNode(permissaoDto.Nome));
            g.Assert(permissaoUri, hasResourceProperty, g.CreateLiteralNode(permissaoDto.Recurso));
            g.Assert(permissaoUri, hasActionProperty, g.CreateLiteralNode(permissaoDto.Acao.ToString()));

            // Ligar a um recurso DBpedia para o 'recurso' da permissão
            if (!string.IsNullOrEmpty(permissaoDto.Recurso))
            {
                var dbpediaDescriptionForResource = await _externalDataService.ObterDescricaoDbpedia(permissaoDto.Recurso);
                if (!string.IsNullOrEmpty(dbpediaDescriptionForResource))
                {
                    var dbpediaResourceUri = new Uri($"http://dbpedia.org/resource/{permissaoDto.Recurso.Replace(" ", "_")}");
                    g.Assert(permissaoUri, g.CreateUriNode("schema:about"), g.CreateUriNode(dbpediaResourceUri));
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
}
