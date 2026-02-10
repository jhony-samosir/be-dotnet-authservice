using System;
using System.Collections.Generic;

namespace AuthService.Domain;

public partial class AuthMenuPermission
{
    public int AuthMenuPermissionId { get; set; }

    public int AuthMenuId { get; set; }

    public int AuthPermissionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
