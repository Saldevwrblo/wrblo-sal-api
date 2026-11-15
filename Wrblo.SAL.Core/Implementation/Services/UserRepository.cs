using Microsoft.EntityFrameworkCore;
using Wrblo.SAL.Core.Context;
using Wrblo.SAL.Core.Implementation.Repositories;
using Wrblo.SAL.Shared.Entities;

namespace Wrblo.SAL.Core.Implementation.Services;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id && !user.IsDeleted, cancellationToken);

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail && !user.IsDeleted,
            cancellationToken);
    }

    public Task<ApplicationUser?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(
            user => user.GoogleId == googleId && !user.IsDeleted,
            cancellationToken);

    public Task<ApplicationUser?> GetByRefreshTokenHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(
            user => user.RefreshToken == refreshTokenHash && !user.IsDeleted,
            cancellationToken);

    public async Task AddAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}