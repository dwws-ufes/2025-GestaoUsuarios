using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersManager.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace UsersManager.Data.Repositories
{
    public class PerfilRepository : Repository<Perfil>
    {
        public PerfilRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Perfil>> GetAllAsync()
        {
            return await _context.Perfis
                                  .Include(pp => pp.PerfisUsuario)
                                  .ThenInclude(p => p.Usuario)
                                  .Include(pe => pe.PerfisPermissao)
                                  .ThenInclude(p => p.Permissao)
                                  .ToListAsync();
        }
    }

}
