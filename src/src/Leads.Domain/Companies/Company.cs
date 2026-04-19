using System;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Leads.Companies;

public class Company : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string Name { get; protected set; } = string.Empty;

    public string? ShortName { get; protected set; }

    public string? TaxId { get; protected set; }

    public string? IndustryCategory { get; protected set; }

    public int EmployeeCount { get; protected set; }

    public decimal CapitalAmount { get; protected set; }

    public string? ContactPerson { get; protected set; }

    public string? PhoneNumber { get; protected set; }

    public string? Website { get; protected set; }

    protected Company()
    {
    }

    public Company(
        Guid id,
        Guid? tenantId,
        string name,
        string? taxId,
        string? industryCategory,
        int employeeCount,
        decimal capitalAmount,
        string? contactPerson,
        string? phoneNumber,
        string? website,
        string? shortName = null)
        : base(id)
    {
        TenantId = tenantId;
        Update(name, taxId, industryCategory, employeeCount, capitalAmount, contactPerson, phoneNumber, website, shortName);
    }

    public void Update(
        string name,
        string? taxId,
        string? industryCategory,
        int employeeCount,
        decimal capitalAmount,
        string? contactPerson,
        string? phoneNumber,
        string? website,
        string? shortName = null)
    {
        if (employeeCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(employeeCount));
        }

        if (capitalAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capitalAmount));
        }

        Name = Check.NotNullOrWhiteSpace(name, nameof(name), CompanyConsts.MaxNameLength).Trim();
        TaxId = NormalizeTaxId(taxId);
        ShortName = NormalizeNullable(shortName, CompanyConsts.MaxShortNameLength, nameof(shortName));
        IndustryCategory = NormalizeNullable(industryCategory, CompanyConsts.MaxIndustryCategoryLength, nameof(industryCategory));
        ContactPerson = NormalizeNullable(contactPerson, CompanyConsts.MaxContactPersonLength, nameof(contactPerson));
        PhoneNumber = NormalizeNullable(phoneNumber, CompanyConsts.MaxPhoneNumberLength, nameof(phoneNumber));
        Website = NormalizeNullable(website, CompanyConsts.MaxWebsiteLength, nameof(website));
        EmployeeCount = employeeCount;
        CapitalAmount = capitalAmount;
    }

    private static string? NormalizeNullable(string? value, int maxLength, string parameterName)
    {
        var normalizedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return null;
        }

        return Check.Length(normalizedValue, parameterName, maxLength);
    }

    private static string? NormalizeTaxId(string? taxId)
    {
        var normalizedTaxId = taxId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTaxId))
        {
            return null;
        }

        Check.Length(normalizedTaxId, nameof(taxId), CompanyConsts.TaxIdLength, CompanyConsts.TaxIdLength);

        if (!Regex.IsMatch(normalizedTaxId, "^[0-9]{8}$"))
        {
            throw new BusinessException("Leads:InvalidTaxId");
        }

        return normalizedTaxId;
    }
}
