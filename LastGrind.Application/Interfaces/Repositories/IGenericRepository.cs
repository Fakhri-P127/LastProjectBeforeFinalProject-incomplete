using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastGrind.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T>
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<bool> AddAsync(T entity);
        Task Update(T entity,bool state=true);
        Task<bool> RemoveAsync(Guid id);
    }
}
