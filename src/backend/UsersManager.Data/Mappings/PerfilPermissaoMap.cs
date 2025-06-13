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
    public class PerfilPermissaoMap : IEntityTypeConfiguration<PerfilPermissao>
    {
        public void Configure(EntityTypeBuilder<PerfilPermissao> builder)
        {
            builder.ToTable("PerfilPermissao"); // Nome da tabela de junção explícita

            builder.HasKey(pp => new { pp.PerfilId, pp.PermissaoId }); // Chave primária composta

            builder.HasOne(pp => pp.Perfil)
                   .WithMany(p => p.PerfisPermissao) // Adicionar PerfisPermissao à classe Perfil
                   .HasForeignKey(pp => pp.PerfilId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pp => pp.Permissao)
                   .WithMany(perm => perm.PerfisPermissao) // Adicionar PerfisPermissao à classe Permissao
                   .HasForeignKey(pp => pp.PermissaoId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}