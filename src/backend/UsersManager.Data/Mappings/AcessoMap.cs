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
    public class AcessoMap : IEntityTypeConfiguration<Acesso>
    {
        public void Configure(EntityTypeBuilder<Acesso> builder)
        {
            builder.ToTable("Acessos");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.DataHora).IsRequired();
            builder.Property(a => a.Ip).HasMaxLength(45);
            builder.Property(a => a.Navegador).HasMaxLength(200);
            builder.Property(a => a.Falhou).HasDefaultValue(false);


            builder.HasOne(a => a.Usuario)
                   .WithMany(u => u.Acessos)
                   .HasForeignKey(a => a.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}