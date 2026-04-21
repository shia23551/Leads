using System;
using System.Linq;
using System.Threading.Tasks;
using Leads.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Leads.Companies;

[Authorize(LeadsPermissions.Companies.Default)]
public class CompanyAppService : LeadsAppService, ICompanyAppService
{
    private readonly IRepository<Company, Guid> _companyRepository;

    public CompanyAppService(IRepository<Company, Guid> companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public virtual async Task<CompanyDto> GetAsync(Guid id)
    {
        var company = await _companyRepository.GetAsync(id);
        return MapToDto(company);
    }

    public virtual async Task<PagedResultDto<CompanyDto>> GetListAsync(CompanyListInput input)
    {
        var query = await _companyRepository.GetQueryableAsync();
        query = ApplyFilters(query, input);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var companies = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(x => x.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<CompanyDto>(
            totalCount,
            companies.Select(MapToDto).ToList()
        );
    }

    [Authorize(LeadsPermissions.Companies.Create)]
    public virtual async Task<CompanyDto> CreateAsync(CreateUpdateCompanyDto input)
    {
        await EnsureTaxIdIsUniqueAsync(input.TaxId);

        var company = new Company(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.Name,
            input.TaxId,
            input.IndustryCategory,
            input.EmployeeCount ?? 0,
            input.CapitalAmount ?? 0,
            input.ContactPerson,
            input.PhoneNumber,
            input.Website,
            input.ShortName
        );

        await _companyRepository.InsertAsync(company, autoSave: true);

        return MapToDto(company);
    }

    [Authorize(LeadsPermissions.Companies.Edit)]
    public virtual async Task<CompanyDto> UpdateAsync(Guid id, CreateUpdateCompanyDto input)
    {
        await EnsureTaxIdIsUniqueAsync(input.TaxId, id);

        var company = await _companyRepository.GetAsync(id);
        company.Update(
            input.Name,
            input.TaxId,
            input.IndustryCategory,
            input.EmployeeCount ?? 0,
            input.CapitalAmount ?? 0,
            input.ContactPerson,
            input.PhoneNumber,
            input.Website,
            input.ShortName
        );

        await _companyRepository.UpdateAsync(company, autoSave: true);

        return MapToDto(company);
    }

    [Authorize(LeadsPermissions.Companies.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _companyRepository.DeleteAsync(id);
    }

    private IQueryable<Company> ApplyFilters(IQueryable<Company> query, CompanyListInput input)
    {
        var filter = input.Filter?.Trim();
        var name = input.Name?.Trim();
        var shortName = input.ShortName?.Trim();
        var taxId = input.TaxId?.Trim();
        var industryCategory = input.IndustryCategory?.Trim();

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(filter), x =>
                x.Name.Contains(filter!) ||
                (x.ShortName != null && x.ShortName.Contains(filter!)) ||
                (x.TaxId != null && x.TaxId.Contains(filter!)) ||
                (x.IndustryCategory != null && x.IndustryCategory.Contains(filter!)) ||
                (x.ContactPerson != null && x.ContactPerson.Contains(filter!)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(filter!)))
            .WhereIf(!string.IsNullOrWhiteSpace(name), x => x.Name.StartsWith(name!))
            .WhereIf(!string.IsNullOrWhiteSpace(shortName), x => x.ShortName != null && x.ShortName.StartsWith(shortName!))
            .WhereIf(!string.IsNullOrWhiteSpace(taxId), x => x.TaxId != null && x.TaxId.StartsWith(taxId!))
            .WhereIf(!string.IsNullOrWhiteSpace(industryCategory), x => x.IndustryCategory != null && x.IndustryCategory.Contains(industryCategory!));

        query = ApplyEmployeeCountFilter(query, input.EmployeeCountLowerBoundOperator, input.EmployeeCountLowerBoundValue);
        query = ApplyEmployeeCountFilter(query, input.EmployeeCountUpperBoundOperator, input.EmployeeCountUpperBoundValue);

        query = ApplyCapitalAmountFilter(query, input.CapitalAmountLowerBoundOperator, input.CapitalAmountLowerBoundValue);
        query = ApplyCapitalAmountFilter(query, input.CapitalAmountUpperBoundOperator, input.CapitalAmountUpperBoundValue);

        return query;
    }

    private static IQueryable<Company> ApplyEmployeeCountFilter(
        IQueryable<Company> query,
        ComparisonOperator? op,
        int? value)
    {
        if (!op.HasValue || !value.HasValue)
        {
            return query;
        }

        return op.Value switch
        {
            ComparisonOperator.Equal => query.Where(x => x.EmployeeCount == value.Value),
            ComparisonOperator.GreaterThan => query.Where(x => x.EmployeeCount > value.Value),
            ComparisonOperator.LessThan => query.Where(x => x.EmployeeCount < value.Value),
            _ => query
        };
    }

    private static IQueryable<Company> ApplyCapitalAmountFilter(
        IQueryable<Company> query,
        ComparisonOperator? op,
        decimal? value)
    {
        if (!op.HasValue || !value.HasValue)
        {
            return query;
        }

        return op.Value switch
        {
            ComparisonOperator.Equal => query.Where(x => x.CapitalAmount == value.Value),
            ComparisonOperator.GreaterThan => query.Where(x => x.CapitalAmount > value.Value),
            ComparisonOperator.LessThan => query.Where(x => x.CapitalAmount < value.Value),
            _ => query
        };
    }

    private async Task EnsureTaxIdIsUniqueAsync(string? taxId, Guid? companyId = null)
    {
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return;
        }

        var normalizedTaxId = taxId.Trim();
        var query = await _companyRepository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(query, x => x.TaxId == normalizedTaxId && x.Id != companyId);

        if (exists)
        {
            throw new UserFriendlyException(L["TaxIdAlreadyExists", normalizedTaxId]);
        }
    }

    private static CompanyDto MapToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            TenantId = company.TenantId,
            Name = company.Name,
            ShortName = company.ShortName,
            TaxId = company.TaxId,
            IndustryCategory = company.IndustryCategory,
            EmployeeCount = company.EmployeeCount,
            CapitalAmount = company.CapitalAmount,
            ContactPerson = company.ContactPerson,
            PhoneNumber = company.PhoneNumber,
            Website = company.Website,
            CreationTime = company.CreationTime,
            CreatorId = company.CreatorId,
            LastModificationTime = company.LastModificationTime,
            LastModifierId = company.LastModifierId
        };
    }
}
