namespace MiniBooking.Data;

public interface ITenantContext { Guid OrgId { get; set; } }

public sealed class TenantContext : ITenantContext
{
    public static readonly Guid DefaultOrgId = new("22222222-2222-2222-2222-222222222222");
    public const string CookieName = "org_key";
    public Guid OrgId { get; set; } = DefaultOrgId;
}
