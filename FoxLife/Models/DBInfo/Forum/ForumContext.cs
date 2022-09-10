using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoxLife.Models.DBInfo.Img;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.Forum
{
    internal class ForumContext : ForumDbContext
    {
        public static bool Add(ForumDb forum, byte[]? imgData)
        {
            using ForumContext db = new();
            try
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    var section = db.Section.AsQueryable().First(x => x.Id == forum.SectionId);
                    section.CountOfForums += 1;

                    var img = new ImgDb
                    {
                        Img = imgData
                    };
                    db.Add(img);
                    db.SaveChanges();

                    forum.ImgId = img.Id;

                    db.Forum.Add(forum);
                    db.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Delete(int id)
        {
            using var db = new ForumContext();
            try
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    var forum = db.Forum.AsQueryable().Include(f => f.Img)
                        .FirstOrDefault(f => f.Id == id);
                    if (forum == null) 
                        throw new Exception();

                    db.Forum.Remove(forum);
                    db.SaveChanges();
                    db.Img.Remove(forum.Img);

                    db.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Update(int id, string? name, byte[]? img)
        {
            using var db = new ForumContext();

            try
            {
                var forum = db.Forum.AsQueryable().Include(f=>f.Img).FirstOrDefault(f => f.Id == id);
                if (forum == null) 
                    throw new Exception();

                if (img != null) forum.Img.Img = img;
                if(name!=null)forum.Name = name;
                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
