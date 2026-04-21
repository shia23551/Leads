using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Leads.SharedAccounts;

public class TenantUserMembership : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid UserId { get; protected set; }

    public bool IsActive { get; protected set; }

    protected TenantUserMembership()
    {
    }

    public TenantUserMembership(Guid id, Guid tenantId, Guid userId, bool isActive = true)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        IsActive = isActive;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
