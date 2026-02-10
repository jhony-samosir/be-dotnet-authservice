using System;
using System.Collections.Generic;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public partial class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public virtual DbSet<AuthMenu> AuthMenu { get; set; }

    public virtual DbSet<AuthMenuPermission> AuthMenuPermission { get; set; }

    public virtual DbSet<AuthPermission> AuthPermission { get; set; }

    public virtual DbSet<AuthRefreshToken> AuthRefreshToken { get; set; }

    public virtual DbSet<AuthRole> AuthRole { get; set; }

    public virtual DbSet<AuthRolePermission> AuthRolePermission { get; set; }

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

            entity.HasIndex(e => e.Service, "idx_auth_permission_service").HasFilter("(is_deleted = false)");

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

        modelBuilder.Entity<AuthRefreshToken>(entity =>
        {
            entity.HasKey(e => e.AuthRefreshTokenId).HasName("auth_refresh_token_pkey");

            entity.ToTable("auth_refresh_token");

            entity.HasIndex(e => e.ExpiredDate, "idx_refresh_token_cleanup").HasFilter("(revoked_date IS NULL)");

            entity.HasIndex(e => new { e.Token, e.ExpiredDate, e.RevokedDate }, "idx_refresh_token_lookup").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.AuthUserId, "idx_refresh_token_user").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Token, "uq_refresh_token_token")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthRefreshTokenId).HasColumnName("auth_refresh_token_id");
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

            entity.HasIndex(e => e.IsDeleted, "idx_auth_role_softdelete");

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

            entity.HasIndex(e => e.AuthPermissionId, "idx_auth_role_perm_perm").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.AuthRoleId, "idx_auth_role_perm_role").HasFilter("(is_deleted = false)");

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

            entity.HasIndex(e => e.AuthTenantId, "idx_auth_user_tenant").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.Email, "uq_auth_user_email")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.HasIndex(e => new { e.AuthTenantId, e.Username }, "uq_auth_user_username")
                .IsUnique()
                .HasFilter("(is_deleted = false)");

            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.AuthTenantId)
                .HasDefaultValue(0)
                .HasColumnName("auth_tenant_id");
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

            entity.HasIndex(e => e.AuthRoleId, "idx_auth_user_role_role").HasFilter("(is_deleted = false)");

            entity.HasIndex(e => e.AuthUserId, "idx_auth_user_role_user").HasFilter("(is_deleted = false)");

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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
