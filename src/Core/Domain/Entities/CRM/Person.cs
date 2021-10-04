using DN.WebApi.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DN.WebApi.Domain.Entities.CRM
{
    public class Person : AuditableEntity, IMustHaveTenant
    {
        public string Name { get; private set; }
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TenantKey { get; set; }
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public List<Note> Notes { get; set; }

        public Person(string name, int age, DateTime dateofbirth, Guid companyid)
        {
            Name = name;
            Age = age;
            DateOfBirth = dateofbirth;
            CompanyId = companyid;
        }

        public Person Update(string name, int age, DateTime dateofbirth, Guid companyid)
        {
            if (!Name.Equals(name)) Name = name;
            if (Age != age) Age = age;
            if (!DateOfBirth.Equals(dateofbirth)) DateOfBirth = dateofbirth;
            if (!CompanyId.Equals(companyid)) CompanyId = companyid;
            return this;
        }
    }
}