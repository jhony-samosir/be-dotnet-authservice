using System;
using System.Collections.Generic;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public partial class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuthRefreshToken> AuthRefreshToken { get; set; }

    public virtual DbSet<AuthRole> AuthRole { get; set; }

    public virtual DbSet<AuthUser> AuthUser { get; set; }

    public virtual DbSet<AuthUserRole> AuthUserRole { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthRefreshToken>(entity =>
        {
            entity.HasKey(e => e.AuthRefreshTokenId).HasName("auth_refresh_token_pkey");

            entity.ToTable("auth_refresh_token");

            entity.HasIndex(e => e.ExpiredDate, "idx_refresh_token_expired");

            entity.HasIndex(e => e.IsDeleted, "idx_refresh_token_softdelete");

            entity.HasIndex(e => e.Token, "idx_refresh_token_token");

            entity.HasIndex(e => e.AuthUserId, "idx_refresh_token_user").HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthRefreshTokenId).HasColumnName("auth_refresh_token_id");
            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("((now() AT TIME ZONE 'UTC'::text) + '07:00:00'::interval)")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.DeviceInfo).HasColumnName("device_info");
            entity.Property(e => e.ExpiredDate).HasColumnName("expired_date");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.RevokedDate).HasColumnName("revoked_date");
            entity.Property(e => e.Token).HasColumnName("token");
        });

        modelBuilder.Entity<AuthRole>(entity =>
        {
            entity.HasKey(e => e.AuthRoleId).HasName("auth_role_pkey");

            entity.ToTable("auth_role");

            entity.HasIndex(e => e.Name, "idx_auth_role_name");

            entity.HasIndex(e => e.IsDeleted, "idx_auth_role_softdelete");

            entity.HasIndex(e => e.Name, "uq_auth_role_name").IsUnique();

            entity.Property(e => e.AuthRoleId).HasColumnName("auth_role_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("((now() AT TIME ZONE 'UTC'::text) + '07:00:00'::interval)")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.HasKey(e => e.AuthUserId).HasName("auth_user_pkey");

            entity.ToTable("auth_user");

            entity.HasIndex(e => e.IsActive, "idx_auth_user_active").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Email, "idx_auth_user_email");

            entity.HasIndex(e => e.AuthUserId, "idx_auth_user_not_deleted").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Username, "idx_auth_user_username");

            entity.HasIndex(e => e.Email, "uq_auth_user_email").IsUnique();

            entity.HasIndex(e => e.Username, "uq_auth_user_username").IsUnique();

            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("((now() AT TIME ZONE 'UTC'::text) + '07:00:00'::interval)")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsLocked)
                .HasDefaultValue(false)
                .HasColumnName("is_locked");
            entity.Property(e => e.LastLoginDate).HasColumnName("last_login_date");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AuthUserRole>(entity =>
        {
            entity.HasKey(e => e.AuthUserRoleId).HasName("auth_user_role_pkey");

            entity.ToTable("auth_user_role");

            entity.HasIndex(e => e.AuthRoleId, "idx_auth_user_role_role").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.IsDeleted, "idx_auth_user_role_softdelete");

            entity.HasIndex(e => e.AuthUserId, "idx_auth_user_role_user").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.AuthUserId, e.AuthRoleId }, "uq_auth_user_role").IsUnique();

            entity.Property(e => e.AuthUserRoleId).HasColumnName("auth_user_role_id");
            entity.Property(e => e.AssignedBy)
                .HasMaxLength(50)
                .HasColumnName("assigned_by");
            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("((now() AT TIME ZONE 'UTC'::text) + '07:00:00'::interval)")
                .HasColumnName("assigned_date");
            entity.Property(e => e.AuthRoleId).HasColumnName("auth_role_id");
            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
