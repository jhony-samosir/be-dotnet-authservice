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

    public virtual DbSet<AuthMenu> AuthMenu { get; set; }

    public virtual DbSet<AuthMenuPermission> AuthMenuPermission { get; set; }

    public virtual DbSet<AuthPermission> AuthPermission { get; set; }

    public virtual DbSet<AuthRole> AuthRole { get; set; }

    public virtual DbSet<AuthRolePermission> AuthRolePermission { get; set; }

    public virtual DbSet<AuthSession> AuthSession { get; set; }

    public virtual DbSet<AuthTenant> AuthTenant { get; set; }

    public virtual DbSet<AuthUser> AuthUser { get; set; }

    public virtual DbSet<AuthUserRole> AuthUserRole { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthMenu>(entity =>
        {
            entity.HasKey(e => e.AuthMenuId).HasName("auth_menu_pkey");

            entity.ToTable("auth_menu");

            entity.HasIndex(e => e.IsDeleted, "idx_auth_menu_not_deleted").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.ParentId, "idx_auth_menu_parent");

            entity.Property(e => e.AuthMenuId).HasColumnName("auth_menu_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.Icon)
                .HasMaxLength(100)
                .HasColumnName("icon");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Path)
                .HasMaxLength(200)
                .HasColumnName("path");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        modelBuilder.Entity<AuthMenuPermission>(entity =>
        {
            entity.HasKey(e => e.AuthMenuPermissionId).HasName("auth_menu_permission_pkey");

            entity.ToTable("auth_menu_permission");

            entity.HasIndex(e => e.AuthMenuId, "idx_auth_menu_perm_menu").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.AuthPermissionId, "idx_auth_menu_perm_perm").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.AuthMenuId, e.AuthPermissionId }, "uq_auth_menu_permission")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthMenuPermissionId).HasColumnName("auth_menu_permission_id");
            entity.Property(e => e.AuthMenuId).HasColumnName("auth_menu_id");
            entity.Property(e => e.AuthPermissionId).HasColumnName("auth_permission_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        modelBuilder.Entity<AuthPermission>(entity =>
        {
            entity.HasKey(e => e.AuthPermissionId).HasName("auth_permission_pkey");

            entity.ToTable("auth_permission");

            entity.HasIndex(e => e.Code, "uq_auth_permission_code")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthPermissionId).HasColumnName("auth_permission_id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Service)
                .HasMaxLength(100)
                .HasColumnName("service");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        modelBuilder.Entity<AuthRole>(entity =>
        {
            entity.HasKey(e => e.AuthRoleId).HasName("auth_role_pkey");

            entity.ToTable("auth_role");

            entity.HasIndex(e => e.Name, "uq_auth_role_name")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthRoleId).HasColumnName("auth_role_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
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

        modelBuilder.Entity<AuthRolePermission>(entity =>
        {
            entity.HasKey(e => e.AuthRolePermissionId).HasName("auth_role_permission_pkey");

            entity.ToTable("auth_role_permission");

            entity.HasIndex(e => new { e.AuthRoleId, e.AuthPermissionId }, "uq_auth_role_permission")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthRolePermissionId).HasColumnName("auth_role_permission_id");
            entity.Property(e => e.AuthPermissionId).HasColumnName("auth_permission_id");
            entity.Property(e => e.AuthRoleId).HasColumnName("auth_role_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.HasKey(e => e.AuthSessionId).HasName("auth_session_pkey");

            entity.ToTable("auth_session");

            entity.HasIndex(e => new { e.AuthUserId, e.RevokedAt, e.ExpiresAt }, "idx_session_active").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.ExpiresAt, "idx_session_cleanup").HasFilter("((revoked_at IS NULL) AND (is_deleted = false))");

            entity.HasIndex(e => e.RefreshTokenHash, "idx_session_token").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.AuthUserId, "idx_session_user").HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthSessionId).HasColumnName("auth_session_id");
            entity.Property(e => e.AuthTenantId).HasColumnName("auth_tenant_id");
            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .HasColumnName("device_id");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(200)
                .HasColumnName("device_name");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address");
            entity.Property(e => e.IsCurrent)
                .HasDefaultValue(true)
                .HasColumnName("is_current");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
            entity.Property(e => e.RefreshTokenHash).HasColumnName("refresh_token_hash");
            entity.Property(e => e.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.RevokedReason)
                .HasMaxLength(100)
                .HasColumnName("revoked_reason");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
        });

        modelBuilder.Entity<AuthTenant>(entity =>
        {
            entity.HasKey(e => e.AuthTenantId).HasName("auth_tenant_pkey");

            entity.ToTable("auth_tenant");

            entity.HasIndex(e => e.Code, "uq_tenant_code")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthTenantId).HasColumnName("auth_tenant_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
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

            entity.HasIndex(e => new { e.Email, e.IsDeleted, e.IsActive, e.IsLocked }, "idx_auth_user_login");

            entity.HasIndex(e => e.Email, "uq_auth_user_email")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.AuthTenantId, e.Username }, "uq_auth_user_username")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.AuthTenantId).HasColumnName("auth_tenant_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
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

            entity.HasIndex(e => new { e.AuthUserId, e.AuthRoleId }, "uq_auth_user_role")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthUserRoleId).HasColumnName("auth_user_role_id");
            entity.Property(e => e.AuthRoleId).HasColumnName("auth_role_id");
            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .HasDefaultValueSql("'system'::character varying")
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_date");
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(50)
                .HasColumnName("deleted_by");
            entity.Property(e => e.DeletedDate).HasColumnName("deleted_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate).HasColumnName("updated_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
