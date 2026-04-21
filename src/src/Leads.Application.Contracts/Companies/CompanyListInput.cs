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

    public ComparisonOperator? EmployeeCountLowerBoundOperator { get; set; }

    public int? EmployeeCountLowerBoundValue { get; set; }

    public ComparisonOperator? EmployeeCountUpperBoundOperator { get; set; }

    public int? EmployeeCountUpperBoundValue { get; set; }

    public ComparisonOperator? CapitalAmountLowerBoundOperator { get; set; }

    public decimal? CapitalAmountLowerBoundValue { get; set; }

    public ComparisonOperator? CapitalAmountUpperBoundOperator { get; set; }

    public decimal? CapitalAmountUpperBoundValue { get; set; }
}
