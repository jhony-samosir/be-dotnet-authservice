using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthService.Data;

public partial class DataContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // === SOFT DELETE GLOBAL FILTER ===
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.IsKeyless) continue;

            entity.AddProperty("IsDeleted", typeof(bool));

            var parameter = Expression.Parameter(entity.ClrType, "e");
            var body = Expression.Equal(
                Expression.Call(
                    typeof(EF),
                    nameof(EF.Property),
                    new[] { typeof(bool) },
                    parameter,
                    Expression.Constant("IsDeleted")
                ),
                Expression.Constant(false)
            );

            modelBuilder.Entity(entity.ClrType)
                .HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }
}
