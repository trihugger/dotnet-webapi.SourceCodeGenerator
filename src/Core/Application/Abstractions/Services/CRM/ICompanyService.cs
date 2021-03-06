// Autogenerated by SourceCodeGenerator

using DN.WebApi.Shared.DTOs.CRM;
using DN.WebApi.Application.Abstractions.Services;
using DN.WebApi.Application.Wrapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DN.WebApi.Application.Abstractions.Services.CRM
{
    public interface ICompanyService : ITransientService
    {
        Task<PaginatedResult<CompanyDto>> GetCompanysAsync(CompanyListFilter filter);
        Task<Result<CompanyDto>> GetByIdUsingDapperAsync(Guid id);
        Task<Result<Guid>> CreateCompanyAsync(CreateCompanyRequest request);
        Task<Result<Guid>> UpdateCompanyAsync(UpdateCompanyRequest request, Guid id);
        Task<Result<Guid>> DeleteCompanyAsync(Guid id);
    }
}