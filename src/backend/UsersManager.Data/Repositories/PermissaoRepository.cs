using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersManager.Data.Entities;

namespace UsersManager.Data.Repositories
{
    public class PermissaoRepository : Repository<Permissao>
    {
        public PermissaoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
