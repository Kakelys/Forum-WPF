using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoxLife.Models.DBInfo.User;

namespace FoxLife.Models.DBInfo.Ban
{
    internal class BanDb
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public int AdminId { get; set; }
        public string Reason { get; set; }
        public DateTime BanTime { get; set; }
        public DateTime UnbanTime { get; set; }
        public bool IsPerm { get; set; }
        public bool IsActive { get; set; }

        [NotMapped]
        public UserDb User;

        [NotMapped]
        public UserDb Admin;
    }
}
