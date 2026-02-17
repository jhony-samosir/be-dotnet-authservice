using System;
using System.Collections.Generic;

namespace AuthService.Domain;

public partial class AuthUserRole
{
    public int AuthUserRoleId { get; set; }

    public int AuthUserId { get; set; }

    public int AuthRoleId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
