using Microsoft.EntityFrameworkCore;
using UsersManager.Data.Entities;
using UsersManager.Data.Mappings;

namespace UsersManager.Data
{
    public class AppDbContext : DbContext, IUnitOfWork
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Perfil> Perfis => Set<Perfil>();
        public DbSet<Permissao> Permissoes => Set<Permissao>();
        public DbSet<Acesso> Acessos => Set<Acesso>();
        public DbSet<PerfilUsuario> PerfisUsuarios => Set<PerfilUsuario>();
        public DbSet<PerfilPermissao> PerfisPermissoes => Set<PerfilPermissao>();

        public Task<int> CommitAsync() => this.CommitAsync();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioMap());
            modelBuilder.ApplyConfiguration(new PerfilMap());
            modelBuilder.ApplyConfiguration(new PermissaoMap());
            modelBuilder.ApplyConfiguration(new AcessoMap());
            modelBuilder.ApplyConfiguration(new PerfilUsuarioMap());
            modelBuilder.ApplyConfiguration(new PerfilPermissaoMap());
        }
    }
}