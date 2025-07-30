using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersManager.Application.DTOs;
using UsersManager.Application.Services;
using UsersManager.Data.Entities;

namespace UsersManager.Application.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly IPerfilService _perfilService;

        public PerfilController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        // GET: api/Perfil
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PerfilDTO>>> GetAll()
        {
            try
            {
                var perfis = await _perfilService.ListarTodosAsync();
                return Ok(perfis);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return BadRequest(ex.Message);
            }

        }

        // GET: api/Perfil/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PerfilDTO>> GetById(int id)
        {
            var perfil = await _perfilService.ObterPorIdAsync(id);
            if (perfil == null)
                return NotFound();

            return Ok(perfil);
        }

        // POST: api/Perfil
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PerfilDTO dto)
        {
            try
            {
                var novoPerfil = await _perfilService.SaveAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = novoPerfil.Id }, novoPerfil);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { message = ex.Message });
            }

        }

        // PUT: api/Perfil/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] PerfilDTO dto)
        {
            if (id != dto.Id)
                return BadRequest("ID do perfil inconsistente.");
            try
            {
                await _perfilService.SaveAsync(dto);

            }
            catch (Exception ex)
            {

                return NotFound(ex.Message);
            }


            return NoContent();
        }

        // DELETE: api/Perfil/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id == 1)
                return BadRequest(new { message = "DeleteForbidden" });
            var removido = await _perfilService.RemoverAsync(id);
            if (!removido)
                return NotFound(new { message = "DeleteFailed" });

            return NoContent();
        }


        // PUT: api/Perfil/5/permissoes
        [HttpPut("{id}/permissoes")]
        public async Task<ActionResult> AtualizarPermissoes(int id, [FromBody] PermissaoDTO dto)
        {
            if (id != dto.Id)
                return BadRequest("ID do perfil inconsistente.");

            var atualizado = await _perfilService.SavePermissoesAsync([dto]);
            if (atualizado == null)
                return NotFound();

            return NoContent();
        }

        // GET: api/Perfil/permissoes
        [HttpGet("permissoes")]
        public async Task<ActionResult<IEnumerable<PermissaoDTO>>> GetPermissoes()
        {

            var permissaoList = await _perfilService.ListarTodasPermissoes();

            return Ok(permissaoList);
        }


        // POST: api/Perfil/permissoes
        [HttpPost("permissoes")]
        public async Task<ActionResult<PermissaoDTO>> PostPermissao([FromBody] PermissaoDTO permissaoDto)
        {
            // As validações definidas no PermissaoDTO são automaticamente verificadas aqui.
            // Se houver algum erro de validação, ModelState.IsValid será false.
            if (!ModelState.IsValid)
            {
                // Retorna um status 400 Bad Request com os detalhes dos erros de validação
                return BadRequest(new { message = "ModelStateError", ModelState });
            }

            try
            {
                // Chamar o serviço para criar a permissão
                var permissaoCriada = await _perfilService.SavePermissoesAsync(permissaoDto);


                return Ok(permissaoCriada);
            }
            catch (Exception ex)
            {
                // Log do erro para depuração (use um logger de verdade em produção)
                Console.Error.WriteLine($"Erro ao criar permissão: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor ao criar a permissão.");
            }
        }

        [HttpDelete("permissoes/{id}")]
        public async Task<ActionResult> DeletePermissao(int id)
        {
            var removido = await _perfilService.DeletePermissoesAsync(new PermissaoDTO { Id = id });
            if (!removido)
                return NotFound();

            return NoContent();
        }

        // === Perfil RDF ===

        // GET: api/Perfil/{id}.rdf
        [HttpGet("{id}.rdf")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPerfilRdf(int id)
        {
            var perfil = await _perfilService.ObterPorIdAsync(id);
            if (perfil == null)
                return NotFound();

            var rdf = await _perfilService.SerializePerfil(perfil);
            return Content(rdf, "text/turtle"); // Retorna o conteúdo RDF no formato Turtle
        }

        // === Permissao RDF ===

        // GET: api/Perfil/permissoes/{id}.rdf
        [HttpGet("permissoes/{id}.rdf")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PermissaoDTO>>> GetPermissao(int id)
        {

            var permissao = (from u in await _perfilService.ListarTodasPermissoes()
                             where u.Id.Equals(id)
                             select u).FirstOrDefault();

            if (permissao == null)
                return NotFound();

            var rdf = await _perfilService.SerializePermissao(permissao);
            return Content(rdf, "text/turtle"); // Retorna o conteúdo RDF no formato Turtle

        }
    }
}
