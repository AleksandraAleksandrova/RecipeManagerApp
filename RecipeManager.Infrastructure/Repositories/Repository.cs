using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Core.Interfaces;
using RecipeManager.Infrastructure.Data;

namespace RecipeManager.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                if (typeof(T) == typeof(RecipeManager.Core.Models.Recipe))
                {
                    return (List<T>)(object)await _context.Set<RecipeManager.Core.Models.Recipe>().Include(r => r.Category).AsNoTracking().ToListAsync();
                }
                return await _dbSet.AsNoTracking().ToListAsync();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("A database error occurred while fetching data.", ex);
            }
        }

        public async Task<List<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
        {
            try
            {
                if (typeof(T) == typeof(RecipeManager.Core.Models.Recipe))
                {
                    var exp = predicate as System.Linq.Expressions.Expression<System.Func<RecipeManager.Core.Models.Recipe, bool>>;
                    return (List<T>)(object)await _context.Set<RecipeManager.Core.Models.Recipe>().Include(r => r.Category).AsNoTracking().Where(exp).ToListAsync();
                }
                return await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("A database error occurred while filtering data.", ex);
            }
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("A database error occurred while fetching the entity.", ex);
            }
        }

        public async Task AddAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new System.Exception("A database error occurred while saving the new entity.", ex);
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new System.Exception("A database error occurred while updating the entity.", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                throw new System.Exception("A database error occurred while deleting the entity.", ex);
            }
        }
    }
}