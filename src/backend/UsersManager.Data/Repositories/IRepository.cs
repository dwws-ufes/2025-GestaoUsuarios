using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace UsersManager.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task RemoveAsync(T entity);


        Task<T?> GetByIdWithIncludeAsync(long id, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

    }

}