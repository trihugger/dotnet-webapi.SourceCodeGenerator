// Autogenerated by SourceCodeGenerator

using System;

namespace DN.WebApi.Shared.DTOs.CRM
{
    public class CreatePersonRequest : IMustBeValid
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Guid CompanyId { get; set; }
    }
}