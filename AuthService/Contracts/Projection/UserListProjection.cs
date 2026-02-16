namespace AuthService.Contracts.Projection
{
    public sealed class UserListProjection
    {
        public int Id { get; init; }
        public string Username { get; init; } = "";
        public string Email { get; init; } = "";
        public string Tenant { get; init; } = "";
        public bool IsActive { get; init; }
        public bool IsLocked { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}
