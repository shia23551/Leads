using System.ComponentModel.DataAnnotations;

namespace Leads.Companies;

public class CreateUpdateCompanyDto
{
    [Required]
    [StringLength(CompanyConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CompanyConsts.MaxShortNameLength)]
    public string? ShortName { get; set; }

    [StringLength(CompanyConsts.TaxIdLength)]
    [RegularExpression("^$|^[0-9]{8}$")]
    public string? TaxId { get; set; }

    [StringLength(CompanyConsts.MaxIndustryCategoryLength)]
    public string? IndustryCategory { get; set; }

    [Range(0, int.MaxValue)]
    public int? EmployeeCount { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal? CapitalAmount { get; set; }

    [StringLength(CompanyConsts.MaxContactPersonLength)]
    public string? ContactPerson { get; set; }

    [StringLength(CompanyConsts.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; set; }

    [StringLength(CompanyConsts.MaxWebsiteLength)]
    [Url]
    public string? Website { get; set; }
}
