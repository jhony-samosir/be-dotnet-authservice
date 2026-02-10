using System;
using System.Collections.Generic;

namespace AuthService.Domain;

public partial class AuthRefreshToken
{
    public int AuthRefreshTokenId { get; set; }

    public int AuthUserId { get; set; }

    public string Token { get; set; } = null!;

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public DateTime ExpiredDate { get; set; }

    public DateTime? RevokedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
