using System;
using System.Collections.Generic;

namespace AuthService.Domain;

public partial class AuthMenu
{
    public int AuthMenuId { get; set; }

    public string Name { get; set; } = null!;

    public string? Path { get; set; }

    public string? Icon { get; set; }

    public int? ParentId { get; set; }

    public int? SortOrder { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? DeletedBy { get; set; }
}
