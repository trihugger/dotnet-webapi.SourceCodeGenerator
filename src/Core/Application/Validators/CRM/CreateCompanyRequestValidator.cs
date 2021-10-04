// Autogenerated by SourceCodeGenerator

using System;
using DN.WebApi.Shared.DTOs.CRM;
using FluentValidation;

namespace DN.WebApi.Application.Validators.CRM
{
    public class CreateCompanyRequestValidator : CustomValidator<CreateCompanyRequest>
    {
        public CreateCompanyRequestValidator()
        {
            RuleFor(p => p.CompanyName).MaximumLength(75).NotEmpty();
            RuleFor(p => p.Id).NotEmpty();
        }
    }
}