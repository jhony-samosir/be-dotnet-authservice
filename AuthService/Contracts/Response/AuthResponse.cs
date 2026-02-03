namespace AuthService.Contracts.Response
{
    public record AuthResponse(
        string AccessToken,
        DateTime ExpiresAt
    );
}
