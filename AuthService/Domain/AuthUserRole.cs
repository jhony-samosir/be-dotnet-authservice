using System;
using System.Collections.Generic;

namespace AuthService.Domain;

public partial class AuthUserRole
{
    public int AuthUserRoleId { get; set; }

    public int AuthUserId { get; set; }

    public int AuthRoleId { get; set; }

    public DateTime AssignedDate { get; set; }

    public string? AssignedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
