using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Xps.Serialization;
using FoxLife.Models.DBInfo.Img;
using FoxLife.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.User
{
    internal class UserContext : ForumDbContext
    {
        public static bool Update(int id, int roleId)
        {
            using var db = new UserContext();
            try
            {
                var user = db.User.FirstOrDefault(u=>u.Id==id);
                if (user == null) return false;
                user.Role = roleId;
                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Update(int id, byte[]? img)
        {
            if (img == null) return false;
            using var db = new UserContext();
            try
            {
                var trans = db.Database.BeginTransaction();
                try
                {
                    var user = db.User.AsQueryable().Include(u => u.Img).FirstOrDefault(u => u.Id == id);
                    if (user == null) throw new Exception();
                    if (user.Avatar.Id == -1)
                    {
                        var newImg = new ImgDb
                        {
                            Img = img
                        };
                        db.Img.Add(newImg);
                        db.SaveChanges();

                        user.Img = newImg.Id;
                        DBInfo.User.User.Update( newImg.Id);
                    }
                    else
                    {
                        user.Avatar.Img = img;
                    }

                    db.SaveChanges();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Update(SecureString? oldPassword,SecureString? newPassword)
        {
            using var db = new UserContext();
            try
            {
                var user = db.User.AsQueryable().FirstOrDefault(u => u.Id == DBInfo.User.User.Id);
                if (user == null)
                {
                    MainViewModel.Message("UserContextLoadPasswordError", MessageViewModel.MessageType.Error);
                    return false;
                }

                var oldPasswd = Password.Hashing(oldPassword);
                if (oldPasswd.Equals(user.Passwd))
                {
                    var newPasswd = Password.Hashing(newPassword);
                    user.Passwd = newPasswd;
                    db.SaveChanges();
                }
                else
                {
                    MainViewModel.Message("UserContextNoMatchPasswordError", MessageViewModel.MessageType.Error);
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static UserDb? GetUser(int id, bool includingBans = false)
        {
            using var db = new UserContext();
            try
            {
                UserDb? user = null;

                if (includingBans)
                {
                    user = db.User
                        .AsNoTracking()
                        .Include(u => u.Avatar)
                        .Include(u => u.RoleObj)
                        .Include(u => u.GetBans)
                        .ThenInclude(b=>b.Admin)
                        .FirstOrDefault(x => x.Id == id);
                }
                else
                {
                    user = db.User
                        .AsNoTracking()
                        .Include(u => u.Avatar)
                        .Include(u => u.RoleObj)
                        .FirstOrDefault(x => x.Id == id);
                }

                if (user != null)
                    user.Passwd = "";
                return user;
            }
            catch
            {
                return null;
            }
        }

        public static UserDb? GetUser(string name, bool includingBans = false)
        {
            using var db = new UserContext();
            try
            {
                UserDb? user = null;

                if (includingBans)
                {
                    user = db.User.AsNoTracking()
                        .Include(u => u.Avatar)
                        .Include(u => u.RoleObj)
                        .Include(u=>u.GetBans)
                        .ThenInclude(b=>b.Admin)
                        .FirstOrDefault(u => u.Name.Equals(name));
                }
                else
                {
                    user = db.User.AsNoTracking()
                        .Include(u => u.Avatar)
                        .Include(u => u.RoleObj)
                        .FirstOrDefault(u => u.Name.Equals(name));
                }

                if(user!=null)
                    user.Passwd = "";
                return user;
            }
            catch
            {
                return null;
            }
        }

        public static bool Add(string name, SecureString? passwd)
        {
            using var db = new UserContext();
            try
            {
                var password = Password.Hashing(passwd);
                var temp = new UserDb(name, password);

                db.Add(temp);
                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static UserDb? CheckLogin(string name, SecureString? passwd)
        {
            using UserContext db = new();
            try
            {
                var password = Password.Hashing(passwd);
                var user = db.User.AsNoTracking().FirstOrDefault(x => x.Name == name && x.Passwd == password);
                if (user == null) return null;

                return user;
            }
            catch
            {
                return null;
            }
        }

        public static UserDb? CheckLogin(int userId, SecureString? passwd)
        {
            using UserContext db = new();
            try
            {
                var password = Password.Hashing(passwd);
                var user = db.User.AsNoTracking().FirstOrDefault(x => x.Id == userId&& x.Passwd == password);
                if (user == null) return null;

                return user;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsExists(string name)
        {
            using UserContext db = new();
            try
            {
                var temp = db.User.AsNoTracking().Where(x => x.Name == name);
                if (temp.Any()) return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDb>().Property(u => u.Img).HasDefaultValue(-1);
            modelBuilder.Entity<UserDb>().Property(u => u.Role).HasDefaultValue(10);
            modelBuilder.Entity<UserDb>().Property(u => u.RegDate).HasDefaultValue(DateTime.UtcNow.ToString("yyyy - MM - dd HH: mm:ss.fff"));
            base.OnModelCreating(modelBuilder);
        }
    }
}

