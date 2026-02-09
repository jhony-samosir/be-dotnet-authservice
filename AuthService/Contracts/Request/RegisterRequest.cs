using System.Collections.Generic;

namespace AuthService.Contracts.Request
{
    /// <summary>
    /// Registration request contract.
    /// Uses email + password and optional roles.
    /// Email is mapped to the existing Username column in the database.
    /// </summary>
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

