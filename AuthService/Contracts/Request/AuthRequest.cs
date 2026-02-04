namespace AuthService.Contracts.Request
{
    /// <summary>
    /// Login request contract.
    /// Uses email + password as requested.
    /// The email is mapped to the existing Username column in the database.
    /// </summary>
    public class AuthRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}

