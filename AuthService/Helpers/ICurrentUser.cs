namespace AuthService.Helpers
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }

        int UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        string? LoginName { get; }
        bool IsAdmin { get; }
        string? RoleId { get; }
        string? Token { get; }
        string? Get(string claim);
    }
}
