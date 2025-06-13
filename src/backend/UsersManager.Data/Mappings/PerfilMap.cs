using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersManager.Data.Entities;

namespace UsersManager.Data.Mappings
{
    public class PerfilMap : IEntityTypeConfiguration<Perfil>
    {
        public void Configure(EntityTypeBuilder<Perfil> builder)
        {
            builder.ToTable("Perfis");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Descricao).HasMaxLength(255); // Novo mapeamento


            builder.HasMany(p => p.PerfisPermissao)
                   .WithOne(pp => pp.Perfil)
                   .HasForeignKey(pp => pp.PerfilId);

            builder.HasMany(p => p.PerfisUsuario)
                   .WithOne(pu => pu.Perfil)
                   .HasForeignKey(pu => pu.PerfilId);
        }
    }
}