using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FoxLife.Models.DBInfo.Post;
using FoxLife.Models.DBInfo.User;

namespace FoxLife.Models.DBInfo.Topic
{
    internal class TopicDb
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ForumId { get; set; }
        public int CountOfMsg { get; set; }
        public int StartMsgUsrId { get; set; }
        public int LastMsgUsrId { get; set; }
        public DateTime LastMsgTime { get; set; }
        public DateTime StartMsgTime { get; set; }
        public string StartMsgTXT { get; set; }
        public bool IsClosed { get; set; }
        public bool IsPinned { get; set; }


        [NotMapped]
        public UserDb StartUserDb { get; set; }
        [NotMapped]
        public UserDb LastUserDb { get; set; }

        [NotMapped]
        public virtual Forum.ForumDb ForumDb { get; set; }

        [NotMapped]
        public ICollection<PostDb> Posts { get; set; } 

        public TopicDb()
        {
        }

        public TopicDb(string name, string message, int forumId)
        {
            this.Name = name;
            StartMsgTXT = message;
            this.ForumId = forumId;
            StartMsgUsrId = User.User.Id;
            LastMsgUsrId = User.User.Id;
            LastMsgTime = DateTime.UtcNow;
        }
    }
}
