using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using FurCoNZ.DAL;
using FurCoNZ.Models;
using System.Data;

namespace FurCoNZ.Services
{
    public class EntityFrameworkUserService : IUserService
    {
        private readonly FurCoNZDbContext _dbContext;

        public EntityFrameworkUserService(FurCoNZDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (await _dbContext.Users.AnyAsync(u => u.Id == user.Id, cancellationToken))
                throw new DuplicateNameException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            await _dbContext.Users.AddAsync(user, cancellationToken); // Seems silly to be async, but used in some SQL Server scenarios

            await _dbContext.SaveChangesAsync();
        }

        public async Task<User> GetUserAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            return await _dbContext.Users.FindAsync(new[] { id }, cancellationToken);
        }

        public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Update(user);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> ListUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.ToListAsync(cancellationToken);
        }
    }
}
