using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;

using FurCoNZ.Services;
using FurCoNZ.Models;

namespace FurCoNZ.Auth
{
    public class NzFursJwtBearerEvents : JwtBearerEvents
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IUserService _userService;

        public NzFursJwtBearerEvents(IMemoryCache memoryCache, IUserService userService) : base()
        {
            _memoryCache = memoryCache;
            _userService = userService;
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var cancellationToken = context.HttpContext.RequestAborted;
            var principal = context.Principal;
            var userId = principal.FindFirstValue("sub");
            var tokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(principal.FindFirstValue("exp")));

            cancellationToken.ThrowIfCancellationRequested();
            if (principal == null) throw new ArgumentNullException(nameof(context), $"{nameof(context.Principal)} must not be null.");
            if (string.IsNullOrWhiteSpace(userId)) throw new KeyNotFoundException($"JWT token does not appear to have a \"sub\" claim.");

            // Try and get token from memory cache
            if (_memoryCache.TryGetValue(userId, out List<Claim> claims))
            {
                ((ClaimsIdentity)principal.Identity).AddClaims(claims);
            }
            else // Did not exist in memory cache, fetch info from DB and populate ClaimsPrincipal, store in memory cache
            {
                // TODO: Make JWT token blacklist

                await AddUserIfNotExistsAsync(userId, cancellationToken);

                claims = await GetAdditionalClaimsAsync(userId, cancellationToken);
                ((ClaimsIdentity)principal.Identity).AddClaims(claims);

                await AddClaimsToCacheAsync(userId, claims, tokenExpiresAt, cancellationToken);
            }

            return;
        }

        private async Task AddUserIfNotExistsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetUserAsync(userId);
            if (user == null) await _userService.CreateUserAsync(new User
            {
                Id = userId,
            });
        }

        private Task AddClaimsToCacheAsync(string userId, List<Claim> claims, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.Low,
                AbsoluteExpiration = expiresAt,
            };

            _memoryCache.Set(userId, claims);

            return Task.CompletedTask;
        }

        private Task<List<Claim>> GetAdditionalClaimsAsync(string userId, CancellationToken cancellationToken)
        {
            var claims = new List<Claim>();

            return Task.FromResult(claims);
        }
    }
}
