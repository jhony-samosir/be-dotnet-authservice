using System;
using System.Collections.Generic;

namespace AuthService.Domain;

public partial class AuthPermission
{
    public int AuthPermissionId { get; set; }

    public string Code { get; set; } = null!;

    public string? Name { get; set; }

    public string? Service { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
