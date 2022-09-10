using System;
using System.ComponentModel.DataAnnotations.Schema;
using FoxLife.Models.DBInfo.Forum;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.SectionInfo;
using FoxLife.Models.DBInfo.Topic;

namespace FoxLife.Models.DBInfo.Forum
{
    internal class ForumDb
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int SectionId { get; set; }
        public int CountOfTopics { get; set; }
        public int CountOfMsg { get; set; }
        public int ForumOrder { get; set; }
        public DateTime? LastMsgTime { get; set; }
        public int LastMsgTopicId { get; set; }
        public int LastMsgUsrId { get; set; }
        public int ImgId { get; set; }

        [NotMapped]
        public ImgDb Img { get; set; }

        [NotMapped] 
        public TopicDb LastTopicDb { get; set; }

        [NotMapped] 
        public SectionDb Section { get; set; }

        public ForumDb()
        {
        }

        public ForumDb(string name, int sectionId)
        {
            Name = name;
            SectionId = sectionId;
        }
    }
}
