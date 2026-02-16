namespace AuthService.Contracts.Projection
{
    public sealed class TenantListProjection
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string Code { get; init; } = "";
        public DateTime CreatedDate { get; init; }
    }
}
