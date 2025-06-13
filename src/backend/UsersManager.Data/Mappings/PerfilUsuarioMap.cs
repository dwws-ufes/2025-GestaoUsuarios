using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsersManager.Data.Entities;

namespace UsersManager.Data.Mappings
{
    public class PerfilUsuarioMap : IEntityTypeConfiguration<PerfilUsuario>
    {
        public void Configure(EntityTypeBuilder<PerfilUsuario> builder)
        {
            builder.ToTable("PerfilUsuario"); 

            builder.HasKey(pu => new { pu.UsuarioId, pu.PerfilId }); 

            builder.HasOne(pu => pu.Usuario)
                   .WithMany(u => u.PerfisUsuario) 
                   .HasForeignKey(pu => pu.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pu => pu.Perfil)
                   .WithMany(p => p.PerfisUsuario) 
                   .HasForeignKey(pu => pu.PerfilId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}