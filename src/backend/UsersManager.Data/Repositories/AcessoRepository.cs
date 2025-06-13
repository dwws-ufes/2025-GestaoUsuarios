using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersManager.Data.Entities;

namespace UsersManager.Data.Repositories
{
    public class AcessoRepository : Repository<Acesso>
    {
        public AcessoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
