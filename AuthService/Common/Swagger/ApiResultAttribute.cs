using Microsoft.AspNetCore.Mvc;

namespace AuthService.Common.Swagger;

[AttributeUsage(AttributeTargets.Method)]
public class ApiResultAttribute(Type successType) : Attribute
{
    public Type SuccessType { get; } = successType;
}
