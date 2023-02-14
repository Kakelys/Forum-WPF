using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.Role
{
    internal class RoleContext:ForumDbContext
    {
        public static List<RoleDb>? GetRoleList()
        {
            using var db = new RoleContext();
            try
            {
                return db.Role.AsNoTracking().ToList();
            }
            catch
            {
                return null;
            }

            return null;
        }

    }
}
