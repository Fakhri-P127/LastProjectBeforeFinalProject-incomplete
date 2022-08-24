using LastGrind.Application.Interfaces.Repositories;
using LastGrind.Domain.Entities;
using LastGrind.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastGrind.Persistance.Repositories
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context):base(context)
        {
            _context = context;
        }
        public async Task<bool> UserOwnsPostAsync(Guid postId, string userId)
        {
            Post post = await _context.Posts.FirstOrDefaultAsync(x=>x.UserId == userId && x.Id == postId );
            
            bool result = post != null;
            return result;
        }
    }
}
