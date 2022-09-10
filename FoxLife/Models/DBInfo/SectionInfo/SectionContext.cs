using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using FoxLife.Models.DBInfo.Forum;
using FoxLife.View;
using FoxLife.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.SectionInfo
{
    internal class SectionContext : ForumDbContext
    {
        public static bool Add(SectionDb sectionDb)
        {
            using SectionContext db = new();
            try
            {
                db.Section.Add(sectionDb);
                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Update(int id, string name)
        {
            using var db = new SectionContext();
            try
            {
                var section = db.Section.AsQueryable().FirstOrDefault(s=>s.Id==id);
                if (section == null) return false;

                section.Name = name;
                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Delete(int id)
        {
            using var db = new SectionContext();
            try
            {
                var section = db.Section.AsQueryable().FirstOrDefault(s => s.Id == id);
                if (section == null) return false;

                db.Section.Remove(section);
                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static List<SectionDb> GetSections()
        {
            
           using var db = new SectionContext();
           try
           {
               return db.Section.AsNoTracking()
                   .Include(s => s.Forums)
                   .ThenInclude(f => f.Img)
                   .Include(s => s.Forums)
                   .ThenInclude(f => f.LastTopicDb.LastUserDb)
                   .ToList();
           }
           catch
           {
               MainViewModel.Message("SectionContextGetError", MessageViewModel.MessageType.Error);
                return null;
           }
        }
    }
}
