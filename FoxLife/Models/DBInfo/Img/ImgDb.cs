using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FoxLife.Models.DBInfo.User;

namespace FoxLife.Models.DBInfo.Img
{
    internal class ImgDb
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public byte[] Img { get; set; }

        public ICollection<User.UserDb> User { get; set; }
        public ICollection<Forum.ForumDb> Forums { get; set; }
    }
}
