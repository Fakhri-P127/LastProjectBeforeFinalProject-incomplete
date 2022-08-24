using LastGrind.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastGrind.Application.Interfaces
{
    public interface IUnitOfWork
    {
        public IPostRepository PostRepository { get; }
        //public IIdentityRepository IdentityRepository { get; }
        Task SaveChangesAsync();
    }
}
