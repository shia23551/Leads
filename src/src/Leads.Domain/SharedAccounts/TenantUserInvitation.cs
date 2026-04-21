using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Leads.SharedAccounts;

public class TenantUserInvitation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxEmailLength = 256;
    public const int MaxTokenLength = 128;

    public Guid? TenantId { get; protected set; }

    public string Email { get; protected set; } = string.Empty;

    public string Token { get; protected set; } = string.Empty;

    public string? RoleNames { get; protected set; }

    public DateTime ExpireTime { get; protected set; }

    public DateTime? AcceptedTime { get; protected set; }

    public bool IsRevoked { get; protected set; }

    protected TenantUserInvitation()
    {
    }

    public TenantUserInvitation(
        Guid id,
        Guid tenantId,
        string email,
        string token,
        DateTime expireTime,
        string? roleNames = null)
        : base(id)
    {
        TenantId = tenantId;
        Email = Check.NotNullOrWhiteSpace(email, nameof(email), MaxEmailLength).Trim();
        Token = Check.NotNullOrWhiteSpace(token, nameof(token), MaxTokenLength).Trim();
        RoleNames = roleNames?.Trim();
        ExpireTime = expireTime;
    }

    public bool IsExpired(DateTime now)
    {
        return ExpireTime <= now;
    }

    public bool IsAvailable(DateTime now)
    {
        return !IsRevoked && !AcceptedTime.HasValue && !IsExpired(now);
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public void Accept(DateTime acceptedTime)
    {
        AcceptedTime = acceptedTime;
    }
}
