using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string CurrentUser =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? "system";

    private static bool Has(EntityEntry e, string name)
        => e.Metadata.FindProperty(name) != null;

    private static void Set(EntityEntry e, string name, object? value)
    {
        if (Has(e, name))
            e.Property(name).CurrentValue = value;
    }

    private void ApplyAudit(DbContext context)
    {
        var now = DateTime.UtcNow;
        var user = CurrentUser;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // INSERT
            if (entry.State == EntityState.Added)
            {
                Set(entry, "CreatedDate", now);
                Set(entry, "CreatedBy", user);
                Set(entry, "IsDeleted", false);
            }

            // UPDATE
            if (entry.State == EntityState.Modified)
            {
                Set(entry, "UpdatedDate", now);
                Set(entry, "UpdatedBy", user);

                if (Has(entry, "CreatedDate"))
                    entry.Property("CreatedDate").IsModified = false;

                if (Has(entry, "CreatedBy"))
                    entry.Property("CreatedBy").IsModified = false;
            }

            // SOFT DELETE
            if (entry.State == EntityState.Deleted && Has(entry, "IsDeleted"))
            {
                entry.State = EntityState.Modified;

                Set(entry, "IsDeleted", true);
                Set(entry, "DeletedDate", now);
                Set(entry, "DeletedBy", user);
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context!);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context!);
        return ValueTask.FromResult(result);
    }
}