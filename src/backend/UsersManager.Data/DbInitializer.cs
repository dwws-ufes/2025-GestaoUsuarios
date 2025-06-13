using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UsersManager.Data.Entities;

namespace UsersManager.Data
{
    public static class DbInitializer
    {

        public static async Task SeedAsync(AppDbContext context, IPasswordHasher<Usuario> hasher)
        {
            // Aplica migrações pendentes (opcional)
            await context.Database.MigrateAsync();

            // Verifica se já existe algum usuário
            if (await context.Usuarios.AnyAsync()) return;

            // Cria perfil Admin se necessário
            var perfilAdmin = new Perfil
            {
                Nome = "Admin",
                Descricao = "Perfil de administrador do sistema." // Adicionada a propriedade Descricao
            };
            context.Perfis.Add(perfilAdmin);
            await context.SaveChangesAsync();

            // Cria usuário administrador
            var usuarioAdmin = new Usuario
            {
                Nome = "Administrador do Sistema",
                Email = "admin@admin.com",
                DataCadastro = DateTime.UtcNow, // Nova propriedade
                Status = "Ativo", // Nova propriedade
                                  // SenhaHash agora é 'Senha' na classe Usuario
                Senha = hasher.HashPassword(new Usuario(), "admin") // Hash da senha para 'Senha'
            };

            // Cria a entidade de junção PerfilUsuario para associar o usuário ao perfil
            var perfilUsuarioAdmin = new PerfilUsuario
            {
                Usuario = usuarioAdmin,
                Perfil = perfilAdmin
            };

            // Adiciona a entidade de junção ao contexto
            context.PerfisUsuarios.Add(perfilUsuarioAdmin);

            // Não adicionamos o usuário diretamente, pois ele será salvo via PerfilUsuario e SaveChanges
            // context.Usuarios.Add(usuarioAdmin); // Removido, pois a associação é feita por PerfilUsuario

            await context.SaveChangesAsync();
        }
    }

}
