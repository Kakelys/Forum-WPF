using System;
using System.ComponentModel.DataAnnotations.Schema;
using FoxLife.Models.DBInfo.User;

namespace FoxLife.Models.DBInfo.Post
{
    internal class PostDb
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string MsgTxt { get; set; }
        public int TopicId { get; set; }
        public DateTime MsgTime { get; set; }
        public int SenderId { get; set; }
        public int Ancestor { get; set; }

        [NotMapped]
        public User.UserDb UserDb { get; set; }

        [NotMapped] 
        public Topic.TopicDb TopicDb { get; set; }

        public PostDb()
        {
        }

        public PostDb(string message, int topicId, int senderId, int ancestorId)
        {
            MsgTxt = message;
            this.TopicId = topicId;
            this.SenderId = senderId;
            Ancestor = ancestorId;
            MsgTime = DateTime.UtcNow;
        }
    }
}
