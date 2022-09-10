using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FoxLife.Models.DBInfo.User;

namespace FoxLife.Models.DBInfo.Role
{
    internal class RoleDb
    {
        public int Id { get; init; }
        public string RoleName { get; init; }


        [NotMapped]
        public List<User.UserDb> Users { get; set; }
    }
}
