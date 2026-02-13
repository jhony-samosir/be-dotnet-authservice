namespace AuthService.Common.Results;

public enum ErrorCode
{
    None = 0,

    // general
    Unknown,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,

    // auth
    InvalidCredential,
    TokenExpired,
    UserLocked,
}
