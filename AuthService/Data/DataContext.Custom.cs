using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthService.Data;

public partial class DataContext : DbContext
{
    public DataContextFlags Flags { get; } = new();
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // === SOFT DELETE GLOBAL FILTER ===
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.IsKeyless)
                continue;

            // Only apply the filter to entities that actually have an IsDeleted property.
            // Previously we always added a shadow property, which conflicts now that
            // the entities define a real IsDeleted column.
            if (entity.FindProperty("IsDeleted") == null)
                continue;

            var parameter = Expression.Parameter(entity.ClrType, "e");
            var body = Expression.Equal(
                Expression.Call(
                    typeof(EF),
                    nameof(EF.Property),
                    [typeof(bool)],
                    parameter,
                    Expression.Constant("IsDeleted")
                ),
                Expression.Constant(false)
            );

            modelBuilder.Entity(entity.ClrType)
                .HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }
    public async Task<int> SaveChangesSkipAuditAsync(CancellationToken cancellationToken = default)
    {
        Flags.SkipAudit = true;

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            Flags.SkipAudit = false;
        }
    }
}
