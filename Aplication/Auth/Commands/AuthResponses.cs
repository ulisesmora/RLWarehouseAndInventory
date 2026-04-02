using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Auth.Commands
{
    
        public record AuthResponses(string Token, string FullName, string Role, Guid OrganizationId);
    
}
