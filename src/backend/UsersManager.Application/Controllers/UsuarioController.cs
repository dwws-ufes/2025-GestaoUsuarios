using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UsersManager.Application.DTOs;
using UsersManager.Application.Services;
using UsersManager.Data;
using UsersManager.Data.Entities;
using UsersManager.Data.Repositories;

namespace UsersManager.Application.Controllers
{
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UsuarioController(
            IUnitOfWork unitOfWork,
            IUsuarioService usuarioService,
            IPasswordHasher<Usuario> passwordHasher,
            IConfiguration configuration
            )
        {
            _usuarioService = usuarioService;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        // === LOGIN ===
        [HttpPost("Login")]
        public async Task<ActionResult<String>> Login([FromBody] LoginDTO dto)
        {
            var usuario = await _usuarioService.Login(dto);

            if (usuario.StateNotFounded)
            {
                return Unauthorized("Usuário não encontrado.");
            }

            if (usuario.StateWrongPassword)
                return Unauthorized("Senha inválida.");

            var token = GerarTokenJwt(usuario.UsuarioLogado);


            return Ok(new
            {
                Token = token,
                NomeUsuario = usuario.UsuarioLogado.Nome,
                PerfilPrimario = usuario.UsuarioLogado.NomePerfil ?? "Desconhecido",
                Perfis=usuario.UsuarioLogado.Perfis.Select(x=>x.Nome),
                Recursos = usuario.UsuarioLogado.Recursos
            });
        }

        // === CRUD DE USUÁRIO ===

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetAll()
        {
            try
            {
                var usuarios = await _usuarioService.ListarTodosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UsuarioDTO>> GetById(int id)
        {
            var usuario = await _usuarioService.ObterPorIdAsync(id);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] UsuarioDTO dto)
        {
            try
            {
                var novoUsuario = await _usuarioService.SaveAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = novoUsuario.Id }, novoUsuario);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, [FromBody] UsuarioDTO dto)
        {
            if (id != dto.Id)
                return BadRequest("ID do perfil inconsistente.");
            try
            {
                await _usuarioService.SaveAsync(dto);

            }
            catch (Exception ex)
            {

                return NotFound(ex.Message);
            }


            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            if (id == 1)
                return BadRequest(new { message = "DeleteForbidden" });
            var removido = await _usuarioService.RemoverAsync(id);
            if (!removido)
                return NotFound(new { message = "DeleteFailed" });

            return NoContent();
        }

        // === JWT ===

        private string GerarTokenJwt(UsuarioDTO usuario)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]!);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.NomePerfil)
        };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        // === Acessos ===


        [HttpGet("acessos")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AcessoDTO>>> GetAllAcessos(DateTime dataInicial, DateTime dataFinal, bool falhou,bool sucesso, string sort)
        {
            try
            {
                var acessos = await _usuarioService.ListarAcessosAsync(dataInicial,dataFinal,falhou,sucesso,sort);
                return Ok(acessos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
