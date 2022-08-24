using LastGrind.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastGrind.Application.Interfaces.Repositories
{
    public interface IPostRepository:IGenericRepository<Post>
    {
        Task<bool> UserOwnsPostAsync(Guid postId, string userId);
    }
}
