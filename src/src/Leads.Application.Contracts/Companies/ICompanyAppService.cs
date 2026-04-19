using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Leads.Companies;

public interface ICompanyAppService : IApplicationService
{
    Task<CompanyDto> GetAsync(Guid id);

    Task<PagedResultDto<CompanyDto>> GetListAsync(CompanyListInput input);

    Task<CompanyDto> CreateAsync(CreateUpdateCompanyDto input);

    Task<CompanyDto> UpdateAsync(Guid id, CreateUpdateCompanyDto input);

    Task DeleteAsync(Guid id);
}
