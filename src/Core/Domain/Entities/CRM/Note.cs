using DN.WebApi.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DN.WebApi.Domain.Entities.CRM
{
    public class Note : AuditableEntity, IMustHaveTenant
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public Guid PersonId { get; set; }
        public Person Person { get; set; }
        public string TenantKey { get; set; }

        public Note(string subject, string message, Guid personid)
        {
            Subject = subject;
            Message = message;
            PersonId = personid;
        }

        public Note Update(string subject, string message, Guid personid)
        {
            if (!Subject.Equals(subject)) Subject = subject;
            if (!Message.Equals(message)) Message = message;
            if (!PersonId.Equals(personid)) PersonId = personid;
            return this;
        }
    }
}