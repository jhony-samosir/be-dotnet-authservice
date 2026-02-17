using System;
using System.Collections.Generic;

namespace AuthService.Domain;

public partial class AuthSession
{
    public long AuthSessionId { get; set; }

    public int AuthUserId { get; set; }

    public int AuthTenantId { get; set; }

    public string RefreshTokenHash { get; set; } = null!;

    public string? DeviceId { get; set; }

    public string? DeviceName { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public bool IsCurrent { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
