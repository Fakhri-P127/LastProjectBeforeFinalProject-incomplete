using LastGrind.Application.Interfaces;
using LastGrind.Application.Interfaces.Repositories;
using LastGrind.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastGrind.Persistance.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IPostRepository PostRepository { get => new PostRepository(_context) ?? throw new NotImplementedException(); }
        //public IIdentityRepository IdentityRepository { get => new IdentityRepository(_context) ?? throw new NotImplementedException(); }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        //public void Dispose()
        //{
        //    _context.Dispose();
        //}
    }
}
