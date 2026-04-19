using Volo.Abp.Application.Dtos;

namespace Leads.Companies;

public class CompanyListInput : PagedResultRequestDto
{
    public CompanyListInput()
    {
        MaxResultCount = 50;
    }

    public string? Filter { get; set; }

    public string? Name { get; set; }

    public string? ShortName { get; set; }

    public string? TaxId { get; set; }

    public string? IndustryCategory { get; set; }

    public ComparisonOperator? EmployeeCountOperator { get; set; }

    public int? EmployeeCountValue { get; set; }

    public ComparisonOperator? CapitalAmountOperator { get; set; }

    public decimal? CapitalAmountValue { get; set; }
}
