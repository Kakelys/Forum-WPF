using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Windows.Documents;
using FoxLife.Models.DBInfo.Ban;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.Post;
using FoxLife.Models.DBInfo.Role;
using FoxLife.Models.DBInfo.Topic;

namespace FoxLife.Models.DBInfo.User
{
    internal class UserDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Passwd {get; set; }
        public int CountOfMsg { get; set; }
        public int CountOfTopics { get; set; }
        public DateTime RegDate { get; set; }
        public int Role { get; set; }
        public int Img { get; set; }
        public bool IsBanned { get; set; }

        [NotMapped]
        public ICollection<Topic.TopicDb> StartUserTopics { get; set; }
        [NotMapped]
        public ICollection<Topic.TopicDb> LastUserTopics { get; set; }

        [NotMapped] 
        public ICollection<PostDb> Posts { get; set; }

        [NotMapped]
        public ICollection<BanDb> GetBans { get; set; }
        [NotMapped]
        public ICollection<BanDb> GiveBans { get; set; }

        [NotMapped]
        public ImgDb Avatar { get; set; }

        [NotMapped]
        public RoleDb RoleObj { get; set; }

        public UserDb() { }

        public UserDb(string name, string password)
        {
            this.Name = name;
            this.Passwd = password;
            RegDate = DateTime.UtcNow;
        }
    }
}
