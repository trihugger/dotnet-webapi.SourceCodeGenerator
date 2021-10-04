// Autogenerated by SourceCodeGenerator

using DN.WebApi.Application.Abstractions.Services.CRM;
using DN.WebApi.Domain.Constants;
using DN.WebApi.Infrastructure.Identity.Permissions;
using DN.WebApi.Shared.DTOs.CRM;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DN.WebApi.Bootstrapper.Controllers.v1
{
    public class CompanysController : BaseController
    {
        private readonly ICompanyService _service;

        public CompanysController(ICompanyService service)
        {
            _service = service;
        }

        [HttpGet]
        [MustHavePermission(Permissions.Companys.ListAll)]
        public async Task<IActionResult> GetListAsync(CompanyListFilter filter)
        {
            var companys = await _service.GetCompanysAsync(filter);
            return Ok(companys);
        }

        [HttpGet("dapper")]
        public async Task<IActionResult> GetDapperAsync(Guid id)
        {
            var companys = await _service.GetByIdUsingDapperAsync(id);
            return Ok(companys);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCompanyRequest request)
        {
            return Ok(await _service.CreateCompanyAsync(request));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateCompanyRequest request, Guid id)
        {
            return Ok(await _service.UpdateCompanyAsync(request, id));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var companyId = await _service.DeleteCompanyAsync(id);
            return Ok(companyId);
        }
    }
}