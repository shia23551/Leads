using System;
using Volo.Abp.Application.Dtos;

namespace Leads.Companies;

public class CompanyDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public string? TaxId { get; set; }

    public string? IndustryCategory { get; set; }

    public int EmployeeCount { get; set; }

    public decimal CapitalAmount { get; set; }

    public string? ContactPerson { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Website { get; set; }
}
