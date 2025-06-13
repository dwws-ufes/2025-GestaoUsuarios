using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UsersManager.Application.DTOs;
using UsersManager.Application.Services;
using UsersManager.Application.Utils;
using UsersManager.Data;
using UsersManager.Data.Entities;
using UsersManager.Data.Repositories;
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

        public PerfilService(IUnitOfWork unitOfWork, IRepository<Perfil> perfilRepository, IRepository<Permissao> permissaoRepository)
        {
            _context = unitOfWork;
            _perfilRepository = perfilRepository;
            _permissaoRepository = permissaoRepository;
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
            // 1. Carrega o perfil existente ou cria um novo
            Perfil perfil;
            if (dto.Id.HasValue && dto.Id.Value > 0)
            {
                var perfis = await _perfilRepository.FindAsync(
            predicate: p => p.Id == dto.Id,
            include: query => query
                .Include(u => u.PerfisPermissao)
                .ThenInclude(p=>p.Permissao)
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

    }

}
