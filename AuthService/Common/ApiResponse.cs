
namespace AuthService.Common
{
    public record ApiResponse<T>(
        bool Success,
        string Message,
        T? Data
    );
}
