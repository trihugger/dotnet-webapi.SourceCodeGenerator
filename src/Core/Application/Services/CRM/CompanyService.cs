// Autogenerated by SourceCodeGenerator

using DN.WebApi.Application.Abstractions.Repositories;
using DN.WebApi.Application.Abstractions.Services.CRM;
using DN.WebApi.Application.Abstractions.Services.General;
using DN.WebApi.Application.Exceptions;
using DN.WebApi.Application.Specifications;
using DN.WebApi.Application.Wrapper;
using DN.WebApi.Domain.Entities.CRM;
using DN.WebApi.Domain.Enums;
using DN.WebApi.Shared.DTOs.CRM;
using Mapster;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DN.WebApi.Application.Services.CRM
{
    public class CompanyService : ICompanyService
    {
        private readonly IStringLocalizer<CompanyService> _localizer;
        private readonly IFileStorageService _file;
        private readonly IRepositoryAsync _repository;

        public CompanyService(IRepositoryAsync repository, IStringLocalizer<CompanyService> localizer, IFileStorageService file)
        {
            _repository = repository;
            _localizer = localizer;
            _file = file;
        }

        public async Task<Result<Guid>> CreateCompanyAsync(CreateCompanyRequest request)
        {
            var companyExists = await _repository.ExistsAsync<Company>(a => a.CompanyName == request.CompanyName);
            if (companyExists) throw new EntityAlreadyExistsException(string.Format(_localizer["company.alreadyexists"], request.CompanyName));

            var company = new Company(request.CompanyName);
            var companyId = await _repository.CreateAsync<Company>(company);
            await _repository.SaveChangesAsync();
            return await Result<Guid>.SuccessAsync(companyId);
        }

        public async Task<Result<Guid>> UpdateCompanyAsync(UpdateCompanyRequest request, Guid id)
        {
            var company = await _repository.GetByIdAsync<Company>(id, null);
            if (company == null) throw new EntityNotFoundException(string.Format(_localizer["company.notfound"], id));

            var updatedCompany = company.Update(request.CompanyName);
            await _repository.UpdateAsync<Company>(updatedCompany);
            await _repository.SaveChangesAsync();
            return await Result<Guid>.SuccessAsync(id);
        }

        public async Task<Result<Guid>> DeleteCompanyAsync(Guid id)
        {
            await _repository.RemoveByIdAsync<Company>(id);
            await _repository.SaveChangesAsync();
            return await Result<Guid>.SuccessAsync(id);
        }

        public async Task<PaginatedResult<CompanyDto>> GetCompanysAsync(CompanyListFilter filter)
        {
            var companys = await _repository.GetPaginatedListAsync<Company, CompanyDto>(filter.PageNumber, filter.PageSize, filter.OrderBy, filter.Search);
            return companys;
        }

        public async Task<Result<CompanyDto>> GetByIdUsingDapperAsync(Guid id)
        {
            var company = await _repository.QueryFirstOrDefaultAsync<Company>($"SELECT * FROM public.\"Companys\" WHERE \"Id\"  = '{id}' AND \"TenantKey\"='@tenantKey'");
            var mappedCompany = company.Adapt<CompanyDto>();
            return await Result<CompanyDto>.SuccessAsync(mappedCompany);
        }
    }
}