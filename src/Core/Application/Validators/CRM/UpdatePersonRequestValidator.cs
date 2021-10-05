// Autogenerated by SourceCodeGenerator

using System;
using DN.WebApi.Shared.DTOs.CRM;
using FluentValidation;

namespace DN.WebApi.Application.Validators.CRM
{
    public class UpdatePersonRequestValidator : CustomValidator<UpdatePersonRequest>
    {
        public UpdatePersonRequestValidator()
        {
            RuleFor(p => p.Name).MaximumLength(75).NotEmpty();
            RuleFor(p => p.Age).GreaterThanOrEqualTo(0).NotEmpty();
            RuleFor(p => p.DateOfBirth).GreaterThanOrEqualTo(DateTime.Today);
            RuleFor(p => p.CompanyId);
            RuleFor(p => p.Id).NotEmpty();
        }
    }
}