using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FoxLife.Models.DBInfo.Forum;

namespace FoxLife.Models.DBInfo.SectionInfo
{
    internal class SectionDb
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountOfForums { get; set; }
        public int SectionOrder { get; set; }

        [NotMapped]
        public List<ForumDb> Forums { get; set; } = new();


        public SectionDb(string name)
        {
            this.Name = name;
        }
    }
}
