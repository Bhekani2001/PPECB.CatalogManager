using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.IRepositories
{
    public interface IRepository<T> where T : class
    {
        // Get operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // Query with includes
        Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);

        // Count
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // Check existence
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        // Add operations
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        // Update operations
        T Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        // Delete operations (soft delete by default)
        Task<bool> SoftDeleteAsync(int id, string deletedBy);
        Task<bool> HardDeleteAsync(int id);
        void SoftDelete(T entity, string deletedBy);
        void HardDelete(T entity);

        // Save changes
        Task<int> SaveChangesAsync();
    }
}