
using LastGrind.Application.Interfaces.Repositories;
using LastGrind.Domain.Entities;
using LastGrind.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace LastGrind.Persistance.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        //private readonly DbSet<T> _context.Set<T>();

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            //_context.Set<T>() = _context.Set<T>();
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> AddAsync(T entity)
        {
           await _context.Set<T>().AddAsync(entity);
            
           int created = await _context.SaveChangesAsync();
           return created > 0;
        }

        public  Task Update(T entity,bool state = true)
        {
            if (state)
            {
                _context.Entry(entity).State = EntityState.Unchanged;
            }
            else
            {
                _context.Set<T>().Attach(entity);
            }
            return Task.CompletedTask;
        }
        public async Task<bool> RemoveAsync(Guid id)
        {
            var post = await GetByIdAsync(id);
            if (post == null) return false;
            _context.Set<T>().Remove(post);
            var deleted = await _context.SaveChangesAsync();
            return deleted > 0;
        }

        
    }
}
