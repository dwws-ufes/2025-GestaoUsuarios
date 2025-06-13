using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsersManager.Data.Entities;

namespace UsersManager.Data.Mappings
{
    public class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Nome).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
            builder.Property(u => u.Senha).IsRequired().HasMaxLength(255); 
            builder.Property(u => u.DataCadastro).IsRequired(); 
            builder.Property(u => u.Status).IsRequired().HasMaxLength(50); 

          
            builder.HasMany(u => u.PerfisUsuario)
                   .WithOne(pu => pu.Usuario)
                   .HasForeignKey(pu => pu.UsuarioId);
        }
    }
}