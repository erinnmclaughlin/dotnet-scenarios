using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Scenarios.AzureB2C.WebApi.Database;

namespace Scenarios.AzureB2C.WebApi.Middleware;

public sealed class UserPersistenceMiddleware(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IMiddleware
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory = dbContextFactory;
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (MapClaimsToUserOrNull(context.User) is { } user)
        {
            await UpsertUser(user, context.RequestAborted);
        }
        
        await next(context);
    }

    private static ApplicationUser? MapClaimsToUserOrNull(ClaimsPrincipal principal)
    {
        if (principal.Identity is { IsAuthenticated: true } && principal.HasClaim(x => x.Type == "tfp"))
        {
            return new ApplicationUser
            {
                Id = principal.FindFirstValue("oid")!,
                FirstName = principal.FindFirstValue("given_name")!,
                LastName = principal.FindFirstValue("family_name")!,
                DisplayName = principal.FindFirstValue("name")!,
                EmailAddress = principal.FindFirstValue("emails"),
                LastActive = DateTimeOffset.UtcNow
            };
        }

        return null;
    }

    private async Task UpsertUser(ApplicationUser user, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var userExists = await dbContext.Users.AnyAsync(x => x.Id == user.Id, cancellationToken);
        
        dbContext.Entry(user).State = userExists ? EntityState.Modified : EntityState.Added;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}