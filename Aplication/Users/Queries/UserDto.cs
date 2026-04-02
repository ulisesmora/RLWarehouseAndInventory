using Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Users.Queries
{
    public class UserDto
    {
        public Guid Id { get; set; }


         public string FullName { get; set; }
        public string Email { get; set; }

         public string Role { get; set; }


        public Guid? RestrictedWarehouseId { get; set; }


        public bool IsActive { get; set; }
        public  DateTime CreatedAt { get; set; }
    }
}
