using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersManager.Data.Entities;
using Microsoft.EntityFrameworkCore;
namespace UsersManager.Data.Repositories
{
    public class UsuarioRepository : Repository<Usuario>
    {
        public UsuarioRepository(AppDbContext context) : base(context)
        {

            
        }

        public override async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios
                                  .Include(pp => pp.PerfisUsuario)
                                  .ThenInclude(p=>p.Perfil)
                                  .ToListAsync();
        }
    }
}
