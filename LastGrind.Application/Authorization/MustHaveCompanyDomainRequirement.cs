using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastGrind.Application.Authorization
{
    public class MustHaveCompanyDomainRequirement:IAuthorizationRequirement
    {
        public string DomainName;
        public MustHaveCompanyDomainRequirement(string domainName)
        {
            DomainName = domainName;
        }
    }
}
