using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    public sealed class RoleNotFoundException : NotFoundException
    {
        public RoleNotFoundException( string roleName) : base($"The Role with Name: {roleName} doesn't exist in the database.")
        {

        }
    }
}
